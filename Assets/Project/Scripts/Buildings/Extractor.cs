using System.Collections;
using Project.Scripts.Grid;
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
