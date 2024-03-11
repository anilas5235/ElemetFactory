using System;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.Buildings.BuildingFoundation
{
    public enum FacingDirection :byte
    {
        Up,
        Right,
        Down,
        Left,
    }
  
    [CreateAssetMenu(menuName = "ElementFactory/Data/Building")]
    public class BuildingScriptableData : ScriptableObject
    {
        public string nameString;
        public int buildingID;
        public GameObject prefab;
        public int2[] neededTiles;
        public PortData[] InputOffsets, OutputOffsets;
        public override string ToString()
        {
            return nameString;
        }
    }

    [Serializable]
    public struct PortData
    {
        public byte bodyPartID;
        public FacingDirection direction;
        public byte portID;
    }
}
