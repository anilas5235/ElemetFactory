using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Buildings;
using Project.Scripts.Grid.DataContainers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project.Scripts.Grid
{
    public static class BuildingGridResources
    {
        #region ResourceGeneration
        
        private static readonly float[] ResourcePatchSizeProbabilities =  {60f, 39f, 1f};
        private static readonly float[] ChunkResourceNumberProbabilities = { 70f, 25f, 5f };

        private static readonly Vector2Int[] Patch0Positions =  { new Vector2Int(0, 0) };
        private static readonly Vector2Int[] Patch1Positions = 
            { new Vector2Int(0, 1),new Vector2Int(1, 1),new Vector2Int(1, 0) };
        private static readonly Vector2Int[] Patch2Positions = 
            {
                new Vector2Int(-1, 1), new Vector2Int(-1, 0),new Vector2Int(-1, -1),new Vector2Int(0, -1),
                new Vector2Int(1, -1),
            };
        private static readonly Vector2Int[] Patch3Positions = 
        {
            new Vector2Int(-2, 0), new Vector2Int(-2, 1),new Vector2Int(-2, -1),new Vector2Int(2, -1),
            new Vector2Int(2, 1), new Vector2Int(2, 0),new Vector2Int(-1, -2),new Vector2Int(1, -2),
            new Vector2Int(0, -2), new Vector2Int(-1, 2),new Vector2Int(1, 2),new Vector2Int(0, 2),
        };

        private static readonly Vector2Int[] NeighbourOffsets =
        { new Vector2Int(0, 1), new Vector2Int(0,-1),new Vector2Int(1,0),new Vector2Int(-1,0),};

        public enum ResourcesType
        {
            None,
            H,
            C,
            N,
            O,
        }
        private static ResourcesType GetRandom(float distanceToCenter)
        {
            int pool=1;
            if (distanceToCenter >= 2f) pool+=3;
            if (distanceToCenter >= 5f) pool+=1;
            return (ResourcesType) Random.Range(1,pool);
        }

        public static ChunkResourcePatch[] GenerateResources(GridChunk chunk)
        {
            List<ChunkResourcePatch> patches = new List<ChunkResourcePatch>();
            float distToCenter = Vector2Int.Distance(chunk.ChunkPosition, Vector2Int.zero);
            if(distToCenter < 2f ) return patches.ToArray();
            int numberOfPatches = GetNumberOfChunkResources();
            if (numberOfPatches <1)return patches.ToArray();
            ResourcesType[] chunkResources = new ResourcesType[numberOfPatches];
            List<Vector2Int> blockPositions = new List<Vector2Int>();

            for (int i = 0; i < numberOfPatches; i++)
            {
                ResourcesType type;
                bool done;
                do
                {
                    done = true;
                    type = GetRandom(Vector2Int.Distance(Vector2Int.zero, chunk.ChunkPosition));
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
                patches.Add(GenerateResourcePatch(chunk,GetPatchSize(numberOfPatches),resource,blockPositions));
            }

            return patches.ToArray();
        }

        private static ChunkResourcePatch GenerateResourcePatch(GridChunk chunk, int patchSize, ResourcesType resourcesType,
            List<Vector2Int> blocked)
        {
            List<Vector2Int> cellPositions = GeneratePatchShape(patchSize, blocked,chunk);

            foreach (Vector2Int cellPosition in cellPositions) blocked.Add(cellPosition);
            
            return new ChunkResourcePatch
            {
                resourceID = (int)resourcesType,
                positions = cellPositions.ToArray()
            };
        }

        private static void GetPathBaseShape(int patchSize, out List<Vector2Int> cellPositions,out List<Vector2Int> outerCells)
        {
            cellPositions = new List<Vector2Int>();
            outerCells = new List<Vector2Int>();

            switch (patchSize)
            {
                case 1:
                    cellPositions.AddRange(Patch0Positions);
                    cellPositions.AddRange(Patch1Positions);
                    outerCells.AddRange(cellPositions);
                    break;
                case 2:
                    cellPositions.AddRange(Patch0Positions);
                    cellPositions.AddRange(Patch1Positions);
                    cellPositions.AddRange(Patch2Positions);
                    outerCells.AddRange(Patch1Positions);
                    outerCells.AddRange(Patch2Positions);
                    break;
                case 3:
                    cellPositions.AddRange(Patch0Positions);
                    cellPositions.AddRange(Patch1Positions);
                    cellPositions.AddRange(Patch2Positions);
                    cellPositions.AddRange(Patch3Positions);
                    outerCells.AddRange(Patch3Positions);
                    break;
            }
        }

        private static List<Vector2Int> GeneratePatchShape(int patchSize,List<Vector2Int> blocked, GridChunk chunk)
        {
            
            GetPathBaseShape(patchSize,out List<Vector2Int> cellPositions, out List<Vector2Int> outerCells);
            
            Vector2Int  minAndMaxCellCount = new Vector2Int((int)(outerCells.Count / 2f + cellPositions.Count),
                (int)(cellPositions.Count * 2f));
            Vector2Int center;
            do
            {
                center = new Vector2Int(Random.Range(patchSize + 1, GridBuildingSystem.ChunkSize.x - patchSize - 1),
                    Random.Range(patchSize + 1, GridBuildingSystem.ChunkSize.y - patchSize - 1));
            } while (blocked.Contains(center));

            for (int i = 0; i < cellPositions.Count; i++) cellPositions[i] += center;
            for (int i = 0; i < outerCells.Count; i++) outerCells[i] += center;

            bool done = false;
            int emptyIterations = 0;

            do
            {
                List<Vector2Int> removeList = new List<Vector2Int>(),
                addList = new List<Vector2Int>();
                foreach (var outerCell in outerCells)
                {
                    if (cellPositions.Count + addList.Count >= minAndMaxCellCount.y) break;
                    if(blocked.Contains(outerCell)){removeList.Add(outerCell); continue;}
                    foreach (Vector2Int neighbourOffset in NeighbourOffsets)
                    {
                        if (cellPositions.Count + addList.Count >= minAndMaxCellCount.y) break;
                        Vector2Int newCell = outerCell + neighbourOffset;
                        if (cellPositions.Contains(newCell) || addList.Contains(newCell) ||
                            blocked.Contains(newCell+center)|| !chunk.BuildingGrid.IsValidPosition(newCell)) continue;
                        float prob = 1f / ((Vector2Int.Distance(Vector2Int.zero, newCell) + .5f) * 4f);
                        if (Random.Range(0f, 1f) >= prob) continue;
                        if (!removeList.Contains(outerCell)) removeList.Add(outerCell);
                        addList.Add(newCell);
                    }
                }

                foreach (var t in removeList) outerCells.Remove(t);
                foreach (var t in addList)
                {
                    outerCells.Add(t);
                    cellPositions.Add(t);
                }

                if (!addList.Any()) emptyIterations++;
                
                if (cellPositions.Count > minAndMaxCellCount.x|| emptyIterations >=20) done = true;

            } while (!done);

            return cellPositions;
        }

        private static int GetNumberOfChunkResources()
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

        private static int GetPatchSize(int numberOfPatches =1)
        {
            int returnVal = 1;
            float random = Random.Range(0f, 100f)-((numberOfPatches-1)*20);
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
        
        #endregion

        #region BuildingHandeling
        public enum PossibleBuildings
        {
            Drill,
            Smelter,
        }

        private static readonly BuildingDataBase[] PossibleBuildingData =
        {
            Resources.Load<BuildingDataBase>("Buildings/Data/test")
        };

        public static BuildingDataBase GetBuildingDataBase(PossibleBuildings buildingType)
        {
            return GetBuildingDataBase((int)buildingType);
        }
        
        public static BuildingDataBase GetBuildingDataBase(int buildingTypeID)
        {
            return PossibleBuildingData[buildingTypeID];
        }
        #endregion
    }
}
