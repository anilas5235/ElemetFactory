using System.Collections.Generic;
using System.Linq;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Flags;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct GenerationSystem : ISystem
    {
        public const int WorldScale = 10;
        public const int ChunkSize = 16;
        public const int ChunkUnitSize = WorldScale * ChunkSize;

        public static GenerationSystem Instance;
        public static EntityManager entityManager;
        public static Entity worldDataEntity, prefabsEntity;
        public static WorldDataAspect worldDataAspect;

        private static EndSimulationEntityCommandBufferSystem _endSimEntityCommandBufferSystem;
        private static Entity _backGround, _generationRequestHolder;
        private static GenerationRequestAspect _generationRequestAspect;

        private static Random _randomObj = new Random(0x6E624EB7u);
        
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
            worldDataAspect = SystemAPI.GetAspect<WorldDataAspect>(worldDataEntity);
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

            using NativeArray<ChunkGenerationRequestBuffElement> requests = _generationRequestAspect.GetAllRequests();
            using var genChunkData = new NativeArray<ChunkGenTempData>(requests.Length, Allocator.TempJob);

            var genJob = new GenerationOfChunkData()
            {
                WorldDataAspect = worldDataAspect,
                Requests = requests,
                RandomGenerator = _randomObj,
                generatedChunks = genChunkData,
            };

            _generationRequestAspect.ClearRequestList();
            var handle = genJob.Schedule();
            handle.Complete();
            foreach (var chunkGenTempData in genChunkData) { GenerateChunk(chunkGenTempData,ecb,prefabsDataComponent); }
        }
        
        private static void GenerateChunk(ChunkGenTempData chunkGenTempData, EntityCommandBuffer ECB,PrefabsDataComponent prefabsComp)
        {
            Entity entity = ECB.CreateEntity();
            int2 chunkPosition = chunkGenTempData.position;
            float3 worldPos = GetChunkWorldPosition(chunkPosition);
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
    
    public struct GenerationOfChunkData : IJob
    {
        public WorldDataAspect WorldDataAspect;
        [NativeDisableContainerSafetyRestriction,ReadOnly] public NativeArray<ChunkGenerationRequestBuffElement> Requests;
        public Random RandomGenerator;
        [NativeDisableContainerSafetyRestriction] public NativeArray<ChunkGenTempData> generatedChunks ;
        
        public void Execute()
        {
            for (var index = 0; index < Requests.Length; index++)
            {
                var request = Requests[index];
                if (CheckIfChunkExitsWhileGenerating(request.ChunkPosition)) { continue; }

                generatedChunks[index] = (GenerateChunkGenTempData(request.ChunkPosition));
            }
        }

        private ChunkGenTempData GenerateChunkGenTempData(int2 chunkPosition)
        {
            return new ChunkGenTempData(chunkPosition,GenerateResources(chunkPosition));
        }
        
        #region GenerationSteps

        private ResourceType GetRandom(float distanceToCenter)
        {
            int pool = 1;
            if (distanceToCenter >= 2f) pool += 3;
            if (distanceToCenter >= 5f) pool += 1;
            return (ResourceType)RandomGenerator.NextInt(1, pool);
        }

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

        private float INT2Length(int2 vec)
        {
            return math.sqrt(vec.x * vec.x + vec.y * vec.y);
        }

       
        private int GetPatchSize(int numberOfPatches = 1)
        {
            int returnVal = 1;
            float random = RandomGenerator.NextFloat(0f, 100f) - ((numberOfPatches - 1) * 20);
            float currentStep = 0f;
            for (int i = 0; i < GenerationSystem.ResourcePatchSizeProbabilities.Length; i++)
            {
                if (GenerationSystem.ResourcePatchSizeProbabilities[i] == 0) continue;
                currentStep += GenerationSystem.ResourcePatchSizeProbabilities[i];
                if (random > currentStep) continue;
                returnVal += i;
                break;
            }

            return returnVal;
        }

        private void GetPathBaseShape(int patchSize, out NativeList<int2> cellPositions,
            out NativeList<int2> outerCells)
        {
            cellPositions = new NativeList<int2>(Allocator.Temp);
            outerCells = new NativeList<int2>(Allocator.Temp);

            switch (patchSize)
            {
                case 1:
                    cellPositions.AddRange(GenerationSystem.Patch0Positions);
                    cellPositions.AddRange(GenerationSystem.Patch1Positions);
                    outerCells.AddRange(cellPositions.AsArray());
                    break;
                case 2:
                    cellPositions.AddRange(GenerationSystem.Patch0Positions);
                    cellPositions.AddRange(GenerationSystem.Patch1Positions);
                    cellPositions.AddRange(GenerationSystem.Patch2Positions);
                    outerCells.AddRange(GenerationSystem.Patch1Positions);
                    outerCells.AddRange(GenerationSystem.Patch2Positions);
                    break;
                case 3:
                    cellPositions.AddRange(GenerationSystem.Patch0Positions);
                    cellPositions.AddRange(GenerationSystem.Patch1Positions);
                    cellPositions.AddRange(GenerationSystem.Patch2Positions);
                    cellPositions.AddRange(GenerationSystem.Patch3Positions);
                    outerCells.AddRange(GenerationSystem.Patch3Positions);
                    break;
            }
        }

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
            for (int i = 0; i < GenerationSystem.ChunkResourceNumberProbabilities.Length; i++)
            {
                if (GenerationSystem.ChunkResourceNumberProbabilities[i] == 0) continue;
                currentStep += GenerationSystem.ChunkResourceNumberProbabilities[i];
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

    public readonly struct ChunkGenTempData
    {
        public readonly int2 position;
        public readonly NativeArray<ResourcePatchTemp> patches;
        public int NumPatches => patches.Length;

        public ChunkGenTempData(int2 position, NativeArray<ResourcePatchTemp> patches)
        {
            this.position = position;
            this.patches = patches;
        }
    }
}
