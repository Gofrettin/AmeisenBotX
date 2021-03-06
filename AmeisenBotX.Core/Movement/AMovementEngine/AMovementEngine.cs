﻿using AmeisenBotX.Core.Character.Objects;
using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Movement.Enums;
using AmeisenBotX.Core.Movement.Objects;
using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmeisenBotX.Core.Movement.AMovementEngine
{
    public class AMovementEngine : IMovementEngine
    {
        public AMovementEngine(AmeisenBotConfig config)
        {
            Config = config;

            FindPathEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
            RefreshPathEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));
            DistanceMovedCheckEvent = new TimegatedEvent(TimeSpan.FromMilliseconds(500));

            PathQueue = new Queue<Vector3>();
            PlacesToAvoidList = new List<(Vector3 position, float radius, DateTime until)>();

            PlayerVehicle = new BasicVehicle();
        }

        public float CurrentSpeed { get; private set; }

        public bool IsAllowedToMove => DateTime.UtcNow > MovementBlockedUntil;

        public bool IsMoving { get; private set; }

        public bool IsUnstucking { get; private set; }

        public DateTime LastMovement { get; private set; }

        public Vector3 LastPosition { get; private set; }

        public IEnumerable<Vector3> Path => PathQueue;

        public IEnumerable<(Vector3 position, float radius)> PlacesToAvoid => PlacesToAvoidList.Where(e => DateTime.UtcNow <= e.until).Select(e => (e.position, e.radius));

        public MovementAction Status { get; private set; }

        public Vector3 UnstuckTarget { get; private set; }

        private AmeisenBotConfig Config { get; }

        private TimegatedEvent DistanceMovedCheckEvent { get; }

        private TimegatedEvent FindPathEvent { get; }

        private Vector3 LastTargetPosition { get; set; }

        private DateTime MovementBlockedUntil { get; set; }

        private Queue<Vector3> PathQueue { get; set; }

        private List<(Vector3 position, float radius, DateTime until)> PlacesToAvoidList { get; set; }

        private BasicVehicle PlayerVehicle { get; set; }

        private TimegatedEvent RefreshPathEvent { get; }

        private bool TriedToMountUp { get; set; }

        public void AvoidPlace(Vector3 position, float radius, TimeSpan timeSpan)
        {
            DateTime now = DateTime.UtcNow;

            PlacesToAvoidList.Add((position, radius, now + timeSpan));
            PlacesToAvoidList.RemoveAll(e => now > e.until);
        }

        public void Execute()
        {
            if (IsUnstucking && UnstuckTarget.GetDistance(WowInterface.I.Player.Position) < 2.0f)
            {
                IsUnstucking = false;
            }

            if (PathQueue.Count > 0)
            {
                Vector3 currentNode = IsUnstucking ? UnstuckTarget : PathQueue.Peek();
                float distanceToNode = WowInterface.I.Player.Position.GetDistance2D(currentNode);

                if (distanceToNode > 1.0f)
                {
                    if (!TriedToMountUp)
                    {
                        float distance = WowInterface.I.Player.Position.GetDistance(PathQueue.Last());

                        // try to mount only once per path
                        if (distance > 40.0f
                            && !WowInterface.I.Player.IsInCombat
                            && !WowInterface.I.Player.IsGhost
                            && !WowInterface.I.Player.IsMounted
                            && WowInterface.I.Player.IsOutdoors
                            && WowInterface.I.CharacterManager.Mounts != null
                            && WowInterface.I.CharacterManager.Mounts.Any())
                        {
                            MountUp();
                            TriedToMountUp = true;
                        }
                    }

                    // we need to move to the node
                    if (IsAllowedToMove && !WowInterface.I.Player.IsCasting)
                    {
                        PlayerVehicle.Update(MoveCharacter, Status, currentNode);
                    }
                }
                else
                {
                    // we are at the node
                    PathQueue.Dequeue();
                }
            }
            else
            {
                if (AvoidAoeStuff(WowInterface.I.Player.Position, out Vector3 newPosition))
                {
                    SetMovementAction(MovementAction.Move, newPosition);
                }
            }
        }

        public bool IsPositionReachable(Vector3 position, out IEnumerable<Vector3> path, float maxDistance = 5.0f)
        {
            // dont search a path into aoe effects
            if (AvoidAoeStuff(position, out Vector3 newPosition))
            {
                position = newPosition;
            }

            path = WowInterface.I.PathfindingHandler.GetPath((int)WowInterface.I.ObjectManager.MapId, WowInterface.I.Player.Position, position);

            if (path != null && path.Any())
            {
                Vector3 lastNode = path.LastOrDefault();
                return lastNode != default && lastNode.GetDistance(position) < maxDistance;
            }

            return false;
        }

        public void PreventMovement(TimeSpan timeSpan)
        {
            StopMovement();
            MovementBlockedUntil = DateTime.UtcNow + timeSpan;
        }

        public void Reset()
        {
            PathQueue.Clear();
            Status = MovementAction.None;
            TriedToMountUp = false;
        }

        public bool SetMovementAction(MovementAction state, Vector3 position, float rotation = 0.0f)
        {
            if (IsAllowedToMove && (PathQueue.Count == 0 || RefreshPathEvent.Ready))
            {
                if (state == MovementAction.DirectMove || WowInterface.I.Player.IsFlying || WowInterface.I.Player.IsUnderwater)
                {
                    WowInterface.I.CharacterManager.MoveToPosition(IsUnstucking ? UnstuckTarget : position);
                    Status = state;
                    DistanceMovedJumpCheck();
                }
                else if (FindPathEvent.Run() && IsPositionReachable(position, out IEnumerable<Vector3> path))
                {
                    // if its a new path, we can try to mount again
                    if (path.Last().GetDistance(LastTargetPosition) > 10.0f)
                    {
                        TriedToMountUp = false;
                    }

                    PathQueue.Clear();

                    foreach (Vector3 node in path)
                    {
                        PathQueue.Enqueue(node);
                    }

                    RefreshPathEvent.Run();
                    Status = state;
                    LastTargetPosition = path.Last();
                    return true;
                }
            }

            return false;
        }

        public void StopMovement()
        {
            PathQueue.Clear();
            WowInterface.I.HookManager.WowStopClickToMove();
        }

        private static Vector3 GetPositionOutsideOfAoeSpells(Vector3 targetPosition, IEnumerable<(Vector3 position, float radius)> aoeSpells)
        {
            if (aoeSpells.Any())
            {
                // build mean position and move away x meters from it
                // x is the biggest distance we have to move
                Vector3 meanAoePos = BotMath.GetMeanPosition(aoeSpells.Select(e => e.position));
                float distanceToMove = aoeSpells.Max(e => e.radius);

                // get average angle to produce the outgoing angle
                float outgoingAngle = aoeSpells.Average(e => BotMath.GetFacingAngle(e.position, meanAoePos));

                // "repell" the position from the aoe spell
                return BotUtils.MoveAhead(targetPosition, outgoingAngle, distanceToMove + 1.5f);
            }

            return targetPosition;
        }

        private bool AvoidAoeStuff(Vector3 position, out Vector3 newPosition)
        {
            List<(Vector3 position, float radius)> places = new List<(Vector3 position, float radius)>(PlacesToAvoid);
            places.AddRange(WowInterface.I.ObjectManager.GetAoeSpells(position).Select(e => (e.Position, e.Radius)));

            if (places.Any())
            {
                newPosition = GetPositionOutsideOfAoeSpells(position, places);
                return true;
            }

            newPosition = position;
            return false;
        }

        private void DistanceMovedJumpCheck()
        {
            if (DistanceMovedCheckEvent.Ready)
            {
                if (LastMovement != default && DateTime.UtcNow - LastMovement < TimeSpan.FromSeconds(1))
                {
                    CurrentSpeed = LastPosition.GetDistance(WowInterface.I.Player.Position) / (float)(DateTime.UtcNow - LastMovement).TotalSeconds;

                    if (IsUnstucking && CurrentSpeed > 1.0f)
                    {
                        IsUnstucking = false;
                    }

                    if (CurrentSpeed == 0.0f && !IsUnstucking)
                    {
                        // hard stuck
                        IsUnstucking = true;

                        // get position behind us
                        Vector3 positionBehind = BotUtils.MoveAhead(WowInterface.I.Player.Position, WowInterface.I.Player.Rotation, -WowInterface.I.MovementSettings.UnstuckDistance);
                        UnstuckTarget = WowInterface.I.PathfindingHandler.GetRandomPointAround((int)WowInterface.I.ObjectManager.MapId, positionBehind, 5.0f);

                        Reset();
                        SetMovementAction(MovementAction.Move, UnstuckTarget);
                    }
                    else if (CurrentSpeed < 0.1f)
                    {
                        // soft stuck
                        WowInterface.I.CharacterManager.Jump();
                    }
                }

                LastMovement = DateTime.UtcNow;
                LastPosition = WowInterface.I.Player.Position;
                DistanceMovedCheckEvent.Run();
            }
        }

        private void MountUp()
        {
            IEnumerable<WowMount> filteredMounts = WowInterface.I.CharacterManager.Mounts;

            if (Config.UseOnlySpecificMounts)
            {
                filteredMounts = filteredMounts.Where(e => Config.Mounts.Split(",", StringSplitOptions.RemoveEmptyEntries).Any(x => x.Equals(e.Name.Trim(), StringComparison.OrdinalIgnoreCase)));
            }

            if (filteredMounts != null && filteredMounts.Any())
            {
                WowMount mount = filteredMounts.ElementAt(new Random().Next(0, filteredMounts.Count()));
                PreventMovement(TimeSpan.FromSeconds(1));
                WowInterface.I.HookManager.LuaCallCompanion(mount.Index);
            }
        }

        private void MoveCharacter(Vector3 positionToGoTo)
        {
            Vector3 node = WowInterface.I.PathfindingHandler.MoveAlongSurface((int)WowInterface.I.ObjectManager.MapId, WowInterface.I.Player.Position, positionToGoTo);

            if (node != default)
            {
                WowInterface.I.CharacterManager.MoveToPosition(node);
            }

            if (Config.MovementSettings.EnableDistanceMovedJumpCheck)
            {
                DistanceMovedJumpCheck();
            }
        }
    }
}