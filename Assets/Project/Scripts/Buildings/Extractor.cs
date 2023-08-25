using System;
using System.Collections;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Extractor : PlacedBuilding, IContainable<Item>, IHaveOutput
    {
        private const int StorageCapacity = 5;
        private static float ExtractionSpeed = .5f;
        private static SlotValidationHandler[] SlotValidationHandlers;
        [SerializeField] private Container<Item> storage;

        [SerializeField] private BuildingGridResources.ResourcesType generatedResource;
        [SerializeField] private Slot outputSlot;

        protected SlotValidationHandler mySlotValidationHandler;
        private Coroutine generation;

        protected override void StartWorking()
        {
            generatedResource = MyChunk.ChunkBuildingGrid.GetCellData(MyPlacedBuildingData.origin).ResourceNode;
            if (generatedResource == BuildingGridResources.ResourcesType.None) return;
            storage = new Container<Item>(new Item(new int[] { (int)generatedResource }), 1, StorageCapacity);

            if (PlacedBuildingUtility.CheckForBuilding(
                    MyGridObject.Position +
                    PlacedBuildingUtility.FacingDirectionToVector(MyPlacedBuildingData.directionID),
                    MyChunk, out PlacedBuilding building))
            {
                if (building.MyPlacedBuildingData.buildingDataID == MyPlacedBuildingData.directionID)
                    building.CheckForSlotToPullForm();
            }

            TryPushItemToOutput(false);
            generation = StartCoroutine(ResourceGeneration());
        }

        public Slot GetOutputSlot(PlacedBuildingData caller, Slot destination)
        {
            return mySlotValidationHandler.ValidateOutputSlotRequest(MyGridObject.Position, caller.origin,
                (FacingDirection)caller.directionID)
                ? outputSlot
                : null;
        }
        protected override void SetUpSlots(FacingDirection facingDirection)
        {
            mySlotValidationHandler = MyPlacedBuildingData.directionID switch
            {
                0 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Extractor/ExtractorUp"),
                1 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Extractor/ExtractorRight"),
                2 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Extractor/ExtractorDown"),
                3 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Extractor/ExtractorLeft"),
                _ => throw new ArgumentOutOfRangeException()
            };
            outputSlot.OnSlotContentChanged += TryPushItemToOutput;
        }

        public override void CheckForSlotToPullForm()
        {
            throw new System.NotImplementedException();
        }

        public override void CheckForSlotsToPushTo()
        {
            throw new System.NotImplementedException();
        }

        private void TryPushItemToOutput(bool fillStatus)
        {
            if (!outputSlot|| fillStatus) return;
            if (storage.ContainedAmount <= 0) return;
            outputSlot.FillSlot(ItemContainer.CreateNewContainer(storage.Extract()[0],outputSlot));
            generation ??= StartCoroutine(ResourceGeneration());
        }

        private IEnumerator ResourceGeneration()
        {
            while (storage.ContainedAmount < storage.MaxContainableAmount)
            {
                storage.AddAmount();
                TryPushItemToOutput(outputSlot.IsOccupied);
                yield return new WaitForSeconds(1 / ExtractionSpeed);
            }
            generation = null;
        }

        public Container<Item> GetContainer()
        {
            return storage;
        }
    }
}
