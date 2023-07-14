using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    /*
     * Code based on the work of Code Monkey
     */
    [CreateAssetMenu]
    public class BuildingDataBase : ScriptableObject
    {
        public static Directions GetNextDirection(Directions direction)
        {
            int id = (int) direction +1;
            if (id > 3) id = 0;
            return (Directions)id;
        }
        
        public enum Directions
        {
            Down,
            Left,
            Up,
            Right
        }

        public string nameString;
        public Transform prefab, visual;
        public Vector2Int size;

        public int GetRotation(Directions direction)
        {
            return -90 * (int)direction;
        }
        public List<Vector2Int> GetGridPositionList(Vector2Int offset, Directions direction)
        {
            List<Vector2Int> gridPositions = new List<Vector2Int>();
            switch (direction)
            {
                case Directions.Down:
                    SetPosition(size.x, size.y, false, false);
                    break;
                case Directions.Up:
                    SetPosition(size.x, size.y, true, false);
                    break;
                case Directions.Left:
                    SetPosition(size.y, size.x, false, true);
                    break;
                case Directions.Right:
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

            return gridPositions;
        }

        public override string ToString()
        {
            return nameString;
        }
    }
}
