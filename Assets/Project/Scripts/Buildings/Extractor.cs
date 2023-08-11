using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Grid;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.Utilities;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Extractor : PlacedBuilding
    {
        private const int ExtractorStorageCapacity = 5;
        private static float ExtractionSpeed = .5f;
        

        [SerializeField] private BuildingGridResources.ResourcesType generatedResource;
        [SerializeField] private int storedResources = 0;
        public Slot outputSlot;

        protected override void StartWorking()
        {
            generatedResource = MyChunk.ChunkBuildingGrid.GetCellData(MyPlacedBuildingData.origin).ResourceNode;
            outputSlot = new Slot(Slot.SlotBehaviour.Output);
            outputSlot.FillSlot(new Item(new int[]{(int)generatedResource}));
            outputSlot.OnSlotContentChanged += Change;
            StartCoroutine(ResourceGeneration(ExtractionSpeed));
            validOutputSorource = new List<Vector2Int>()
            {
               -1* GeneralConstants.NeighbourOffsets2D4[MyPlacedBuildingData.directionID] + MyPlacedBuildingData.origin,
            };
        }

        protected override Slot GetInputSlot(GridObject callerPosition)
        {
            return null;
        }

        protected override Slot GetOutputSlot(GridObject callerPosition)
        {
            return validInputSorource.Contains(callerPosition.Position) ? outputSlot : null;
        }

        private void Change(bool fillStatus)
        {
            if (!fillStatus && storedResources > 0)
            {
                outputSlot.FillSlot(new Item(new int[]{(int)generatedResource}));
                storedResources--;
            }
        }

        private IEnumerator ResourceGeneration(float ratePerSecond)
        {
            while (storedResources < ExtractorStorageCapacity)
            {
                storedResources++;
                yield return new WaitForSeconds(1 / ratePerSecond);
            }
        }
    }
}
