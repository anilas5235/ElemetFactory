using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components.Buildings.Specific;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.SlotSystem;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace Project.Scripts.Grid
{
    public static class BuildingGridEntityUtilities
    {
        private static EntityManager EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        private static EntityArchetype conveyor;
        private static bool archetypesInit;
                                                
        public static Mesh quad;
        private static Material conveyorMaterial = Resources.Load<Material>("Materials/ConveyorUp");

        private static void InitArchetypes()
        {
            conveyor = EntityManager.CreateArchetype(typeof(Translation), typeof(Scale), typeof(Rotation), typeof(RenderMesh),
                typeof(RenderBounds), typeof(LocalToWorld), typeof(ConveyorDataComponent),typeof(InputDataComponent),typeof(OutputDataComponent));
        }

        
        
        public static Entity CreateBuildingEntity(Vector3 position,PlacedBuildingData data)
        {
            if (!archetypesInit)
            {
                InitArchetypes();
                archetypesInit = true;
            }

            Entity entity = default;
            
            switch ((PossibleBuildings)data.buildingDataID)
            {
                case PossibleBuildings.Extractor:
                    break;
                case PossibleBuildings.Conveyor: entity = CreateConveyor( data);
                    break;
                case PossibleBuildings.Combiner:
                    break;
                case PossibleBuildings.TrashCan:
                    break;
                case PossibleBuildings.Separator:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            EntityManager.SetComponentData(entity, new Rotation(){Value = quaternion.RotateZ(math.radians(PlacedBuildingUtility.GetRotation(data.directionID)))});
            EntityManager.SetComponentData(entity, new Translation(){Value = position});
            EntityManager.SetComponentData(entity, new Scale() { Value = 1f });

            return entity;
        }

        private static Entity CreateConveyor(PlacedBuildingData data)
        {
            Entity entity = EntityManager.CreateEntity(conveyor);
            EntityManager.SetSharedComponentData(entity,new RenderMesh(){ mesh = quad, material = conveyorMaterial, layerMask = 1 });
            EntityManager.SetComponentData(entity, new RenderBounds() { Value = new AABB() { Extents = new float3(.5f, .5f, 0) } });
            
            DynamicBuffer<InputDataComponent> buffer = EntityManager.AddBuffer<InputDataComponent>(entity);
            buffer.Add(new InputDataComponent(float3.zero,SlotBehaviour.InAndOutput));

            var buffer2 = EntityManager.AddBuffer<OutputDataComponent>(entity);
            buffer2.Add(new OutputDataComponent(float3.zero, SlotBehaviour.InAndOutput));
            EntityManager.SetName(entity,"Conveyor");
            return entity;
        }
    }
}
