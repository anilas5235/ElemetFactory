using System;
using System.Collections.Generic;
using Project.Scripts.Buildings;
using Project.Scripts.Grid.DataContainers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Scripts.Grid
{
    public static class BuildingGridResources
    {
        private static readonly float[] ResourcePatchSizeProbabilities = new[] { 0f, 30f, 40f, 15f, 10f, 5f};
        private static readonly float[] ChunkResourceNumberProbabilities = new[] { 20f, 60f, 20f };

        public enum ResourcesType
        {
            None,
            H,
            C,
            N,
            O,
        }

        public static ResourcesType GetRandom()
        {
            return (ResourcesType) Random.Range(1,Enum.GetNames(typeof(ResourcesType)).Length);
        }
        
        public static void GenerateResources(GridChunk chunk)
        {
            float distToCenter = Vector2Int.Distance(chunk.ChunkPosition, Vector2Int.zero);
            if(distToCenter < 2f ) return;
            int numberOfPatches = GetNumberOfChunkResources();
            if (numberOfPatches <1)return;
            ResourcesType[] chunkResources = new ResourcesType[numberOfPatches];
            
            for (int i = 0; i < numberOfPatches; i++)
            {
                ResourcesType type;
                bool done = false;
                do
                {
                    done = true;
                    type = GetRandom();
                    foreach (ResourcesType resource in chunkResources)
                    {
                        if (type != resource) continue;
                        done = false;
                        break;
                    }
                } while (!done);
                chunkResources[i] = type;
            }

            foreach (ResourcesType resource in chunkResources)
            {
                GenerateResourcePatch(chunk,GetPatchSize(),resource);
            }
        }

        public static void GenerateResourcePatch(GridChunk chunk, int patchSize, ResourcesType resourcesType)
        {
            List<Vector2Int> cellPositions = new List<Vector2Int>();
            for (int x = -patchSize; x < patchSize; x++)
            {
                for (int y = -patchSize; y < patchSize; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) >= patchSize) continue;
                    if (patchSize >= 3)
                    {
                        if (Mathf.Abs(x) + Mathf.Abs(y) > patchSize * 0.666f && Random.Range(0f, 1f) >= 0.5f) continue;
                    }

                    cellPositions.Add(new Vector2Int(x, y));
                }
            }
            GridField<GridObject> buildGridField = chunk.BuildingGrid;
            Vector2Int center = new Vector2Int(Random.Range(patchSize, GridBuildingSystem.GridSize.x-patchSize),
                Random.Range(patchSize, GridBuildingSystem.GridSize.y-patchSize));

            for (int i = 0; i < cellPositions.Count; i++)
            {
                Vector2Int cellPosition = cellPositions[i]+center;
                if (buildGridField.IsValidPosition(cellPosition))
                {
                    buildGridField.GetCellData(cellPosition).SetResource(resourcesType);
                }
            }

        }

        public static int GetNumberOfChunkResources()
        {
            int returnVal = 0;
            float random = Random.Range(0f, 100f);
            float currentStep = 0f;
            for (int i = 0; i < ChunkResourceNumberProbabilities.Length; i++)
            {
                if(ChunkResourceNumberProbabilities[i]==0)continue;
                currentStep += ChunkResourceNumberProbabilities[i];
                if (random > currentStep) continue;
                returnVal += i;
                break;
            }

            return returnVal;
        }

        public static int GetPatchSize()
        {
            int returnVal = 1;
            float random = Random.Range(0f, 100f);
            float currentStep = 0f;
            for (int i = 0; i < ResourcePatchSizeProbabilities.Length; i++)
            {
                if(ResourcePatchSizeProbabilities[i]==0)continue;
                currentStep += ResourcePatchSizeProbabilities[i];
                if (random > currentStep) continue;
                returnVal += i;
                break;
            }

            return returnVal;
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
            return GetBuildingDataBase((int)buildingType);
        }
        
        public static BuildingDataBase GetBuildingDataBase(int buildingTypeID)
        {
            return possibleBuildingData[buildingTypeID];
        }
    }
}
