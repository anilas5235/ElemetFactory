using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    public partial struct GenerationSystem : ISystem
    {
        public static GenerationSystem Instance;
        private static EntityManager _entityManager;
        
        private static System.Random _random = new System.Random();

        private static readonly float[] ResourcePatchSizeProbabilities = { 60f, 39f, 1f };
        private static readonly float[] ChunkResourceNumberProbabilities = { 70f, 25f, 5f };

        private static readonly int2[] Patch0Positions = { new int2(0, 0) };

        private static readonly int2[] Patch1Positions =
            { new int2(0, 1), new int2(1, 1), new int2(1, 0) };

        private static readonly int2[] Patch2Positions =
        {
            new int2(-1, 1), new int2(-1, 0), new int2(-1, -1), new int2(0, -1),
            new int2(1, -1),
        };

        private static readonly int2[] Patch3Positions =
        {
            new int2(-2, 0), new int2(-2, 1), new int2(-2, -1), new int2(2, -1),
            new int2(2, 1), new int2(2, 0), new int2(-1, -2), new int2(1, -2),
            new int2(0, -2), new int2(-1, 2), new int2(1, 2), new int2(0, 2),
        };

        public void OnCreate(ref SystemState state)
        {
            Instance = this;
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            state.RequireForUpdate<PrefapsDataComponent>();
        }

        public void UpdateInView(Vector2Int[]chunksEnteringView, Vector2Int[]chunksExitingView)
        {
            
        }

        public ChunkDataAspect GenerateChunk(int2 chunkPosition, WorldDataAspect worldDataAspect)
        {
            var ecb = new EntityCommandBuffer();
            Entity entity = ecb.CreateEntity();
            float3 worldPos = WorldDataAspect.GetChunkWorldPosition(chunkPosition);
            ecb.AddComponent(entity, new LocalTransform()
            {
                Position = worldPos,
                Scale = 1,
            });
            ResourcePatch[] patches = GenerateResources();
            ecb.AddComponent(entity, new ChunkDataComponent(chunkPosition, worldPos, ecb,
                SystemAPI.GetSingleton<PrefapsDataComponent>().TileVisual, patches));
            ecb.Playback(_entityManager);
            ecb.Dispose();
            return SystemAPI.GetAspect<ChunkDataAspect>(entity);


            ResourcePatch[] GenerateResources()
            {
                float distToCenter = math.sqrt(chunkPosition.x * chunkPosition.x + chunkPosition.y * chunkPosition.y);
                if (distToCenter < 2f) return Array.Empty<ResourcePatch>();
                int numberOfPatches = GetNumberOfChunkResources(7f);
                if (numberOfPatches < 1) return Array.Empty<ResourcePatch>();
                ResourcePatch[] resourcePatches = new ResourcePatch[numberOfPatches];
                ResourceType[] chunkResources = new ResourceType[numberOfPatches];
                List<int2> blockPositions = new List<int2>();

                for (int i = 0; i < numberOfPatches; i++)
                {
                    bool done;
                    do
                    {
                        ResourceType type = GetRandom(distToCenter);
                        done = chunkResources.All(resource => type != resource);
                        if (!done) continue;
                        chunkResources[i] = type;
                        resourcePatches[i] = GenerateResourcePatch(GetPatchSize(numberOfPatches), type, blockPositions);
                    } while (!done);
                }

                return resourcePatches.ToArray();
            }

            ResourceType GetRandom(float distanceToCenter)
            {
                int pool = 1;
                if (distanceToCenter >= 2f) pool += 3;
                if (distanceToCenter >= 5f) pool += 1;
                return (ResourceType)_random.Next(1, pool);
            }

            ResourcePatch GenerateResourcePatch(int patchSize, ResourceType resourceType, List<int2> blocked)
            {
                List<int2> cellPositions = GeneratePatchShape(patchSize, blocked);

                blocked.AddRange(cellPositions);

                return new ResourcePatch()
                {
                    Positions = new NativeArray<int2>(cellPositions.ToArray(), Allocator.Persistent),
                    Resource = ResourcesUtility.CreateItemData(new[] { (uint)resourceType })
                };
            }

            void GetPathBaseShape(int patchSize, out List<int2> cellPositions, out List<int2> outerCells)
            {
                cellPositions = new List<int2>();
                outerCells = new List<int2>();

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

            List<int2> GeneratePatchShape(int patchSize, List<int2> blocked)
            {

                GetPathBaseShape(patchSize, out List<int2> cellPositions, out List<int2> outerCells);

                int2 minAndMaxCellCount = new int2((int)(outerCells.Count / 2f + cellPositions.Count),
                    (int)(cellPositions.Count * 2f));
                int2 center;
                do
                {
                    center = new int2(_random.Next(patchSize + 1, GridBuildingSystem.ChunkSize.x - patchSize - 1),
                        _random.Next(patchSize + 1, GridBuildingSystem.ChunkSize.y - patchSize - 1));
                } while (blocked.Contains(center));

                for (int i = 0; i < cellPositions.Count; i++) cellPositions[i] += center;
                for (int i = 0; i < outerCells.Count; i++) outerCells[i] += center;

                bool done = false;
                int emptyIterations = 0;

                do
                {
                    List<int2> removeList = new List<int2>(),
                        addList = new List<int2>();
                    foreach (var outerCell in outerCells)
                    {
                        if (cellPositions.Count + addList.Count >= minAndMaxCellCount.y) break;
                        if (blocked.Contains(outerCell))
                        {
                            removeList.Add(outerCell);
                            continue;
                        }

                        foreach (Vector2Int neighbourOffset in GeneralConstants.NeighbourOffsets2D4)
                        {
                            if (cellPositions.Count + addList.Count >= minAndMaxCellCount.y) break;
                            int2 newCell = outerCell + new int2(neighbourOffset.x, neighbourOffset.y);
                            if (cellPositions.Contains(newCell) || addList.Contains(newCell) ||
                                blocked.Contains(newCell + center) ||
                                !ChunkDataAspect.IsValidPositionInChunk(newCell)) continue;
                            float prob = 1f / (INT2Length(newCell) + .5f) * 4f;
                            if (_random.Next(0, 100) / 100f >= prob) continue;
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

                    if (cellPositions.Count > minAndMaxCellCount.x || emptyIterations >= 20) done = true;

                } while (!done);

                return cellPositions;
            }

            int GetNumberOfChunkResources(float antiCrowdingMultiplier)
            {
                int returnVal = 0;
                float random = _random.Next(0, 100);

                foreach (Vector2Int neighbourOffset in GeneralConstants.NeighbourOffsets2D8)
                {
                    int2 chunkPos = new int2(neighbourOffset.x, neighbourOffset.y) + chunkPosition;
                    if (worldDataAspect.TryGetValue(chunkPos, out var chunk))
                    {
                        random -= chunk.NumPatches * antiCrowdingMultiplier;
                    }
                }

                float currentStep = 0f;
                for (int i = 0; i < ChunkResourceNumberProbabilities.Length; i++)
                {
                    if (ChunkResourceNumberProbabilities[i] == 0) continue;
                    currentStep += ChunkResourceNumberProbabilities[i];
                    if (random > currentStep) continue;
                    returnVal += i;
                    break;
                }

                return returnVal;
            }

            int GetPatchSize(int numberOfPatches = 1)
            {
                int returnVal = 1;
                float random = (float)_random.Next(0, 100) - ((numberOfPatches - 1) * 20);
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

            float INT2Length(int2 vec)
            {
                return math.sqrt(vec.x * vec.x + vec.y * vec.y);
            }
        }
    }
}
