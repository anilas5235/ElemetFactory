using System;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Grid.DataContainers;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.Buildings.BuildingFoundation
{
    public static class PlacedBuildingUtility
    {
        public static readonly int2 INT2Up = new int2(0, 1),
            INT2Down = new int2(0, -1),
            INT2Left = new int2(-1, 0),
            INT2Right = new int2(1, 0);
        
        public static bool CheckForBuilding(int2 targetPos, ChunkDataAspect myChunk,out Entity building)
        {
            CellObject cell = myChunk.GetCell(targetPos,myChunk.ChunksPosition);
            building = cell.Building;
            return cell.IsOccupied;
        }
        
        public static FacingDirection GetNextDirectionClockwise(FacingDirection facingDirection)
        {
            return GetNextDirectionClockwise((int)facingDirection);
        }
        
        public static FacingDirection GetNextDirectionClockwise(int facingDirectionID)
        {
            int id = facingDirectionID +1;
            if (id > 3) id = 0;
            return (FacingDirection)id;
        }

        public static FacingDirection GetNextDirectionCounterClockwise(FacingDirection facingDirection)
        {
            return GetNextDirectionCounterClockwise((int)facingDirection);
        }
        
        public static FacingDirection GetNextDirectionCounterClockwise(int facingDirectionID)
        {
            int id = facingDirectionID -1;
            if (id < 0) id = 3;
            return (FacingDirection)id;
        }
        
          public static FacingDirection GetOppositeDirection(FacingDirection facingDirection)
          {
              return GetOppositeDirection((int)facingDirection);
          }
        
        public static FacingDirection GetOppositeDirection(int facingDirectionID)
        {
            return facingDirectionID switch
            {
                0=> FacingDirection.Down,
                1 => FacingDirection.Left, 
                2 => FacingDirection.Up,
                3 => FacingDirection.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(facingDirectionID), facingDirectionID, null)
            };
        }
        public static int2 GetRotatedVectorClockwise(int2 vector)
        {
            return new int2(vector.y, vector.x * -1);
        }

        public static int2 FacingDirectionToVector(FacingDirection facingDirection)
        {
            return FacingDirectionToVector((int)facingDirection);
        }
        
        public static int2 FacingDirectionToVector(int facingDirectionID)
        {
            return facingDirectionID switch
            {
                0 => INT2Up,
                1 => INT2Right,
                2 => INT2Down,
                3 => INT2Left,
                _ => throw new ArgumentOutOfRangeException(nameof(facingDirectionID), facingDirectionID, null)
            };
        }

        public static int GetRotation(FacingDirection facingDirection)
        {
            return GetRotation((int)facingDirection);
        }
        
        public static int GetRotation(int directionID)
        {
            return -90 * directionID;
        }

        public static bool DoYouPointAtMe(FacingDirection facingDirectionOfTarget, Vector2Int realtivPositionOfTarget)
        {
            if (realtivPositionOfTarget is { x: 0, y: > 0 } && facingDirectionOfTarget == FacingDirection.Down) return true;
            if (realtivPositionOfTarget is { x: 0, y: < 0 } && facingDirectionOfTarget == FacingDirection.Up) return true;
            if (realtivPositionOfTarget is { x: > 0, y: 0 } && facingDirectionOfTarget == FacingDirection.Left) return true;
            if (realtivPositionOfTarget is { x: < 0, y: 0 } && facingDirectionOfTarget == FacingDirection.Right) return true;
            return false;
        }

        public static bool VectorToFacingDirection(Vector2 vector2, out FacingDirection facingDirection)
        {
            facingDirection = FacingDirection.Up;
            if (vector2 == Vector2.up)
            {
                return true;
            }
            if (vector2 == Vector2.right)
            {
                facingDirection = FacingDirection.Right;
                return true;
            }
            if (vector2 == Vector2.down)
            {
                facingDirection = FacingDirection.Down;
                return true;
            }

            if (vector2 != Vector2.left) return false;
            
            facingDirection = FacingDirection.Left;
            return true;
        }
        public static bool DoYouPointAtMe(int facingDirectionOfTargetID, Vector2Int realtivPositionOfTarget)
        {
            return DoYouPointAtMe((FacingDirection)facingDirectionOfTargetID, realtivPositionOfTarget);
        }
    }
}
