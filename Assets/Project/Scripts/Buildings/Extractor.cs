using System.Collections;
using Project.Scripts.Grid;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using Project.Scripts.Visualisation;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Extractor : PlacedBuilding
    {
        private const int StorageCapacity = 5;
        private static float ExtractionSpeed = .5f;
        private static SlotValidationHandler[] SlotValidationHandlers;


        [SerializeField] private BuildingGridResources.ResourcesType generatedResource;
        [SerializeField] private int storedResources = 0;
        [SerializeField] private Slot outputSlot;

        private SlotValidationHandler mySlotValidationHandler;
        private Coroutine generation;

        protected override void StartWorking()
        {
            generatedResource = MyChunk.ChunkBuildingGrid.GetCellData(MyPlacedBuildingData.origin).ResourceNode;
            if (generatedResource == BuildingGridResources.ResourcesType.None) return;
            outputSlot.FillSlot(ItemContainer.CreateNewContainer(new Item(new int[] { (int)generatedResource }),outputSlot));
            outputSlot.OnSlotContentChanged += TryPushItemToOutput;
            generation = StartCoroutine(ResourceGeneration());
        }

        public override Slot GetInputSlot(PlacedBuildingData caller, Slot destination)
        {
            return null;
        }

        public override Slot GetOutputSlot(PlacedBuildingData caller, Slot destination)
        {
            return mySlotValidationHandler.ValidateOutputSlotRequest(MyGridObject.Position, caller.origin,
                (BuildingScriptableData.FacingDirection)caller.directionID)
                ? outputSlot
                : null;
        }

        protected override void SetUpSlots(BuildingScriptableData.FacingDirection facingDirection)
        {
            SlotValidationHandlers ??= new[]
            {
                Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/ExtractorUp"),
                Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/ExtractorRight"),
                Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/ExtractorDown"),
                Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/ExtractorLeft"),
            };

            mySlotValidationHandler = SlotValidationHandlers[(int)facingDirection];
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
            if (storedResources <= 0) return;
            outputSlot.FillSlot(ItemContainer.CreateNewContainer(new Item(new int[] { (int)generatedResource }),outputSlot));
            storedResources--;
            if (generation == null) generation = StartCoroutine(ResourceGeneration());
        }

        private IEnumerator ResourceGeneration()
        {
            while (storedResources < StorageCapacity)
            {
                storedResources++;
                TryPushItemToOutput(outputSlot.IsOccupied);
                yield return new WaitForSeconds(1 / ExtractionSpeed);
            }

            generation = null;
        }
    }
}
