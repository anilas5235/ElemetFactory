using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Flags;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Project.Scripts.EntitySystem.Systems
{
    public class GenerationOfChunkData 
    {
        private WorldDataAspect WorldDataAspect;
        private NativeArray<ChunkGenerationRequestBuffElement> Requests;
        private Random RandomGenerator;
        private NativeList<ChunkGenTempData> generatedChunks;
        private EntityCommandBuffer ECB;
        private PrefabsDataComponent prefabsComp;

        private static float WorldScale => GenerationSystem.WorldScale;
        
        #region Consts

        public static readonly float[] ResourcePatchSizeProbabilities = { 60f, 39f, 1f };
        public static readonly float[] ChunkResourceNumberProbabilities = { 70f, 25f, 5f };

        public static readonly NativeArray<int2> Patch0Positions = new(new int2[] { new(0, 0) }, Allocator.Persistent);

        public static readonly NativeArray<int2> Patch1Positions = new(
            new int2[] { new(0, 1), new(1, 1), new(1, 0) }, Allocator.Persistent);

        public static readonly NativeArray<int2> Patch2Positions = new(new int2[]
        {
            new(-1, 1), new(-1, 0), new(-1, -1), new(0, -1),
            new(1, -1),
        }, Allocator.Persistent);

        public static readonly NativeArray<int2> Patch3Positions = new(new int2[]
        {
            new(-2, 0), new(-2, 1), new(-2, -1), new(2, -1),
            new(2, 1), new(2, 0), new(-1, -2), new(1, -2),
            new(0, -2), new(-1, 2), new(1, 2), new(0, 2),
        }, Allocator.Persistent);
        #endregion

        public GenerationOfChunkData(WorldDataAspect worldDataAspect, NativeArray<ChunkGenerationRequestBuffElement> requests, Random randomGenerator, EntityCommandBuffer ecb, PrefabsDataComponent prefabsComp)
        {
            WorldDataAspect = worldDataAspect;
            Requests = requests;
            RandomGenerator = randomGenerator;
            generatedChunks = new NativeList<ChunkGenTempData>(Allocator.TempJob);
            ECB = ecb;
            this.prefabsComp = prefabsComp;
        }

        public void Execute()
        {
            foreach (var request in Requests)
            {
                if (CheckIfChunkExitsWhileGenerating(request.ChunkPosition)) { continue; }

                var chunkData = GenerateChunkGenTempData(request.ChunkPosition);
                
                generatedChunks.Add(chunkData);
                GenerateChunk(chunkData);
            }

            generatedChunks.Dispose();
        }

        private ChunkGenTempData GenerateChunkGenTempData(int2 chunkPosition)
        {
            return new ChunkGenTempData(chunkPosition,GenerateResources(chunkPosition));
        }
        
        private void GenerateChunk(ChunkGenTempData chunkGenTempData)
        {
            Entity entity = ECB.CreateEntity();
            int2 chunkPosition = chunkGenTempData.position;
            float3 worldPos = GenerationSystem.GetChunkWorldPosition(chunkPosition);
            ECB.SetName(entity, $"Ch({chunkPosition.x},{chunkPosition.y})");
            ECB.AddComponent(entity, new LocalTransform()
            {
                Position = worldPos,
                Scale = WorldScale,
            });
            var patches = new NativeArray<ResourcePatch>(chunkGenTempData.patches.Length,Allocator.Temp);

            for (var index = 0; index < chunkGenTempData.patches.Length; index++)
            {
                var tempData = chunkGenTempData.patches[index];
                patches[index] = new ResourcePatch()
                {
                    Positions = tempData.Positions,
                    Resource = ResourcesUtility.CreateItemData(tempData.ResourceIDs)
                };
            }

            ECB.AddComponent(entity, new ChunkDataComponent(entity, chunkPosition, worldPos,
                prefabsComp,patches, ECB));
            patches.Dispose();
            ECB.AddComponent(entity,new NewChunkDataComponent(chunkGenTempData.position,chunkGenTempData.patches.Length));
        }

        #region GenerationSteps

        private ResourceType GetRandom(float distanceToCenter)
        {
            int pool = 1;
            if (distanceToCenter >= 2f) pool += 3;
            if (distanceToCenter >= 5f) pool += 1;
            return (ResourceType)RandomGenerator.NextInt(1, pool);
        }
        private float INT2Length(int2 vec)
        {
            return math.sqrt(vec.x * vec.x + vec.y * vec.y);
        }

        #region PatchGeneration
        private ResourcePatchTemp GenerateResourcePatch(int patchSize, ResourceType resourceType, NativeList<int2> blocked)
        {
            using NativeList<int2> cellPositions = GeneratePatchShape(patchSize, blocked);

            blocked.AddRange(cellPositions.AsArray());

            return new ResourcePatchTemp()
            {
                Positions = new NativeArray<int2>(cellPositions.AsArray(), Allocator.Temp),
                ResourceIDs = new NativeArray<uint>(new[] { (uint)resourceType },Allocator.Temp)
            };
        }
        private int GetPatchSize(int numberOfPatches = 1)
        {
            int returnVal = 1;
            float random = RandomGenerator.NextFloat(0f, 100f) - ((numberOfPatches - 1) * 20);
            float currentStep = 0f;
            for (int i = 0; i < ResourcePatchSizeProbabilities.Length; i++)
            {
                if (ResourcePatchSizeProbabilities[i] == 0) continue;
                currentStep += ResourcePatchSizeProbabilities[i];
                if (random > currentStep) continue;
                returnVal += i;
                break;
            }

            return returnVal;
        }
        
        private NativeList<int2> GeneratePatchShape(int patchSize, NativeList<int2> blocked)
        {

            GetPathBaseShape(patchSize, out var cellPositions, out var outerCells);

            int2 minAndMaxCellCount = new int2((int)(outerCells.Length / 2f + cellPositions.Length),
                (int)(cellPositions.Length * 2f));
            int2 center;
            do
            {
                center = new int2(RandomGenerator.NextInt(patchSize + 1, ChunkDataComponent.ChunkSize - patchSize - 1),
                    RandomGenerator.NextInt(patchSize + 1, ChunkDataComponent.ChunkSize - patchSize - 1));
            } while (blocked.Contains(center));

            for (int i = 0; i < cellPositions.Length; i++) cellPositions[i] += center;
            for (int i = 0; i < outerCells.Length; i++) outerCells[i] += center;

            bool done = false;
            int emptyIterations = 0;

            do
            {
                using NativeList<int2> removeList = new(Allocator.TempJob);
                using NativeList<int2> addList = new(Allocator.TempJob);
                foreach (var outerCell in outerCells)
                {
                    if (cellPositions.Length + addList.Length >= minAndMaxCellCount.y) break;
                    if (blocked.Contains(outerCell))
                    {
                        removeList.Add(outerCell);
                        continue;
                    }

                    foreach (Vector2Int neighbourOffset in GeneralConstants.NeighbourOffsets2D4)
                    {
                        if (cellPositions.Length + addList.Length >= minAndMaxCellCount.y) break;
                        int2 newCell = outerCell + new int2(neighbourOffset.x, neighbourOffset.y);
                        if (cellPositions.Contains(newCell) || addList.Contains(newCell) ||
                            blocked.Contains(newCell + center) ||
                            !ChunkDataAspect.IsValidPositionInChunk(newCell)) continue;
                        float prob = 1f / (INT2Length(newCell) + .5f) * 4f;
                        if (RandomGenerator.NextFloat(0f, 100f) / 100f >= prob) continue;
                        if (!removeList.Contains(outerCell)) removeList.Add(outerCell);
                        addList.Add(newCell);
                    }
                }

                foreach (var t in removeList)
                {
                    for (int i = 0; i < outerCells.Length; i++)
                    {
                        var condition = outerCells[i] == t;
                        if (condition is { x: true, y: true })
                        {
                            outerCells.RemoveAt(i);
                        }
                    }
                }

                foreach (var t in addList)
                {
                    outerCells.Add(t);
                    cellPositions.Add(t);
                }

                if (addList.Length < 1) emptyIterations++;

                if (cellPositions.Length > minAndMaxCellCount.x || emptyIterations >= 20) done = true;

            } while (!done);

            return cellPositions;
        }

        private void GetPathBaseShape(int patchSize, out NativeList<int2> cellPositions,
            out NativeList<int2> outerCells)
        {
            cellPositions = new NativeList<int2>(Allocator.Temp);
            outerCells = new NativeList<int2>(Allocator.Temp);

            switch (patchSize)
            {
                case 1:
                    cellPositions.AddRange(Patch0Positions);
                    cellPositions.AddRange(Patch1Positions);
                    outerCells.AddRange(cellPositions.AsArray());
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
        #endregion
        private NativeArray<ResourcePatchTemp> GenerateResources(int2 chunkPosition)
        {
            float distToCenter = math.sqrt(chunkPosition.x * chunkPosition.x + chunkPosition.y * chunkPosition.y);
            int numberOfPatches = GetNumberOfChunkResources(7f, chunkPosition);
            if (distToCenter < 2f || numberOfPatches < 1) return new NativeArray<ResourcePatchTemp>();
            NativeArray<ResourcePatchTemp> resourcePatches =
                new NativeArray<ResourcePatchTemp>(numberOfPatches, Allocator.Temp);
            NativeArray<ResourceType> chunkResources =
                new NativeArray<ResourceType>(numberOfPatches, Allocator.Temp);
            NativeList<int2> blockPositions = new NativeList<int2>(Allocator.Temp);

            for (int i = 0; i < numberOfPatches; i++)
            {
                bool done;
                do
                {
                    ResourceType type = GetRandom(distToCenter);
                    done = true;
                    foreach (var resource in chunkResources)
                    {
                        if (type == resource)
                        {
                            done = false;
                            break;
                        }
                    }

                    if (!done) continue;
                    chunkResources[i] = type;
                    resourcePatches[i] = GenerateResourcePatch(GetPatchSize(numberOfPatches), type, blockPositions);
                } while (!done);
            }

            return resourcePatches;
        }
        private int GetNumberOfChunkResources(float antiCrowdingMultiplier, int2 chunkPosition)
        {
            int returnVal = 0;
            float randomNum = RandomGenerator.NextFloat(0f, 100f);

            foreach (Vector2Int neighbourOffset in GeneralConstants.NeighbourOffsets2D8)
            {
                int2 chunkPos = new int2(neighbourOffset.x, neighbourOffset.y) + chunkPosition;
                if (TryGetChunkWhileGenerating(chunkPos, out var chunk))
                {
                    randomNum -= chunk.NumPatches * antiCrowdingMultiplier;
                }
            }

            float currentStep = 0f;
            for (int i = 0; i < ChunkResourceNumberProbabilities.Length; i++)
            {
                if (ChunkResourceNumberProbabilities[i] == 0) continue;
                currentStep += ChunkResourceNumberProbabilities[i];
                if (randomNum > currentStep) continue;
                returnVal += i;
                break;
            }

            return returnVal;
        }

        private bool TryGetChunkWhileGenerating(int2 chunkPosition, out ChunkGenTempData chunk)
        {
            chunk = default;
            if (WorldDataAspect.TryGetPositionChunkPair(chunkPosition, out var pair))
            {
                chunk = new ChunkGenTempData(chunkPosition,
                    new NativeArray<ResourcePatchTemp>(pair.NumOfPatches, Allocator.Temp));
                return true;
            }

            foreach (var chunkData in generatedChunks)
            {
                var condition = chunkData.position == chunkPosition;
                if (condition is not { x: true, y: true }) continue;
                chunk = chunkData;
                return true;
            }

            return false;
        }

        private bool CheckIfChunkExitsWhileGenerating(int2 chunkPosition)
        {
            if (WorldDataAspect.ChunkExits(chunkPosition)){ return true;}

            foreach (var chunkData in generatedChunks)
            {
                var condition = chunkData.position == chunkPosition;
                if (condition is not { x: true, y: true }) continue;
                return true;
            }

            return false;
        }
        #endregion
    }
}