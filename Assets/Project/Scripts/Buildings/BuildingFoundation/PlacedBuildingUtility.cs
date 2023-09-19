using System;
using Project.Scripts.Grid.DataContainers;
using UnityEngine;

namespace Project.Scripts.Buildings.BuildingFoundation
{
    public static class PlacedBuildingUtility 
    {
        public static bool CheckForBuilding(Vector2Int targetPos, GridChunk myChunk,out PlacedBuildingEntity building)
        {
            GridObject cell = myChunk.ChunkBuildingGrid.IsValidPosition(targetPos)
                ? myChunk.ChunkBuildingGrid.GetCellData(targetPos)
                : myChunk.myGridBuildingSystem.GetGridObjectFormPseudoPosition(targetPos, myChunk);
            building = cell.Building;
            return cell.Occupied;
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

        public static Vector2Int FacingDirectionToVector(FacingDirection facingDirection)
        {
            return FacingDirectionToVector((int)facingDirection);
        }
        
        public static Vector2Int FacingDirectionToVector(int facingDirectionID)
        {
            return facingDirectionID switch
            {
                0 => Vector2Int.up,
                1 => Vector2Int.right,
                2 => Vector2Int.down,
                3 => Vector2Int.left,
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
