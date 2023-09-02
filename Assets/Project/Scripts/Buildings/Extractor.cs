using System;
using System.Collections;
using Project.Scripts.Buildings.BuildingFoundation;
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
        [SerializeField] private ResourceType generatedResource;

        private Coroutine generation;

        protected override void StartWorking()
        {
            generatedResource = MyChunk.ChunkBuildingGrid.GetCellData(MyPlacedBuildingData.origin).ResourceNode;
            if (generatedResource == ResourceType.None) return;
            storage = new Container<Item>(new Item(new int[] { (int)generatedResource }), 1, StorageCapacity);

            slotsToPushTo = new Slot[outputs.Length];

            CheckForSlotsToPushTo();
            TryPushItemToOutput(outputs[0].IsOccupied);
        }

        public Slot GetOutputSlot(PlacedBuilding caller, Slot destination)
        {
            if (slotsToPushTo[0]) return null;
            if (!mySlotValidationHandler.ValidateOutputSlotRequest(this, caller, out int index)) return null;
            
            slotsToPushTo[0] = destination;
            return outputs[index];
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
            outputs[0].OnSlotContentChanged += TryPushItemToOutput;
        }

        private void TryPushItemToOutput(bool fillStatus)
        {
            if (!outputs[0]|| fillStatus) return;
            if (storage.ContainedAmount <= 0) return;
            outputs[0].FillSlot(ItemUtility.GetItemContainerWith(storage.Extract()[0],outputs[0]));
            generation ??= StartCoroutine(ResourceGeneration());
        }

        private IEnumerator ResourceGeneration()
        {
            while (storage.ContainedAmount < storage.MaxContainableAmount)
            {
                storage.AddAmount();
                TryPushItemToOutput(outputs[0].IsOccupied);
                yield return new WaitForSeconds(1f / ExtractionSpeed);
            }
            generation = null;
        }

        public Container<Item> GetContainer()
        {
            return storage;
        }
    }
}
