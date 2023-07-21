using System;
using Project.Scripts.Buildings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Scripts.Grid
{
    public static class BuildingGridResources
    {
        public enum ResourcesType
        {
            None,
            H,
            He,
            Li,
            Be,
            Bor,
            C,
            N,
            O,
        }

        public static ResourcesType GetRandom()
        {
            return (ResourcesType) Random.Range(1,Enum.GetNames(typeof(ResourcesType)).Length);
        }
        public enum PossibleBuildings
        {
            Drill,
            Smelter,
        }

        private static BuildingDataBase[] possibleBuildingData = new[]
        {
            Resources.Load<BuildingDataBase>("Buildings/Data/test")
        };

        public static BuildingDataBase GetBuildingDataBase(PossibleBuildings buildingType)
        {
            return possibleBuildingData[(int)buildingType];
        }
    }
}
