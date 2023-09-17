using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings.Specific;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.EntitySystem.Components.Transmission;
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

            EntityArchetype slot = entityManager.CreateArchetype(typeof(Translation), typeof(SlotDataComponent));
            item = entityManager.CreateArchetype(typeof(Translation), typeof(Scale), typeof(RenderMesh),
                typeof(RenderBounds), typeof(LocalToWorld), typeof(ItemColor), typeof(ItemDataComponent));
            conveyor = entityManager.CreateArchetype(typeof(Translation), typeof(Scale), typeof(RenderMesh),
                typeof(RenderBounds), typeof(LocalToWorld), typeof(ConveyorDataComponent),typeof(InputDataComponent),typeof(OutputDataComponent));

            Entity inputSlot = entityManager.CreateEntity(slot);
            entityManager.SetName(inputSlot,"Input");
            Entity outputSlot = entityManager.CreateEntity(slot);
            entityManager.SetName(outputSlot,"Output");
            
            Entity conveyorEntity = entityManager.CreateEntity(conveyor);
            entityManager.SetSharedComponentData(conveyorEntity,new RenderMesh(){ mesh = mesh, material = conveyorMaterial, layerMask = 1 });
            entityManager.SetComponentData(conveyorEntity,
                new RenderBounds() { Value = new AABB() { Extents = new float3(.5f, .5f, 0) } });
            entityManager.SetComponentData(conveyorEntity, new InputDataComponent(inputSlot));
            entityManager.SetComponentData(conveyorEntity, new OutputDataComponent(outputSlot));
            entityManager.SetComponentData(conveyorEntity, new Scale() { Value = 1f });
            entityManager.SetName(conveyorEntity,"Conveyor");
            
            entityManager.SetComponentData(inputSlot, new SlotDataComponent(SlotBehaviour.InAndOutput,default,conveyorEntity));
            entityManager.SetComponentData(outputSlot, new SlotDataComponent(SlotBehaviour.InAndOutput,default,conveyorEntity));

            return;
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
        }
    }
}
