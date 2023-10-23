using System;
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
    }
}
