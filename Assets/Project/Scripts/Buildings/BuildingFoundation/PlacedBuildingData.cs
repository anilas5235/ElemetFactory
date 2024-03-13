using System;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.Buildings.BuildingFoundation
{
    [Serializable]
    public struct PlacedBuildingData
    {
        public int buildingDataID;
        public int2 origin;
        public byte directionID;
    }
    
    public enum FacingDirection :byte
    {
        Up,
        Right,
        Down,
        Left,
    }

    [Serializable]
    public struct PortData
    {
        public byte bodyPartID;
        public FacingDirection direction;
        public byte portID;
    }
}