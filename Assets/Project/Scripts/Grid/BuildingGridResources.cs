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
        private static readonly float[] ResourcePatchSizeProbabilities = new[] {40f, 55f, 5f};
        private static readonly float[] ChunkResourceNumberProbabilities = new[] { 70f, 25f, 5f };

        private static readonly Vector2Int[] Patch0Positions = new Vector2Int[] { new Vector2Int(0, 0) };
        private static readonly Vector2Int[] Patch1Positions = new Vector2Int[] { new Vector2Int(0, 1),new Vector2Int(1, 1),new Vector2Int(1, 0) };
        private static readonly Vector2Int[] Patch2Positions = new Vector2Int[] { new Vector2Int(-1, 0),new Vector2Int(-1, -1),new Vector2Int(0, -1) };
        private static readonly Vector2Int[] Patch3Positions = new Vector2Int[] { new Vector2Int(-2, 0),
            new Vector2Int(-2, 1),new Vector2Int(-2, -1),new Vector2Int(2, -1),new Vector2Int(2, 1),
            new Vector2Int(2, 0),new Vector2Int(-1, -2),new Vector2Int(1, -2),new Vector2Int(0, -2),
            new Vector2Int(-1, 2),new Vector2Int(1, 2),new Vector2Int(0, 2),
        };

        private static readonly Vector2Int[] NeighbourOffsets = new[] { new Vector2Int(0, 1),
            new Vector2Int(0,-1),new Vector2Int(1,0),new Vector2Int(-1,0),Vector2Int.one,
            Vector2Int.one*-1,new Vector2Int(-1,1),new Vector2Int(1,-1),};

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
                bool done;
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
            List<Vector2Int> outerCells = new List<Vector2Int>();
            Vector2Int MinAndMax;

            switch (patchSize)
            {
                case 1:
                    cellPositions.AddRange( Patch0Positions);
                    cellPositions.AddRange( Patch1Positions);
                    outerCells.AddRange(cellPositions);
                    break;
                case 2:
                    cellPositions.AddRange( Patch0Positions);
                    cellPositions.AddRange( Patch1Positions);
                    cellPositions.AddRange( Patch2Positions);
                    outerCells.AddRange(Patch1Positions);
                    outerCells.AddRange(Patch2Positions);
                    break;
                case 3: 
                    cellPositions.AddRange( Patch0Positions);
                    cellPositions.AddRange( Patch1Positions);
                    cellPositions.AddRange( Patch2Positions);
                    cellPositions.AddRange(Patch3Positions);
                    outerCells.AddRange(Patch3Positions);
                    break;
            }
            MinAndMax = new Vector2Int((int)(outerCells.Count*1.5), (int)(cellPositions.Count*1.5));

            while (cellPositions.Count <= MinAndMax.x)
            {
                List<Vector2Int> Remove = new List<Vector2Int>();
                List<Vector2Int> Add = new List<Vector2Int>();
                foreach (var outerCell in outerCells)
                {
                    if(cellPositions.Count + Add.Count >= MinAndMax.y) break;
                    foreach (Vector2Int neighbourOffset in NeighbourOffsets)
                    {
                        if(cellPositions.Count + Add.Count >= MinAndMax.y) break;
                        Vector2Int newCell = outerCell + neighbourOffset;
                        if (cellPositions.Contains(newCell)) continue;
                        if (Random.Range(0f,1f) >=.5f)continue;
                        Remove.Add(outerCell);
                        Add.Add(newCell);
                    }
                }
                foreach (var t in Remove) outerCells.Remove(t);
                foreach (var t in Add)
                {
                    outerCells.Add(t);
                    cellPositions.Add(t);
                }
            }

            GridField<GridObject> buildGridField = chunk.BuildingGrid;
            Vector2Int center = new Vector2Int(Random.Range(patchSize+1, GridBuildingSystem.GridSize.x-patchSize-1),
                Random.Range(patchSize+1, GridBuildingSystem.GridSize.y-patchSize-1));

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
