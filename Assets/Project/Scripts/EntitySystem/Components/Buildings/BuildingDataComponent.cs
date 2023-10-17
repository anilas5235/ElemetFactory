using Project.Scripts.Buildings.BuildingFoundation;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Buildings
{
    public readonly struct BuildingDataComponent : IComponentData
    {
        public readonly PlacedBuildingData BuildingData;

        public BuildingDataComponent(PlacedBuildingData buildingData)
        {
            BuildingData = buildingData;
        }
    }
}
