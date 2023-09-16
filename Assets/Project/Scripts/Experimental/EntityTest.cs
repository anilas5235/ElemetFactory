using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.SlotSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;


namespace Project.Scripts.Experimental
{
    public class EntityTest : MonoBehaviour
    {
        public Mesh mesh;
        public Material material;

        private EntityManager entityManager;
        private EntityArchetype item;
        private void Start()
        {
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            EntityArchetype slot= entityManager.CreateArchetype(typeof(Translation),typeof(SlotDataComponent));
            item = entityManager.CreateArchetype(typeof(Translation),typeof(Scale),typeof(RenderMesh),typeof(RenderBounds),typeof(LocalToWorld),typeof(ItemColorChange));

            NativeArray<Entity> entities = new NativeArray<Entity>(10000, Allocator.Temp);
            entityManager.CreateEntity(item, entities);

            foreach (Entity entity in entities)
            {
                entityManager.SetSharedComponentData(entity, new RenderMesh(){mesh = mesh,material = material,layerMask = 1});
                entityManager.SetComponentData(entity,new Translation(){Value = new float3(Random.Range(-40f,40f),Random.Range(-30f,30f),0)});
                entityManager.SetComponentData(entity, new ItemColorChange(){Value = new float4(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f),1)});
                entityManager.SetComponentData(entity,new Scale(){Value = .5f});
                entityManager.SetComponentData(entity,new RenderBounds(){Value = new AABB(){Extents = new float3(.5f,.5f,0)}});
            }
        }
    }
}
