using System;
using UnityEngine;

namespace Project.Scripts.Buildings.BuildingFoundation
{
    [Serializable]
    public struct PlacedBuildingData
    {
        public int buildingDataID;
        public Vector2Int origin;
        public int directionID;
    }
}