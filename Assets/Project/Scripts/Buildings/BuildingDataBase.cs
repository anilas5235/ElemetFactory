using UnityEngine;

namespace Project.Scripts.Buildings
{
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
    }
}
