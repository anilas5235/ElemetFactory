using UnityEngine;

namespace Project.Scripts.ScriptableData
{
    [CreateAssetMenu(fileName = "TileData", menuName = "Factory/Data/Tile", order = 0)]
    public class TileScriptableData : ScriptableObject
    {
        public string nameString;
        public Sprite sprite;
        public int uniqueID;
    }
}