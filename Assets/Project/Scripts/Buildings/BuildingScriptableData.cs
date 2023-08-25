using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public enum FacingDirection
    {
        Up,
        Right,
        Down,
        Left,
    }

    /*
     * Code based on the work of Code Monkey
     */
    [CreateAssetMenu(menuName = "BuildingSystem/BuildingScriptableData")]
    public class BuildingScriptableData : ScriptableObject
    {
        public string nameString;
        public Transform prefab;
        public Vector2Int size;

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
