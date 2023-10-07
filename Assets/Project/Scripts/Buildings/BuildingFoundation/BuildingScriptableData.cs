using System.Collections.Generic;
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

    /*
     * Code based on the work of Code Monkey
     */
    [CreateAssetMenu(menuName = "BuildingSystem/BuildingScriptableData")]
    public class BuildingScriptableData : ScriptableObject
    {
        public string nameString;
        public int buildingID;
        public Transform prefab;
        public int2[] neededTiles;
        public int2[] InputOffsets, OutputOffsets;
        public override string ToString()
        {
            return nameString;
        }
    }
}
