using System.Collections;
using Project.Scripts.Grid;
using Project.Scripts.Grid.DataContainers;
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

        protected override void StartWorking()
        {
            generatedResource = MyChunk.ChunkBuildingGrid.GetCellData(MyPlacedBuildingData.origin).ResourceNode;
            outputSlot.FillSlot(ItemContainer.CreateNewContainer(new Item(new int[] { (int)generatedResource }),outputSlot));
            outputSlot.OnSlotContentChanged += TryPushItemToOutput;
            StartCoroutine(ResourceGeneration(ExtractionSpeed));
        }

        public override Slot GetInputSlot(GridObject callerPosition, Slot destination)
        {
            return null;
        }

        public override Slot GetOutputSlot(GridObject callerPosition, Slot destination)
        {
            return mySlotValidationHandler.ValidateOutputSlotRequest(MyGridObject.Position, callerPosition.Position)
                ? outputSlot
                : null;
        }

        protected override void SetUpSlots(BuildingScriptableData.Directions direction)
        {
            SlotValidationHandlers ??= new[]
            {
                Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/ExtractorUp"),
                Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/ExtractorRight"),
                Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/ExtractorDown"),
                Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/ExtractorLeft"),
            };

            mySlotValidationHandler = SlotValidationHandlers[(int)direction];
        }

        public override void CheckForInputs()
        {
            return;
        }

        public override void CheckForOutputs()
        {
            foreach (Vector2Int validOutputPosition in mySlotValidationHandler.ValidOutputPositions)
            {
                GridObject cell = MyChunk.ChunkBuildingGrid.GetCellData(validOutputPosition + MyGridObject.Position);
                PlacedBuilding cellBuild = cell.Building;
                if (!cellBuild) return;
                cellBuild.CheckForInputs();
            }
        }

        private void TryPushItemToOutput(bool fillStatus)
        {
            if (!outputSlot) return;
            if (fillStatus || storedResources <= 0) return;
            outputSlot.FillSlot(ItemContainer.CreateNewContainer(new Item(new int[] { (int)generatedResource }),outputSlot));
            storedResources--;
        }

        private IEnumerator ResourceGeneration(float ratePerSecond)
        {
            while (storedResources < StorageCapacity)
            {
                storedResources++;
                TryPushItemToOutput(outputSlot.IsOccupied);
                yield return new WaitForSeconds(1 / ratePerSecond);
            }
        }
    }
}
