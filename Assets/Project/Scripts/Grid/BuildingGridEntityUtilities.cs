using System;
using System.Collections.Generic;
using System.Linq;
using Noise;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components.Buildings;
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

        private const int pixelsPerUnit = 200;
        
        private static EntityArchetype conveyor, extractor,combiner,trashCan,separator;

        private static List<ComponentType> standardtComps = new List<ComponentType>()
        {
            typeof(Translation), typeof(NonUniformScale), typeof(Rotation),
            typeof(RenderMesh), typeof(RenderBounds), typeof(LocalToWorld), typeof(InputDataComponent),
            typeof(OutputDataComponent)
        };
        private static bool archetypesInit;
                                                
        public static Mesh quad;
        private static Material conveyorMaterial = Resources.Load<Material>("Materials/ConveyorUp"),
            extractorMaterial = Resources.Load<Material>("Materials/Excavator");

        private static readonly int BaseTexture = Shader.PropertyToID("_BaseTexture");

        private static void InitArchetypes()
        {
            List<ComponentType> componentTypes = new List<ComponentType>();
            componentTypes.AddRange(standardtComps);
            componentTypes.Add(typeof(ConveyorTickDataComponent ));
            
            conveyor = EntityManager.CreateArchetype(componentTypes.ToArray());
            
            componentTypes = new List<ComponentType>();
            componentTypes.AddRange(standardtComps);
            componentTypes.Add(typeof( ExtractorTickDataComponent));

            extractor = EntityManager.CreateArchetype(componentTypes.ToArray());
            
            componentTypes = new List<ComponentType>();
            componentTypes.AddRange(standardtComps);
            componentTypes.Add(typeof( CombinerTickDataComponent));

            combiner = EntityManager.CreateArchetype(componentTypes.ToArray());
            
            componentTypes = new List<ComponentType>();
            componentTypes.AddRange(standardtComps);
            componentTypes.Add(typeof( TrashCanTickDataComponent));

            trashCan = EntityManager.CreateArchetype(componentTypes.ToArray());
            
            componentTypes = new List<ComponentType>();
            componentTypes.AddRange(standardtComps);
            componentTypes.Add(typeof( SeparatorTickDataComponent));

            separator = EntityManager.CreateArchetype(componentTypes.ToArray());
        }

        private static void InitMesh()
        {
            quad = new Mesh();

            int width = 1;
            int height = 1;

            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(0, 0, 0),
                new Vector3(width, 0, 0),
                new Vector3(0, height, 0),
                new Vector3(width, height, 0)
            };
            quad.vertices = vertices;

            int[] tris = new int[6]
            {
                // lower left triangle
                0, 2, 1,
                // upper right triangle
                2, 3, 1
            };
            quad.triangles = tris;

            Vector3[] normals = new Vector3[4]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };
            quad.normals = normals;

            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            quad.uv = uv;
        }

        public static Entity CreateBuildingEntity(Vector3 position, PlacedBuildingData data)
        {
            if (!archetypesInit)
            {
                InitArchetypes();
                InitMesh();
                archetypesInit = true;
            }

            Entity entity;
            switch ((PossibleBuildings)data.buildingDataID)
            {
                case PossibleBuildings.Extractor:
                    entity = CreateBaseEntity(0, 1, ((PossibleBuildings)data.buildingDataID).ToString());
                    EntityManager.SetSharedComponentData(entity,
                        new RenderMesh() { mesh = quad, material = extractorMaterial, layerMask = 1 });
                    break;
                case PossibleBuildings.Conveyor:
                    entity = CreateBaseEntity(1, 1, ((PossibleBuildings)data.buildingDataID).ToString());
                    EntityManager.SetSharedComponentData(entity,
                        new RenderMesh() { mesh = quad, material = conveyorMaterial, layerMask = 1 });
                    break;
                case PossibleBuildings.Combiner:
                    entity = CreateBaseEntity(2, 1, ((PossibleBuildings)data.buildingDataID).ToString());
                    break;
                case PossibleBuildings.TrashCan:
                    entity = CreateBaseEntity(4, 0, ((PossibleBuildings)data.buildingDataID).ToString());
                    break;
                case PossibleBuildings.Separator:
                    entity = CreateBaseEntity(1, 2, ((PossibleBuildings)data.buildingDataID).ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EntityManager.SetComponentData(entity,
                new Rotation()
                    { Value = quaternion.RotateZ(math.radians(PlacedBuildingUtility.GetRotation(data.directionID))) });

            Texture tex = EntityManager.GetSharedComponentData<RenderMesh>(entity).material.GetTexture(BaseTexture);
            float3 scale = new float3((float)tex.width / pixelsPerUnit, (float)tex.height / pixelsPerUnit, 1);
            EntityManager.SetComponentData(entity, new NonUniformScale() { Value = scale });

            EntityManager.SetComponentData(entity,
                new Translation() { Value = position - new Vector3(scale.x - 1, scale.y - 1, 0) });

            return entity;
        }

        private static Entity CreateBaseEntity(int inputs, int outputs, string name)
        {
            Entity entity = EntityManager.CreateEntity(conveyor);
            EntityManager.SetComponentData(entity, new RenderBounds() { Value = new AABB() { Extents = new float3(.5f, .5f, 0) } });
            
            DynamicBuffer<InputDataComponent> inputBuffer = EntityManager.AddBuffer<InputDataComponent>(entity);
            for (int i = 0; i < inputs; i++)
            {
                inputBuffer.Add(new InputDataComponent(float3.zero,SlotBehaviour.Input));
            }
           

            DynamicBuffer<OutputDataComponent> outputBuffer = EntityManager.AddBuffer<OutputDataComponent>(entity);
            for (int i = 0; i < outputs; i++)
            {
               outputBuffer.Add(new OutputDataComponent(float3.zero, SlotBehaviour.Output)); 
            }

            EntityManager.SetName(entity,name);
            return entity;
        }
    }
}
