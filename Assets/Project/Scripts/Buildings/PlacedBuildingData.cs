using System;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    [Serializable]
    public class PlacedBuildingData
    {
        public int buildingDataID;
        public Vector2Int origin;
        public int directionID;
    }
}