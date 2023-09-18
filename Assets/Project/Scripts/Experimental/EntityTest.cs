using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings.Specific;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Grid;
using Project.Scripts.SlotSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


namespace Project.Scripts.Experimental
{
    public class EntityTest : MonoBehaviour
    {
        public Mesh mesh;
        public Material itemMaterial,conveyorMaterial;

        private EntityManager entityManager;
        private EntityArchetype item,conveyor;

        private void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            //item = entityManager.CreateArchetype(typeof(Translation), typeof(Scale), typeof(RenderMesh),typeof(RenderBounds), typeof(LocalToWorld), typeof(ItemColor), typeof(ItemDataComponent));

            BuildingGridEntityUtilities.quad = mesh;

            BuildingGridEntityUtilities.CreateBuildingEntity(Vector3.zero, new PlacedBuildingData() { buildingDataID = 1, directionID = 1});

            /*
            NativeArray<Entity> entities = new NativeArray<Entity>(10000, Allocator.Temp);
            entityManager.CreateEntity(item, entities);

            foreach (Entity entity in entities)
            {
                entityManager.SetSharedComponentData(entity,
                    new RenderMesh() { mesh = mesh, material = itemMaterial, layerMask = 1 });
                entityManager.SetComponentData(entity,
                    new Translation() { Value = new float3(Random.Range(-40f, 40f), Random.Range(-30f, 30f), 0) });
                entityManager.SetComponentData(entity,
                    new ItemColor()
                        { Value = new float4(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1) });
                entityManager.SetComponentData(entity, new Scale() { Value = .5f });
                entityManager.SetComponentData(entity,
                    new RenderBounds() { Value = new AABB() { Extents = new float3(.5f, .5f, 0) } });
            }
            */
        }
    }
}
