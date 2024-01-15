using System.Collections.Generic;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.Grid;
using Unity.Collections;
using Unity.Entities;
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

        private static WorldDataAspect worldDataAspect;
        private static EndSimulationEntityCommandBufferSystem _endSimEntityCommandBufferSystem;
        private static Entity _backGround, _generationRequestHolder;
        private static GenerationRequestAspect _generationRequestAspect;

        private static Random _randomObj = new Random(0x6E624EB7u);
        
        
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
                state.EntityManager.SetName(_generationRequestHolder, "GenerationRequests");
                state.EntityManager.AddBuffer<ChunkGenerationRequestBuffElement>(_generationRequestHolder);

                generationComp.FirstUpdate = false;
            }
            else
            {
                _generationRequestAspect = SystemAPI.GetAspect<GenerationRequestAspect>(_generationRequestHolder);

                UpdatingPlayerState(state, generationComp);

                if (_generationRequestAspect.GetNumOfRequestCount() > 0)
                {
                    var ecb = _endSimEntityCommandBufferSystem.CreateCommandBuffer();

                    using NativeArray<ChunkGenerationRequestBuffElement> requests =
                        _generationRequestAspect.GetAllRequests();

                    PrefabsDataComponent prefabsDataComponent =
                        SystemAPI.GetComponent<PrefabsDataComponent>(prefabsEntity);
                    var genJob =
                        new GenerationOfChunkData(worldDataAspect, requests, _randomObj, ecb, prefabsDataComponent);

                    genJob.Execute();

                    _generationRequestAspect.ClearRequestList();
                }
            }

            SystemAPI.SetComponent(state.SystemHandle, generationComp);
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
