using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Flags;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.General;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.Utilities;
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
        public static Entity worldDataEntity;
        public void OnCreate(ref SystemState state)
        {
            worldDataEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<PositionChunkPair>(worldDataEntity);
            
            state.EntityManager.SetName(worldDataEntity,"WorldData");
            GenerationSystem.worldDataEntity = worldDataEntity;
            
            _endSimEntityCommandBufferSystem = state.World.GetOrCreateSystemManaged<EndInitializationEntityCommandBufferSystem>();
            state.RequireForUpdate<PrefabsDataComponent>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var worldData =  WorldSaveHandler.GetWorldSave();

            var ecb = _endSimEntityCommandBufferSystem.CreateCommandBuffer();

            var builder = new WorldBuilder(ecb,SystemAPI.GetComponent<PrefabsDataComponent>(GenerationSystem.prefabsEntity),worldData);
            
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
        private ComponentLookup<BuildingDataComponent> buildingsLookUp;
        private WorldDataAspect _worldDataAspect;

        public WorldSaver(WorldDataAspect worldDataAspect, ComponentLookup<BuildingDataComponent> buildingsLookUp)
        {
            _worldDataAspect = worldDataAspect;
            this.buildingsLookUp = buildingsLookUp;
        }

        public void Execute()
        {
            List<ChunkSave> chunkSaves = new List<ChunkSave>();
            using var chunks = new NativeList<ChunkDataAspect>();
            
            _worldDataAspect.GetAllChunks(chunks);

            foreach (var chunkDataAspect in chunks)
            {
                var newChunkData = new ChunkSave();
                var patches = chunkDataAspect._chunkData.ValueRO.ResourcePatches.ToArray();
                newChunkData.chunkResourcePatches = new ChunkResourcePatch[patches.Length];
                for (int i = 0; i < patches.Length; i++)
                {
                    var patchData = new ChunkResourcePatch();
                    patchData.positions = patches[i].Positions.ToArray();
                    patchData.resourceID = patches[i].Resource.ResourceIDs[0];
                    newChunkData.chunkResourcePatches[i] = patchData;
                }

                newChunkData.placedBuildingData = new PlacedBuildingData[chunkDataAspect._buildings.Length];

                for (var i = 0; i < chunkDataAspect._buildings.Length; i++)
                {
                    var entityRef = chunkDataAspect._buildings[i];
                    var buildingData = buildingsLookUp.GetRefRO(entityRef.Entity).ValueRO.BuildingData;
                    newChunkData.placedBuildingData[i] = buildingData;
                }

                newChunkData.chunkPosition = chunkDataAspect.ChunksPosition;
                chunkSaves.Add(newChunkData);
            }
            
            WorldSaveHandler.CurrentWorldSave.chunkSaves = chunkSaves.ToArray();
            WorldSaveHandler.SaveWorldToFile();
        }
    }

    public class WorldBuilder
    {
        private EntityCommandBuffer ECB;
        private PrefabsDataComponent prefabsComp;
        private WorldSave saveData;

        public WorldBuilder(EntityCommandBuffer ecb, PrefabsDataComponent prefabsComp, WorldSave saveData)
        {
            ECB = ecb;
            this.prefabsComp = prefabsComp;
            this.saveData = saveData;
        }

        private static float WorldScale => GenerationSystem.WorldScale;
        
        public void Execute()
        {
            foreach (var chunkData in saveData.chunkSaves)
            {
                LoadChunk(chunkData);
            }
        }
        
        private void LoadChunk(ChunkSave chunkData)
        {
            Entity entity = ECB.CreateEntity();
            int2 chunkPosition = chunkData.chunkPosition;
            float3 worldPos = GenerationSystem.GetChunkWorldPosition(chunkPosition);
            ECB.SetName(entity, $"Ch({chunkPosition.x},{chunkPosition.y})");
            ECB.AddComponent(entity, new LocalTransform()
            {
                Position = worldPos,
                Scale = WorldScale,
            });
            var patches = new NativeArray<ResourcePatch>(chunkData.chunkResourcePatches.Length,Allocator.Temp);
            for (int i = 0; i < patches.Length; i++)
            {
                var patchData = chunkData.chunkResourcePatches[i];
                patches[i] = new ResourcePatch()
                {
                    Positions = new NativeArray<int2>(patchData.positions,Allocator.Temp),
                    Resource = ResourcesUtility.CreateItemData(new []{patchData.resourceID})
                };
            }

            ECB.AddComponent(entity, new ChunkDataComponent(entity, chunkPosition, worldPos,
                prefabsComp,patches, ECB));
            patches.Dispose();
            ECB.AddComponent(entity,new NewChunkDataComponent(chunkData.chunkPosition,chunkData.chunkResourcePatches.Length));
        }
    }
}
