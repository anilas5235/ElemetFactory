using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    /*
     * Code based on the work of Code Monkey
     */
    [CreateAssetMenu(menuName = "BuildingSystem/BuildingScriptableData")]
    public class BuildingScriptableData : ScriptableObject
    {
        public string nameString;
        public Transform prefab;
        public Vector2Int size;

        #region Rotations
        public static FacingDirection GetNextDirectionClockwise(FacingDirection facingDirection)
        {
            int id = (int) facingDirection +1;
            if (id > 3) id = 0;
            return (FacingDirection)id;
        }

        public static FacingDirection GetNextDirectionCounterClockwise(FacingDirection facingDirection)
        {
            int id = (int) facingDirection -1;
            if (id < 0) id = 3;
            return (FacingDirection)id;
        }

        public enum FacingDirection
        {
            Up,
            Right,
            Down,
            Left,
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
        
        #endregion

        #region Positions
        public Vector2Int[] GetGridPositionList(Vector2Int offset, FacingDirection facingDirection)
        {
            List<Vector2Int> gridPositions = new List<Vector2Int>();
            switch (facingDirection)
            {
                case FacingDirection.Down:
                    SetPosition(size.x, size.y, false, false);
                    break;
                case FacingDirection.Up:
                    SetPosition(size.x, size.y, true, false);
                    break;
                case FacingDirection.Left:
                    SetPosition(size.y, size.x, false, true);
                    break;
                case FacingDirection.Right:
                    SetPosition(size.y, size.x, false, false);
                    break;
            }

            void SetPosition(int sizeX, int sizeY, bool negativeX, bool negativeY)
            {
                int xMulti = negativeX ? -1 : 1;
                int yMulti = negativeY ? -1 : 1;
                
                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        gridPositions.Add(offset + new Vector2Int(x * xMulti, y * yMulti));
                    }
                }
            }

            return gridPositions.ToArray();
        }

        public Vector2Int[] GetGridPositionList(PlacedBuildingData data)
        {
            return GetGridPositionList(data.origin,(FacingDirection) data.directionID);
        }
        
        #endregion

        public override string ToString()
        {
            return nameString;
        }
    }
}
