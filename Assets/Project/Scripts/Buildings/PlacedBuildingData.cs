using System;
using Project.Scripts.Grid;
using UnityEngine;
using UnityEngine.Serialization;

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