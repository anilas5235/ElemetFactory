using System.Collections;
using Project.Scripts.Grid;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Extractor : PlacedBuilding
    {
        private const int ExtractorStorageCapacity = 5;
        private const float ExtractionSpeed = .5f;

        [SerializeField] private BuildingGridResources.ResourcesType generatedResource;
        [SerializeField] private int storedResources = 0;


        protected override void StartWorking()
        {
            base.StartWorking();
            generatedResource = MyChunk.ChunkBuildingGrid.GetCellData(MyPlacedBuildingData.origin).ResourceNode;
            StartCoroutine(ResourceGeneration(ExtractionSpeed));
        }

        public bool OutputResources(int amount, out BuildingGridResources.ResourcesType type)
        {
            type = generatedResource;
            if (storedResources < amount) return false;

            storedResources -= amount;
            return true;
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
