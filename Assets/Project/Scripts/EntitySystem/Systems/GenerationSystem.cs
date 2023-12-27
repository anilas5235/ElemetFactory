using System.Collections.Generic;
using System.Linq;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = System.Random;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct GenerationSystem : ISystem
    {
        public const int WorldScale = 10;
        public const int ChunkSize = 16;

        public static GenerationSystem Instance;
        public static EntityManager entityManager;
        public static Entity worldDataEntity, prefabsEntity;
        public static WorldDataAspect worldDataAspect;

        private static EndSimulationEntityCommandBufferSystem _endSimEntityCommandBufferSystem;
        private static Entity _backGround, _generationRequestHolder;
        private static GenerationRequestAspect _generationRequestAspect;
        private static Unity.Mathematics.Random _random = new();
        public void OnCreate(ref SystemState state)
        {
            Instance = this;
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            state.RequireForUpdate<PrefabsDataComponent>();
            state.EntityManager.AddComponentData(state.SystemHandle, new GenerationSystemComponent()
            {
                PlayerViewRadius =1,
                ViewSize = WorldScale * ChunkSize * 9,
                ChunkPosWithPlayer = new int2(-1000, -1000),
                LoadedChunks = new NativeList<int2>(Allocator.Persistent),
                FirstUpdate = true,
            });
            
            _endSimEntityCommandBufferSystem = state.World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var generationComp = SystemAPI.GetComponent<GenerationSystemComponent>(state.SystemHandle);
            
            if (generationComp.FirstUpdate)
            {
                GridBuildingSystem.Work = true;
                prefabsEntity = SystemAPI.GetSingleton<PrefabsDataComponent>().entity;

                _backGround = state.EntityManager.Instantiate(state.EntityManager
                    .GetComponentData<PrefabsDataComponent>(prefabsEntity).TileVisual);
                state.EntityManager.SetName(_backGround, "BackGroundTile");

                _generationRequestHolder = state.EntityManager.CreateEntity();
                state.EntityManager.SetName(_generationRequestHolder,"GenerationRequests");
                state.EntityManager.AddBuffer<ChunkGenerationRequestBuffElement>(_generationRequestHolder);

                worldDataAspect = SystemAPI.GetAspect<WorldDataAspect>(worldDataEntity);
                
                generationComp.FirstUpdate = false;
            }
            else
            {
                _generationRequestAspect = SystemAPI.GetAspect<GenerationRequestAspect>(_generationRequestHolder);

                UpdatingPlayerState(state, generationComp);

                if (_generationRequestAspect.GetNumOfRequestCount() > 0)
                {
                    CreateGenerationJob(SystemAPI.GetComponent<PrefabsDataComponent>(prefabsEntity));
                }
            }
            
            SystemAPI.SetComponent(state.SystemHandle, generationComp);
        }

        private static void CreateGenerationJob(PrefabsDataComponent prefabsDataComponent)
        {
            var ecb = _endSimEntityCommandBufferSystem.CreateCommandBuffer();

            var genJob = new Generation()
            {
                ECB = ecb,
                WorldScale = WorldScale,
                WorldDataAspect = worldDataAspect,
                Requests = _generationRequestAspect.GetAllRequests(),
                prefabsComp = prefabsDataComponent,
                _Random = _random,
            };

            _generationRequestAspect.ClearRequestList();
            _endSimEntityCommandBufferSystem.AddJobHandleForProducer(genJob.Schedule());
        }

        private void UpdatingPlayerState(SystemState state, GenerationSystemComponent generationComp)
        {
            Camera playerCam = GridBuildingSystem.Instance.PlayerCam;
            int2 currentPos = GetChunkPosition(playerCam.transform.position);
            int radius =
                Mathf.CeilToInt(playerCam.orthographicSize * playerCam.aspect /
                                (ChunkDataComponent.ChunkSize * WorldScale));
            if (generationComp.ChunkPosWithPlayer.x == currentPos.x &&
                generationComp.ChunkPosWithPlayer.y == currentPos.y &&
                generationComp.PlayerViewRadius == radius) return;

            generationComp.ChunkPosWithPlayer = currentPos.xy;

            float3 backgroundPos = new float3(GetChunkWorldPosition(currentPos).xy, 2);


            state.EntityManager.SetComponentData(_backGround, new LocalTransform
            {
                Position = backgroundPos,
                Scale = generationComp.ViewSize,
            });

            generationComp.PlayerViewRadius = radius;

            List<int2> chunksToLoad = new List<int2>();
            List<int2> chunksToUnLoad = new List<int2>();

            int playerViewRadius = generationComp.PlayerViewRadius;
            int2 chunkPosWithPlayer = generationComp.ChunkPosWithPlayer;
            for (int x = -playerViewRadius; x < playerViewRadius + 1; x++)
            {
                for (int y = -playerViewRadius; y < playerViewRadius + 1; y++)
                {
                    chunksToLoad.Add(new int2(x, y) + chunkPosWithPlayer);
                }
            }

            foreach (int2 loadedChunkPos in generationComp.LoadedChunks)
            {
                if (chunksToLoad.Contains(loadedChunkPos)) chunksToLoad.Remove(loadedChunkPos);
                else chunksToUnLoad.Add(loadedChunkPos);
            }

            foreach (int2 pos in chunksToUnLoad)
            {
                if (TryGetChunk(pos, out var chunk))
                {
                    chunk.InView = false;
                }

                for (int i = 0; i < generationComp.LoadedChunks.Length; i++)
                {
                    var condition = generationComp.LoadedChunks[i] == pos;
                    if (condition is not { x: true, y: true }) continue;

                    generationComp.LoadedChunks.RemoveAt(i);
                    break;
                }
            }

            foreach (int2 pos in chunksToLoad)
            {
                if (TryGetChunk(pos, out var chunk))
                {
                    chunk.InView = true;
                }

                generationComp.LoadedChunks.Add(pos);
            }
        }

        public bool TryGetChunk(int2 chunkPos, out ChunkDataAspect chunkDataAspect)
        {
            if (worldDataAspect.TryGetChunk(chunkPos, out chunkDataAspect)) { return true; }

            _generationRequestAspect.AddRequest(chunkPos);
            return false;

        }

        #region Helpers

        public static float3 GetChunkWorldPosition(int2 chunkPosition)
        {
            float factor = ChunkDataComponent.ChunkSize * ChunkDataComponent.CellSize;
            return new float3((float2)chunkPosition * factor, 0);
        }

        public static int2 GetChunkPosition(float3 transformPosition)
        {
            return new int2(
                Mathf.RoundToInt(transformPosition.x / (ChunkDataComponent.ChunkSize * WorldScale)),
                Mathf.RoundToInt(transformPosition.y / (ChunkDataComponent.ChunkSize * WorldScale)));
        }
        
        #endregion
    }
    
    public struct Generation : IJob
    {
        public EntityCommandBuffer ECB;
        public int WorldScale;
        public WorldDataAspect WorldDataAspect;
        public NativeArray<ChunkGenerationRequestBuffElement> Requests;
        public PrefabsDataComponent prefabsComp;
        public Unity.Mathematics.Random _Random;
        public NativeList<PositionChunkPair> generatedChunks;

        #region Consts

        private static readonly float[] ResourcePatchSizeProbabilities = { 60f, 39f, 1f };
        private static readonly float[] ChunkResourceNumberProbabilities = { 70f, 25f, 5f };

        private static readonly NativeArray<int2> Patch0Positions = new(new int2[] { new(0, 0) }, Allocator.Persistent);

        private static readonly NativeArray<int2> Patch1Positions = new(
            new int2[] { new(0, 1), new(1, 1), new(1, 0) }, Allocator.Persistent);

        private static readonly NativeArray<int2> Patch2Positions = new(new int2[]
        {
            new(-1, 1), new(-1, 0), new(-1, -1), new(0, -1),
            new(1, -1),
        }, Allocator.Persistent);

        private static readonly NativeArray<int2> Patch3Positions = new(new int2[]
        {
            new(-2, 0), new(-2, 1), new(-2, -1), new(2, -1),
            new(2, 1), new(2, 0), new(-1, -2), new(1, -2),
            new(0, -2), new(-1, 2), new(1, 2), new(0, 2),
        }, Allocator.Persistent);


        #endregion

        private static float3 GetChunkWorldPosition(int2 chunkPosition)
        {
            float factor = ChunkDataComponent.ChunkSize * ChunkDataComponent.CellSize;
            return new float3((float2)chunkPosition * factor, 0);
        }

        public void Execute()
        {
            foreach (var request in Requests)
            {
                if (CheckIfChunkExitsWhileGenerating(request.ChunkPosition)) { continue; }
                generatedChunks.Add(GenerateChunk(request.ChunkPosition));
            }
            
            WorldDataAspect.AddChunksToData(generatedChunks.AsArray());
        }

        private PositionChunkPair GenerateChunk(int2 chunkPosition)
        {
            Entity entity = ECB.CreateEntity();
            float3 worldPos = GetChunkWorldPosition(chunkPosition);
            ECB.SetName(entity, $"Chunk({chunkPosition.x},{chunkPosition.y})");
            ECB.AddComponent(entity, new LocalTransform()
            {
                Position = worldPos,
                Scale = WorldScale,
            });
            using var patches = GenerateResources(chunkPosition);
            ECB.AddComponent(entity, new ChunkDataComponent(entity, chunkPosition, worldPos,
                prefabsComp, patches, ECB));
            return new PositionChunkPair(entity, chunkPosition);
        }

        private ResourceType GetRandom(float distanceToCenter)
        {
            int pool = 1;
            if (distanceToCenter >= 2f) pool += 3;
            if (distanceToCenter >= 5f) pool += 1;
            return (ResourceType)_Random.NextInt(1, pool);
        }

        private ResourcePatch GenerateResourcePatch(int patchSize, ResourceType resourceType, NativeList<int2> blocked)
        {
            using NativeList<int2> cellPositions = GeneratePatchShape(patchSize, blocked);

            blocked.AddRange(cellPositions.AsArray());

            return new ResourcePatch()
            {
                Positions = new NativeArray<int2>(cellPositions.ToArray(), Allocator.Persistent),
                Resource = ResourcesUtility.CreateItemData(new[] { (uint)resourceType })
            };
        }

        private float INT2Length(int2 vec)
        {
            return math.sqrt(vec.x * vec.x + vec.y * vec.y);
        }

        private int GetPatchSize(int numberOfPatches = 1)
        {
            int returnVal = 1;
            float random = _Random.NextFloat(0f, 100f) - ((numberOfPatches - 1) * 20);
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

        private void GetPathBaseShape(int patchSize, out NativeList<int2> cellPositions,
            out NativeList<int2> outerCells)
        {
            cellPositions = new NativeList<int2>();
            outerCells = new NativeList<int2>();

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

        private NativeArray<ResourcePatch> GenerateResources(int2 chunkPosition)
        {
            float distToCenter = math.sqrt(chunkPosition.x * chunkPosition.x + chunkPosition.y * chunkPosition.y);
            int numberOfPatches = GetNumberOfChunkResources(7f, chunkPosition);
            if (distToCenter < 2f || numberOfPatches < 1) return new NativeArray<ResourcePatch>();
            NativeArray<ResourcePatch> resourcePatches =
                new NativeArray<ResourcePatch>(numberOfPatches, Allocator.TempJob);
            NativeArray<ResourceType> chunkResources =
                new NativeArray<ResourceType>(numberOfPatches, Allocator.TempJob);
            NativeList<int2> blockPositions = new NativeList<int2>();

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

        private NativeList<int2> GeneratePatchShape(int patchSize, NativeList<int2> blocked)
        {

            GetPathBaseShape(patchSize, out var cellPositions, out var outerCells);

            int2 minAndMaxCellCount = new int2((int)(outerCells.Length / 2f + cellPositions.Length),
                (int)(cellPositions.Length * 2f));
            int2 center;
            do
            {
                center = new int2(_Random.NextInt(patchSize + 1, ChunkDataComponent.ChunkSize - patchSize - 1),
                    _Random.NextInt(patchSize + 1, ChunkDataComponent.ChunkSize - patchSize - 1));
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
                        if (_Random.NextFloat(0f, 100f) / 100f >= prob) continue;
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

                if (!addList.Any()) emptyIterations++;

                if (cellPositions.Length > minAndMaxCellCount.x || emptyIterations >= 20) done = true;

            } while (!done);

            return cellPositions;
        }

        private int GetNumberOfChunkResources(float antiCrowdingMultiplier, int2 chunkPosition)
        {
            int returnVal = 0;
            float randomNum = _Random.NextFloat(0f, 100f);

            foreach (Vector2Int neighbourOffset in GeneralConstants.NeighbourOffsets2D8)
            {
                int2 chunkPos = new int2(neighbourOffset.x, neighbourOffset.y) + chunkPosition;
                if (TryGetChunkWhileGenerating(chunkPos, out ChunkDataAspect chunk))
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

        private bool TryGetChunkWhileGenerating(int2 chunkPosition, out ChunkDataAspect chunk)
        {
            if (WorldDataAspect.TryGetChunk(chunkPosition, out chunk)){ return true;}

            foreach (PositionChunkPair chunkPair in generatedChunks)
            {
               var condition = chunkPair.Position == chunkPosition;
               if (condition is not { x: true, y: true }) continue;
               chunk = chunkPair.Chunk;
               return true;
            }

            return false;
        }

        private bool CheckIfChunkExitsWhileGenerating(int2 chunkPosition)
        {
            if (WorldDataAspect.ChunkExits(chunkPosition)){ return true;}

            foreach (PositionChunkPair chunkPair in generatedChunks)
            {
                var condition = chunkPair.Position == chunkPosition;
                if (condition is not { x: true, y: true }) continue;
                return true;
            }

            return false;
        }
    }
}
