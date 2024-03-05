using System.Collections.Generic;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.DataObject;
using Project.Scripts.EntitySystem.Components.Flags;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.General;
using Project.Scripts.Grid.DataContainers;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct WorldSetUpSystem : ISystem
    {
        public static WorldSetUpSystem Instance;
        private static EndInitializationEntityCommandBufferSystem _endSimEntityCommandBufferSystem;
        private static Entity worldDataEntity;
        public void OnCreate(ref SystemState state)
        {
            worldDataEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<PositionChunkPair>(worldDataEntity);
            
            state.EntityManager.SetName(worldDataEntity,"WorldData");
            GenerationSystem.worldDataEntity = worldDataEntity;
            
            _endSimEntityCommandBufferSystem = state.World.GetOrCreateSystemManaged<EndInitializationEntityCommandBufferSystem>();
            state.RequireForUpdate<ItemPrefabsDataComponent>();
            state.RequireForUpdate<TilePrefabsDataComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var worldData =  WorldSaveHandler.GetWorldSave();

            var ecb = _endSimEntityCommandBufferSystem.CreateCommandBuffer();

            var builder = new WorldBuilder(ecb,worldData,SystemAPI.GetSingleton<TilePrefabsDataComponent>().TilePrefab);
            
            builder.Execute();
          
            state.Enabled = false;
        }

        public void OnDestroy(ref SystemState state)
        {
            var save = new WorldSaver(SystemAPI.GetAspect<WorldDataAspect>(GenerationSystem.worldDataEntity),
                SystemAPI.GetComponentLookup<BuildingDataComponent>());
            save.Execute();
        }
    }

    public class WorldSaver
    {
        private ComponentLookup<BuildingDataComponent> _buildingsLookUp;
        private readonly WorldDataAspect _worldDataAspect;

        public WorldSaver(WorldDataAspect worldDataAspect, ComponentLookup<BuildingDataComponent> buildingsLookUp)
        {
            _worldDataAspect = worldDataAspect;
            _buildingsLookUp = buildingsLookUp;
        }

        public void Execute()
        {
            var chunkSaves = new List<ChunkSave>();
            var chunks = new NativeList<ChunkDataAspect>(Allocator.Temp);
            
            _worldDataAspect.GetAllChunks(chunks);

            foreach (var chunkDataAspect in chunks)
            {
                var newChunkData = new ChunkSave();
                var patches = chunkDataAspect._chunkData.ValueRO.ResourcePatches.ToArray();
                newChunkData.chunkResourcePatches = new ChunkResourcePatch[patches.Length];
                for (var i = 0; i < patches.Length; i++)
                {
                    var patchData = new ChunkResourcePatch
                    {
                        positions = patches[i].Positions.ToArray(),
                        resourceID = patches[i].ItemID,
                    };
                    newChunkData.chunkResourcePatches[i] = patchData;
                }

                newChunkData.placedBuildingData = new PlacedBuildingData[chunkDataAspect._buildings.Length];

                for (var i = 0; i < chunkDataAspect._buildings.Length; i++)
                {
                    var entityRef = chunkDataAspect._buildings[i];
                    var buildingData = _buildingsLookUp.GetRefRO(entityRef.Entity).ValueRO.BuildingData;
                    newChunkData.placedBuildingData[i] = buildingData;
                }

                newChunkData.chunkPosition = chunkDataAspect.ChunksPosition;
                chunkSaves.Add(newChunkData);
            }
            
            WorldSaveHandler.CurrentWorldSave.chunkSaves = chunkSaves.ToArray();
            WorldSaveHandler.SaveWorldToFile();
            chunks.Dispose();
        }
    }

    public class WorldBuilder
    {
        private EntityCommandBuffer _ecb;
        private readonly WorldSave _saveData;
        private readonly Entity _tilePrefab;

        public WorldBuilder(EntityCommandBuffer ecb, WorldSave saveData, Entity tilePrefab)
        {
            _ecb = ecb;
            _saveData = saveData;
            _tilePrefab = tilePrefab;
        }

        private static float WorldScale => GenerationSystem.WorldScale;
        
        public void Execute()
        {
            foreach (var chunkData in _saveData.chunkSaves)
            {
                LoadChunk(chunkData);
            }
        }
        
        private void LoadChunk(ChunkSave chunkData)
        {
            var entity = _ecb.CreateEntity();
            var chunkPosition = chunkData.chunkPosition;
            var worldPos = GenerationSystem.GetChunkWorldPosition(chunkPosition);
            
            _ecb.SetName(entity, $"Ch({chunkPosition.x},{chunkPosition.y})");
            _ecb.AddComponent(entity, new LocalTransform()
            {
                Position = worldPos,
                Scale = WorldScale,
            });
            var patches = new NativeArray<ResourcePatch>(chunkData.chunkResourcePatches.Length,Allocator.Temp);
            for (var i = 0; i < patches.Length; i++)
            {
                var patchData = chunkData.chunkResourcePatches[i];
                patches[i] = new ResourcePatch()
                {
                    Positions = new NativeArray<int2>(patchData.positions,Allocator.Temp),
                    ItemID = patchData.resourceID,
                };
            }

            _ecb.AddComponent(entity, new ChunkDataComponent(entity, chunkPosition, worldPos,
                _tilePrefab,patches, _ecb));
            patches.Dispose();
            _ecb.AddComponent(entity,new NewChunkDataComponent(chunkData.chunkPosition,chunkData.chunkResourcePatches.Length));
        }
    }
}
