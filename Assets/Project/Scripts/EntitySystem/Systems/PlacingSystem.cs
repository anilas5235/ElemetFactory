using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.DataObject;
using Project.Scripts.EntitySystem.Components.Grid;
using Project.Scripts.EntitySystem.Components.Item;
using Project.Scripts.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


namespace Project.Scripts.EntitySystem.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct PlacingSystem : ISystem
    {
        public static PlacingSystem Instance;
        private static EntityManager TheEntityManager => GenerationSystem.entityManager;
        
        public static BeginSimulationEntityCommandBufferSystem.Singleton beginSimulationEntityCommandBuffer; 
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>(); 
            Instance = this;
           
            state.RequireForUpdate<ItemPrefabsDataComponent>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            ResourcesUtility.SetUpBuildingData(
                GenerationSystem.entityManager.GetBuffer<EntityIDPair>(GenerationSystem.prefabsEntity).AsNativeArray());
            
            beginSimulationEntityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        public bool TryToDeleteBuilding(float3 mousePos)
        {
            var cellPos = ChunkDataAspect.GetCellPositionFormWorldPosition(mousePos, out var chunkPosition);
            return GenerationSystem.Instance.TryGetChunk(chunkPosition, out var chunkDataAspect) && chunkDataAspect.TryToDeleteBuilding(cellPos);
        }

        public bool TryToPlaceBuilding(float3 mousePos, int buildingID, FacingDirection facingDirection)
        {
            var cellPos = ChunkDataAspect.GetCellPositionFormWorldPosition(mousePos, out var chunkPosition);
            
            return GenerationSystem.Instance.TryGetChunk(chunkPosition, out var chunkDataAspect) && chunkDataAspect.TryToPlaceBuilding(buildingID,cellPos,facingDirection);
        }

        public static Entity CreateBuildingEntity(CellObject cell, PlacedBuildingData buildingData)
        {
            if (!ResourcesUtility.GetBuildingData(buildingData.buildingDataID, out var data))
                return default;

            var entity = TheEntityManager.Instantiate(data.Prefab);

            var rotation = quaternion.RotateZ(math.radians(PlacedBuildingUtility.GetRotation(buildingData.directionID)));

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
                        ItemID = cell.ResourceID,
                    });
                    break;
            }

            TheEntityManager.SetComponentData(entity, new BuildingDataComponent(buildingData));

            TheEntityManager.SetName(entity, data.Name);

            return entity;
        }
    }
}
