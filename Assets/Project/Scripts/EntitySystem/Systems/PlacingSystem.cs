using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.EntitySystem.Components.Tags;
using Project.Scripts.Utilities;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct PlacingSystem : ISystem
    {
        public static PlacingSystem Instance;
        private static EntityManager TheEntityManager => GenerationSystem._entityManager;
        
        public static BeginSimulationEntityCommandBufferSystem.Singleton beginSimulationEntityCommandBuffer; 
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); 
            Instance = this;
           
            state.RequireForUpdate<PrefabsDataComponent>();
            state.RequireForUpdate<WorldDataComponent>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            ResourcesUtility.SetUpBuildingData(
                GenerationSystem._entityManager.GetComponentData<PrefabsDataComponent>(GenerationSystem.prefabsEntity));
            
            beginSimulationEntityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        public bool TryToDeleteBuilding(float3 mousePos)
        {
            int2 cellPos = ChunkDataAspect.GetCellPositionFormWorldPosition(mousePos, out int2 chunkPosition);
            
            if (GenerationSystem.TryGetChunk(chunkPosition, out ChunkDataAspect chunkDataAspect))
            {
               return chunkDataAspect.TryToDeleteBuilding(cellPos);
            }
            return false;
        }

        public bool TryToPlaceBuilding(float3 mousePos, int buildingID, FacingDirection facingDirection)
        {
            int2 cellPos = ChunkDataAspect.GetCellPositionFormWorldPosition(mousePos, out int2 chunkPosition);
            
            if (GenerationSystem.TryGetChunk(chunkPosition, out ChunkDataAspect chunkDataAspect))
            {
               return chunkDataAspect.TryToPlaceBuilding(buildingID,cellPos,facingDirection);
            }
            return false;
        }

        public static Entity CreateBuildingEntity(CellObject cell, PlacedBuildingData buildingData)
        {
            if (!ResourcesUtility.GetBuildingData(buildingData.buildingDataID, out BuildingLookUpData data))
                return default;

            Entity entity = TheEntityManager.Instantiate(data.Prefab);

            quaternion rotation =
                quaternion.RotateZ(math.radians(PlacedBuildingUtility.GetRotation(buildingData.directionID)));

            TheEntityManager.SetComponentData(entity, new LocalTransform()
            {
                Position = cell.WorldPosition,
                Scale = GenerationSystem.WorldScale,
                Rotation = rotation,
            });

            switch (buildingData.buildingDataID)
            {
                case 0:
                    TheEntityManager.SetComponentData(entity, new ItemDataComponent()
                    {
                        itemForm = cell.Resource.ItemForm,
                        itemColor = cell.Resource.Color,
                    });
                    var buffer = TheEntityManager.GetBuffer<ResourceDataPoint>(entity);

                    foreach (uint resourceID in cell.Resource.ResourceIDs)
                    {
                        buffer.Add(new ResourceDataPoint()
                        {
                            id = resourceID
                        });
                    }

                    break;
            }

            if (buildingData.buildingDataID != 1)TheEntityManager.AddComponent<BuildingWithPushPullTag>(entity);

            TheEntityManager.SetComponentData(entity, new BuildingDataComponent(buildingData));

            TheEntityManager.SetName(entity, data.Name);

            return entity;
        }
    }
}
