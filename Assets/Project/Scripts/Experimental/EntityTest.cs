using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Grid;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Experimental
{
    public class EntityTest : MonoBehaviour
    {
        private void Start()
        {
            //item = entityManager.CreateArchetype(typeof(Translation), typeof(Scale), typeof(RenderMesh),typeof(RenderBounds), typeof(LocalToWorld), typeof(ItemColor), typeof(ItemDataComponent));

            for (int i = 0; i < 5; i++)
            {
                BuildingGridEntityUtilities.CreateBuildingEntity(new Vector3(0,i,0), new PlacedBuildingData() { buildingDataID = i, directionID = 0});
            }

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
