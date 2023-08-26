using System;
using Project.Scripts.Grid.DataContainers;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public static class PlacedBuildingUtility 
    {
        public static bool CheckForBuilding(Vector2Int targetPos, GridChunk myChunk,out PlacedBuilding building)
        {
            GridObject cell = myChunk.ChunkBuildingGrid.IsValidPosition(targetPos)
                ? myChunk.ChunkBuildingGrid.GetCellData(targetPos)
                : myChunk.myGridBuildingSystem.GetGridObjectFormPseudoPosition(targetPos, myChunk);
            building = cell.Building;
            return building;
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
            return facingDirection switch
            {
                FacingDirection.Up => FacingDirection.Down,
                FacingDirection.Right => FacingDirection.Left,
                FacingDirection.Down => FacingDirection.Up,
                FacingDirection.Left => FacingDirection.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(facingDirection), facingDirection, null)
            };
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
            return facingDirection switch
            {
                FacingDirection.Up => Vector2Int.up,
                FacingDirection.Right => Vector2Int.right,
                FacingDirection.Down => Vector2Int.down,
                FacingDirection.Left => Vector2Int.left,
                _ => throw new ArgumentOutOfRangeException(nameof(facingDirection), facingDirection, null)
            };
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
            return -90 * directionID +180;
        }

        public static bool DoYouPointAtMe(FacingDirection facingDirectionOfTarget, Vector2Int realtivPositionOfTarget)
        {
            if (realtivPositionOfTarget.x == 0 && realtivPositionOfTarget.y > 0 && facingDirectionOfTarget == FacingDirection.Down) return true;
            if (realtivPositionOfTarget.x == 0 && realtivPositionOfTarget.y < 0 && facingDirectionOfTarget == FacingDirection.Up) return true;
            if (realtivPositionOfTarget.x > 0 && realtivPositionOfTarget.y == 0 && facingDirectionOfTarget == FacingDirection.Left) return true;
            if (realtivPositionOfTarget.x < 0 && realtivPositionOfTarget.y == 0 && facingDirectionOfTarget == FacingDirection.Right) return true;
            return false;
        }
        public static bool DoYouPointAtMe(int facingDirectionOfTargetID, Vector2Int realtivPositionOfTarget)
        {
            return DoYouPointAtMe((FacingDirection)facingDirectionOfTargetID, realtivPositionOfTarget);
        }
    }
}
