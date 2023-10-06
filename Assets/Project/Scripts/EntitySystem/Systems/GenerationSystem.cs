using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [StructLayout(LayoutKind.Auto)]
    public partial struct GenerationSystem : ISystem
    {
        public static readonly int WorldScale = 10;
        
        public static GenerationSystem Instance;
        public static EntityManager _entityManager;
        public static Entity worldDataEntity, prefabsEntity;
        public static ComponentLookup<WorldDataComponent> worldDataLookup;
        
        private static System.Random _random = new System.Random();

        #region Consts

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


        #endregion

        private static int playerViewRadius;
        private static int2 chunkPosWithPlayer;
        private static List<int2> LoadedChunks;

        public void OnCreate(ref SystemState state)
        {
            Instance = this;
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            state.RequireForUpdate<PrefabsDataComponent>();
            state.RequireForUpdate<WorldDataComponent>();
            LoadedChunks = new List<int2>();
        }

        public void OnUpdate(ref SystemState state)
        {
            GridBuildingSystem.Work = true;
            if (worldDataEntity == default) worldDataEntity = SystemAPI.GetSingleton<WorldDataComponent>().entity;
            if (prefabsEntity == default)
            {
                prefabsEntity = SystemAPI.GetSingleton<PrefabsDataComponent>().entity;
            }
            worldDataLookup = SystemAPI.GetComponentLookup<WorldDataComponent>();
            
            Camera playerCam = GridBuildingSystem.Instance.PlayerCam;
            int2 currentPos = GetChunkPosition(playerCam.transform.position);
            int radius = Mathf.CeilToInt(playerCam.orthographicSize * playerCam.aspect / ChunkDataComponent.ChunkUnitSize);
            if (chunkPosWithPlayer.x == currentPos.x && chunkPosWithPlayer.y == currentPos.y && playerViewRadius == radius) return;
            chunkPosWithPlayer = new int2(currentPos.x,currentPos.y);
            playerViewRadius = radius;

            List<int2> chunksToLoad = new List<int2>();
            List<int2> chunksToUnLoad = new List<int2>();
            
            for (int x = -playerViewRadius; x < playerViewRadius+1; x++)
            {
                for (int y = -playerViewRadius; y < playerViewRadius+1; y++)
                { 
                    chunksToLoad.Add(new int2(x, y) + chunkPosWithPlayer);
                }
            }

            foreach (int2 loadedChunkPos in LoadedChunks)
            {
                if (chunksToLoad.Contains(loadedChunkPos)) chunksToLoad.Remove(loadedChunkPos);
                else chunksToUnLoad.Add(loadedChunkPos);
            }
            
            foreach (int2 pos in chunksToUnLoad)
            {
                ChunkDataAspect chunk = GetChunk(pos,out bool gen,ref state);
                LoadedChunks.Remove(pos);
                if(gen)continue;
                chunk.InView = false;
            }
            foreach (int2 pos in chunksToLoad)
            {
                ChunkDataAspect chunk = GetChunk(pos, out bool gen,ref state);
                LoadedChunks.Add(pos);
                if(gen)continue;
                chunk.InView = true;
            }
        }

        private Entity GenerateChunk(int2 chunkPosition)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
            Entity entity = _entityManager.CreateEntity();
            float3 worldPos = GetChunkWorldPosition(chunkPosition);
            ecb.SetName(entity,$"Chunk({chunkPosition.x},{chunkPosition.y})");
            ecb.AddComponent(entity, new LocalTransform()
            {
                Position = worldPos,
                Scale = WorldScale,
            });
            ResourcePatch[] patches = GenerateResources();
            _entityManager.AddComponentData(entity,new ChunkDataComponent(chunkPosition, worldPos,
                SystemAPI.GetSingleton<PrefabsDataComponent>(), patches,ecb));
            ecb.Playback(_entityManager);
            ecb.Dispose();
            return entity;

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
                    center = new int2(_random.Next(patchSize + 1, ChunkDataComponent.ChunkSize - patchSize - 1),
                        _random.Next(patchSize + 1, ChunkDataComponent.ChunkSize - patchSize - 1));
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
                    if (TryGetChunk(chunkPos, out ChunkDataAspect chunk))
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
        
        public static ChunkDataAspect GetChunk(int2 chunkPosition, out bool newGenerated, ref SystemState systemState)
        {
            newGenerated = false;
            if (TryGetChunk(chunkPosition, out ChunkDataAspect chunkDataAspect)) return chunkDataAspect;

            newGenerated = true;
            PositionChunkPair pair = new PositionChunkPair(Instance.GenerateChunk(chunkPosition),
                chunkPosition);
            worldDataLookup.Update(ref systemState);
            worldDataLookup.GetRefRW(worldDataEntity).ValueRW.ChunkDataBank.Add(pair);
            return default;
        }
        public static bool TryGetChunk(int2 chunkPos, out ChunkDataAspect chunkDataAspect)
        {
            chunkDataAspect = default;
            foreach (var pair in worldDataLookup.GetRefRO(worldDataEntity).ValueRO.ChunkDataBank)
            {
                if (pair.Position.x != chunkPos.x ||
                    pair.Position.y != chunkPos.y) continue;
                chunkDataAspect = pair.Chunk;
                return true;
            }
            return false;
        }

        public static float3 GetChunkWorldPosition(int2 chunkPosition)
        {
            float factor = ChunkDataComponent.ChunkSize * ChunkDataComponent.CellSize;
            return new float3((float2)chunkPosition * factor, 0);
        }


        public static int2 GetChunkPosition(float3 transformPosition)
        {
            return new int2(
                Mathf.RoundToInt(transformPosition.x / ChunkDataComponent.ChunkUnitSize),
                Mathf.RoundToInt(transformPosition.y / ChunkDataComponent.ChunkUnitSize));
        }
    }
}
