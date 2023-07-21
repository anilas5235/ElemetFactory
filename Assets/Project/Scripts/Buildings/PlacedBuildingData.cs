using System;
using Project.Scripts.Grid;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    [Serializable]
    public class PlacedBuildingData
    {
        public BuildingGridResources.PossibleBuildings buildingData;
        public Vector2Int origin;
        public BuildingDataBase.Directions direction;
    }
}