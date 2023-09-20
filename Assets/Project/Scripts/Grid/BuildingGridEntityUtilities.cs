using System;
using System.Collections.Generic;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using Project.Scripts.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


namespace Project.Scripts.Grid
{
    public static class BuildingGridEntityUtilities
    {
        private static EntityManager _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        private const int PixelsPerUnit = 200;

        private static readonly List<ComponentType> BuildingDefaultComps = new List<ComponentType>()
        {
            typeof(Translation), typeof(NonUniformScale), typeof(Rotation),
            typeof(RenderMesh), typeof(RenderBounds), typeof(LocalToWorld),
            typeof(InputDataComponent), typeof(OutputDataComponent)
        };

        private static readonly List<ComponentType> ItemDefaultComps = new List<ComponentType>()
        {
            typeof(Translation), typeof(Scale), typeof(RenderMesh), typeof(ItemColor),
            typeof(RenderBounds), typeof(LocalToWorld), typeof(ItemDataComponent)
        };

        private static readonly Material[] ItemMaterials = new[]
        {
            Resources.Load<Material>("Materials/Gas_Bottle"),
            Resources.Load<Material>("Materials/Liquid_Bottle"),
            Resources.Load<Material>("Materials/Solid_Bottle"),
        };
        
        private static bool _init;

        private static Mesh _quad;

        private static EntityConstructionData[] _constructionData;

        private static readonly int BaseTexture = Shader.PropertyToID("_BaseTexture");

        private static readonly float EntityScaleFactor = 10f;
        private static void InitConstructionData()
        {
            string[] names = Enum.GetNames(typeof(PossibleBuildings));
            _constructionData = new EntityConstructionData[names.Length];
            List<ComponentType> componentTypes = new List<ComponentType>();

            float3[] nullOffset = new[] { float3.zero, float3.zero, float3.zero, float3.zero };
            
            //extractor construction data setup
            componentTypes.AddRange(BuildingDefaultComps);
            componentTypes.Add(typeof(ExtractorTickDataComponent));
            _constructionData[0] = new EntityConstructionData(Resources.Load<Material>("Materials/Excavator"),
                _entityManager.CreateArchetype(componentTypes.ToArray()),  nullOffset,0,1,names[0]);
            
            //conveyor construction data setup
            componentTypes = new List<ComponentType>();
            componentTypes.AddRange(BuildingDefaultComps);
            componentTypes.Add(typeof(ConveyorTickDataComponent));
            _constructionData[1] = new EntityConstructionData(Resources.Load<Material>("Materials/ConveyorUp"),
                _entityManager.CreateArchetype(componentTypes.ToArray()), nullOffset,1,1,names[1]);
            
            //Combiner construction data setup
            componentTypes = new List<ComponentType>();
            componentTypes.AddRange(BuildingDefaultComps);
            componentTypes.Add(typeof(CombinerTickDataComponent));
            _constructionData[2] = new EntityConstructionData(Resources.Load<Material>("Materials/Combiner"),
                _entityManager.CreateArchetype(componentTypes.ToArray()), 
                new []{new float3(1, -.25f, 0),new float3(-.25f,1,0),new float3(1, .25f, 0),new float3(-.25f,-1,0)},
                2,1,names[2]);
            
            //TrashCan construction data setup
            componentTypes = new List<ComponentType>();
            componentTypes.AddRange(BuildingDefaultComps);
            componentTypes.Add(typeof(TrashCanTickDataComponent));
            _constructionData[3] = new EntityConstructionData(Resources.Load<Material>("Materials/TrashCan"),
                _entityManager.CreateArchetype(componentTypes.ToArray()),
                new []{ new float3(-.25f, -.25f, 0),new float3(-.25f, -.25f, 0),new float3(-.25f, -.25f, 0),new float3(-.25f, -.25f, 0)},
                1,0,names[3]);
            
            //Separator construction data setup
            componentTypes = new List<ComponentType>();
            componentTypes.AddRange(BuildingDefaultComps);
            componentTypes.Add(typeof(SeparatorTickDataComponent));
            _constructionData[4] = new EntityConstructionData(Resources.Load<Material>("Materials/Separator"),
                _entityManager.CreateArchetype(componentTypes.ToArray()),
                new []{ new float3(1, -.25f, 0),new float3(-.25f,1,0),new float3(1, .25f, 0),new float3(-.25f,-1,0)}
                ,1,2,names[4]);
            
            _quad = MeshUtils.CreateQuad();
        }

        public static Entity CreateBuildingEntity(Vector3 position, PlacedBuildingData data)
        {
            if (!_init)
            {
                InitConstructionData();
                _init = true;
            }
            //get constructionData
            EntityConstructionData constructionData = _constructionData[data.buildingDataID];
            
            //create entity
            Entity entity = _entityManager.CreateEntity(constructionData.Archetype);
            
            //set Position of entity
            _entityManager.SetComponentData(entity, new Translation() 
            { Value = (float3)position + constructionData.Offset[data.directionID]* EntityScaleFactor});
            
            //set mesh/render data
            _entityManager.SetSharedComponentData(entity, new RenderMesh() 
                { mesh = _quad, material = constructionData.Material, layer = 0,layerMask = 1});
            
            //setup inputs
            DynamicBuffer<InputDataComponent> inputBuffer = _entityManager.AddBuffer<InputDataComponent>(entity);
            for (int i = 0; i < constructionData.NumInputs; i++)
            {
                inputBuffer.Add(new InputDataComponent(float3.zero, SlotBehaviour.Input,(byte)i));
            }

            //setup outputs
            DynamicBuffer<OutputDataComponent> outputBuffer = _entityManager.AddBuffer<OutputDataComponent>(entity);
            for (int i = 0; i < constructionData.NumOutputs; i++)
            {
                outputBuffer.Add(new OutputDataComponent(float3.zero, SlotBehaviour.Output,(byte)i));
            }

            //set entity Name
            _entityManager.SetName(entity, constructionData.Name);
            
            //set rotation of entity
            _entityManager.SetComponentData(entity, new Rotation()
                { Value = quaternion.RotateZ(math.radians(PlacedBuildingUtility.GetRotation(data.directionID))) });
            
            //set entity scale
            Texture tex = _entityManager.GetSharedComponentData<RenderMesh>(entity).material.GetTexture(BaseTexture);
            float3 scale = new float3((float)tex.width / PixelsPerUnit, (float)tex.height / PixelsPerUnit, 1)*EntityScaleFactor;
            _entityManager.SetComponentData(entity, new NonUniformScale() { Value = scale });
            
            //set RenderBounds
            _entityManager.SetComponentData(entity, new RenderBounds()
            {
                Value = new AABB() { Extents = new float3(scale.x / 2f, scale.y / 2f, 0) }
            });

            return entity;
        }

        public static Entity CreateItemEntity(Vector3 position, Item item)
        {
            //create entity
            Entity entity = _entityManager.CreateEntity(ItemDefaultComps.ToArray());
            
            //set Position of entity
            _entityManager.SetComponentData(entity, new Translation() { Value = position});
            
            //set Scale of entity
            _entityManager.SetComponentData(entity, new Scale(){Value = EntityScaleFactor});
            
            //set mesh/render data
            _entityManager.SetSharedComponentData(entity, new RenderMesh() 
                { mesh = _quad, material = ItemMaterials[(int)item.ItemForm], layer = 0,layerMask = 1});
            
            //set Item Color Data
            _entityManager.SetComponentData(entity,new ItemColor(){Value = item.Color});
            
            //set Item Data
            _entityManager.SetComponentData(entity, new ItemDataComponent()
            {
                ItemID = ItemMemory.GetItemID(item), PreviousPos = position,
                DestinationPos = position, Arrived = true, Progress = 1,
            });
            
            return entity;
        }

        private struct EntityConstructionData
        {
            public EntityConstructionData(Material material, EntityArchetype archetype, float3[] offset, int numInputs, int numOutputs, string name)
            {
                Material = material;
                Archetype = archetype;
                Offset = offset;
                NumInputs = numInputs;
                NumOutputs = numOutputs;
                Name = name;
            }
            public Material Material { get; }
            public EntityArchetype Archetype { get; }
            public float3[] Offset { get; }
            public int NumInputs { get; }
            public int NumOutputs { get; }
            public string Name { get; }
        }
    }
}
