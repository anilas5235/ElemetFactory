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
        private static readonly float[] ResourcePatchSizeProbabilities = new[] { 0f, 5f, 20f, 35f, 25f, 15f};

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
            if(distToCenter < 1f || distToCenter> 100f) return;
            int numberOfPatches = Random.Range(1, 3);
            for (int i = 0; i < numberOfPatches; i++)
            {
                GenerateResourcePatch(chunk,GetPatchSize(),GetRandom());
            }
        }

        public static void GenerateResourcePatch(GridChunk chunk, int patchSize, ResourcesType resourcesType)
        {
            List<Vector2Int> cellPositions = new List<Vector2Int>();
            for (int x = -patchSize; x < patchSize; x++)
            {
                for (int y = -patchSize; y < patchSize; y++)
                {
                    if(Mathf.Abs(x)+Mathf.Abs(y)>patchSize)continue;
                    cellPositions.Add(new Vector2Int(x,y));
                }
            }
            GridField<GridObject> buildGridField = chunk.BuildingGrid;
            Vector2Int center = new Vector2Int((int)Random.Range(0, GridBuildingSystem.ChunkSize.x/2f),(int)
                Random.Range(0, GridBuildingSystem.ChunkSize.y/2f));

            for (int i = 0; i < cellPositions.Count; i++)
            {
                Vector2Int cellPosition = cellPositions[i]+center;
                if (buildGridField.IsValidPosition(cellPosition))
                {
                    buildGridField.GetCellData(cellPosition).SetResource(resourcesType);
                }
                else
                {
                    continue;
                    GridChunk otherChunk = chunk.myGridBuildingSystem.GetChunk(
                            GridChunk.GetChunkPositionOverflow(cellPosition, chunk.ChunkPosition));
                    Vector2Int positionOfCell = GridChunk.GetCellPositionOverflow(cellPosition);
                    otherChunk.BuildingGrid.GetCellData(positionOfCell).SetResource(resourcesType);
                }
            }

        }

        public static int GetPatchSize()
        {
            int returnVal = 0;
            float random = Random.Range(0f, 100f);
            float currentStep = 0f;
            for (int i = 0; i < ResourcePatchSizeProbabilities.Length; i++)
            {
                if(ResourcePatchSizeProbabilities[i]==0)continue;
                currentStep += ResourcePatchSizeProbabilities[i];
                if (!(random <= currentStep)) continue;
                returnVal = i+1;
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
            return possibleBuildingData[(int)buildingType];
        }
    }
}
