using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Baker;
using Project.Scripts.EntitySystem.BlobAssets;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.DataObject;
using Project.Scripts.EntitySystem.Components.Flags;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.General;
using Project.Scripts.Grid.DataContainers;
using Project.Scripts.ScriptableData;
using Project.Scripts.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct WorldSetUpSystem : ISystem,ISystemStartStop
    {
        public static Entity PrefabDataEntity;
        public static BlobAssetReference<BlobGamePrefabData> BlobGameAssetReference;
        
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

        public void OnStartRunning(ref SystemState state) { }

        public void OnUpdate(ref SystemState state)
        {
            var tilePrefabData = SystemAPI.GetSingleton<TilePrefabsDataComponent>();
            PrefabDataEntity = tilePrefabData.Entity;
            var itemPrefabData = SystemAPI.GetComponent<ItemPrefabsDataComponent>(PrefabDataEntity);
            var buildingPrefabBuffer = SystemAPI.GetBuffer<EntityIDPair>(PrefabDataEntity);

            #region BlobAssetCreation

            //creating the general blob game asset---------------------------------------------------------------------- 
            using var blobBuilder = new BlobBuilder(Allocator.Temp);
            ref var blobGamePrefabData = ref blobBuilder.ConstructRoot<BlobGamePrefabData>();

            blobGamePrefabData.ItemPrefab = itemPrefabData.ItemPrefab;
            blobGamePrefabData.TilePrefab = tilePrefabData.TilePrefab;
            blobGamePrefabData.TiledBackgroundPrefab = tilePrefabData.TileBackGroundPrefab;

            //adding the building data to the blob asset----------------------------------------------------------------
            var buildingsPrefDataArray = Resources.LoadAll<GameObject>("Prefabs/Buildings");

            var buildingDataArray =
                blobBuilder.Allocate(ref blobGamePrefabData.Buildings, buildingsPrefDataArray.Length);

            for (var i = 0; i < buildingsPrefDataArray.Length; i++)
            {
                var data = buildingsPrefDataArray[i].GetComponent<BuildingMono>();

                buildingDataArray[i] = new BlobBuildingData()
                {
                    nameString = data.NameString,
                    BuildingID = data.BuildingID,
                    Prefab = GetPrefabFormID(buildingPrefabBuffer, data.BuildingID),
                };

                var needTiles = blobBuilder.Allocate(ref buildingDataArray[i].neededTiles, data.NeededTiles.Length);
                for (var j = 0; j < needTiles.Length; j++)
                {
                    needTiles[j] = data.NeededTiles[j];
                }

                var inPorts = blobBuilder.Allocate(ref buildingDataArray[i].inputOffsets, data.InputOffsets.Length);
                for (var j = 0; j < inPorts.Length; j++)
                {
                    inPorts[j] = data.InputOffsets[j];
                }

                var outPorts = blobBuilder.Allocate(ref buildingDataArray[i].outputOffsets, data.OutputOffsets.Length);
                for (var j = 0; j < inPorts.Length; j++)
                {
                    outPorts[j] = data.OutputOffsets[j];
                }
            }

            //adding the item atlas data to the blob asset--------------------------------------------------------------
            var itemDataArray = Resources.LoadAll<ItemScriptableData>("Data/Items");
            if (itemDataArray.Length > 0)
            {
                var itemIdSpitePairs = itemDataArray
                    .Select(itemData => new IDSpritePair(itemData.uniqueID, itemData.sprite)).ToArray();
                //using first sprite to determine the tile size
                var sprite0 = itemIdSpitePairs[0].sprite;

                TextureUtility.CreateAtlasWithIDs(itemIdSpitePairs,
                    new Vector2Int((int)sprite0.rect.width, (int)sprite0.rect.height), out var atlasData);

                var itemAtlasData = blobBuilder.Allocate(ref blobGamePrefabData.ItemAtlasLookUp, itemDataArray.Length);

                for (var i = 0; i < itemAtlasData.Length; i++)
                {
                    itemAtlasData[i] = atlasData[i];
                }
            }

            //adding the tile atlas data to the blob asset--------------------------------------------------------------
            
            var tileDataArray = Resources.LoadAll<TileScriptableData>("Data/Items");
            if (tileDataArray.Length > 0)
            {
                var tileIdSpitePairs = tileDataArray
                    .Select(itemData => new IDSpritePair(itemData.uniqueID, itemData.sprite)).ToArray();
                //using first sprite to determine the tile size
                var sprite0 = tileIdSpitePairs[0].sprite;

                TextureUtility.CreateAtlasWithIDs(tileIdSpitePairs,
                    new Vector2Int((int)sprite0.rect.width, (int)sprite0.rect.height), out var atlasData);

                var tileAtlasData = blobBuilder.Allocate(ref blobGamePrefabData.TileAtlasLookUp, tileDataArray.Length);

                for (var i = 0; i < tileAtlasData.Length; i++)
                {
                    tileAtlasData[i] = atlasData[i];
                }
            }

            BlobGameAssetReference = blobBuilder.CreateBlobAssetReference<BlobGamePrefabData>(Allocator.Persistent);

            #endregion

            //load world from save file
            var worldData = WorldSaveHandler.GetWorldSave();
            var ecb = _endSimEntityCommandBufferSystem.CreateCommandBuffer();
            var builder = new WorldBuilder(ecb, worldData, tilePrefabData.TilePrefab);
            builder.Execute();

            state.Enabled = false;
        }

        private Entity GetPrefabFormID(DynamicBuffer<EntityIDPair> data, int id)
        {
            foreach (var entityIDPair in data)
            {
                if (entityIDPair.ID == id) {return entityIDPair.Entity;}
            }
            Debug.LogError($"Prefab for building with id {id} was not found in the buffer of the prefabEntity");
            return default;
        }

        public void OnStopRunning(ref SystemState state)
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
                var patches = chunkDataAspect.chunkData.ValueRO.ResourcePatches.ToArray();
                newChunkData.chunkResourcePatches = new ChunkResourcePatch[patches.Length];
                for (var i = 0; i < patches.Length; i++)
                {
                    var patchData = new ChunkResourcePatch
                    {
                        positions = patches[i].Positions.ToArray(),
                        resourceID = patches[i].itemID,
                    };
                    newChunkData.chunkResourcePatches[i] = patchData;
                }

                newChunkData.placedBuildingData = new PlacedBuildingData[chunkDataAspect.buildings.Length];

                for (var i = 0; i < chunkDataAspect.buildings.Length; i++)
                {
                    var entityRef = chunkDataAspect.buildings[i];
                    var buildingData = _buildingsLookUp.GetRefRO(entityRef.Entity).ValueRO.BuildingData;
                    newChunkData.placedBuildingData[i] = buildingData;
                }

                newChunkData.chunkPosition = chunkDataAspect.ChunksPosition;
                chunkSaves.Add(newChunkData);
            }
            
            //WorldSaveHandler.CurrentWorldSave.chunkSaves = chunkSaves.ToArray();
            //WorldSaveHandler.SaveWorldToFile();
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
                    itemID = patchData.resourceID,
                };
            }

            _ecb.AddComponent(entity, new ChunkDataComponent(entity, chunkPosition, worldPos,
                _tilePrefab,patches, _ecb));
            patches.Dispose();
            _ecb.AddComponent(entity,new NewChunkDataComponent(chunkData.chunkPosition,chunkData.chunkResourcePatches.Length));
        }
    }
}
