using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.SlotSystem;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Buildings.General
{
    [Serializable]
    public struct BuildingDataComponent : IComponentData
    {
        public static int NumberOfBuildings;
        public PlacedBuildingData MyPlacedBuildingData { get; }
        public GridChunk MyChunk { get; }

        public GridObject MyGridObject { get; }

        public bool subedToConveyorTick;

        public SlotValidationHandler mySlotValidationHandler;

        public BuildingDataComponent(GridChunk myChunk, GridObject myGridObject,
            PlacedBuildingData myPlacedBuildingData)
        {
            MyChunk = myChunk;
            MyGridObject = myGridObject;
            MyPlacedBuildingData = myPlacedBuildingData;
            subedToConveyorTick = false;
            mySlotValidationHandler = null;
        }
    }
}
