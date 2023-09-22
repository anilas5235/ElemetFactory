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
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;


namespace Project.Scripts.Grid
{
    public static class BuildingGridEntityUtilities
    {
        private static EntityManager _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        private const int PixelsPerUnit = 200;

        private static readonly List<ComponentType> BuildingDefaultComps = new List<ComponentType>()
        {
            typeof(LocalTransform), typeof(RenderMesh), typeof(RenderBounds), 
            typeof(LocalToWorld), typeof(InputDataComponent), typeof(OutputDataComponent)
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
        
        private static EntityConstructionData[] _constructionData;

        private static Mesh itemMesh;

        private static readonly int BaseTexture = Shader.PropertyToID("_BaseTexture");

        private static readonly float EntityScaleFactor = 10f;
        private static void InitConstructionData()
        {
            string[] names = Enum.GetNames(typeof(PossibleBuildings));
            _constructionData = new EntityConstructionData[names.Length];
            List<ComponentType> componentTypes = new List<ComponentType>();
            
            //extractor construction data setup
            componentTypes.AddRange(BuildingDefaultComps);
            componentTypes.Add(typeof(ExtractorTickDataComponent));
            _constructionData[0] = new EntityConstructionData(Resources.Load<Material>("Materials/Excavator"),
                _entityManager.CreateArchetype(componentTypes.ToArray()), 0,1,names[0],
                MeshUtils.CreateQuad(new float2(.5f,.4f)));
            
            //conveyor construction data setup
            componentTypes = new List<ComponentType>();
            componentTypes.AddRange(BuildingDefaultComps);
            componentTypes.Add(typeof(ConveyorTickDataComponent));
            _constructionData[1] = new EntityConstructionData(Resources.Load<Material>("Materials/ConveyorUp"),
                _entityManager.CreateArchetype(componentTypes.ToArray()),1,1,names[1],
                MeshUtils.CreateQuad(new float2(.5f,.5f)));
            
            //Combiner construction data setup
            componentTypes = new List<ComponentType>();
            componentTypes.AddRange(BuildingDefaultComps);
            componentTypes.Add(typeof(CombinerTickDataComponent));
            _constructionData[2] = new EntityConstructionData(Resources.Load<Material>("Materials/Combiner"),
                _entityManager.CreateArchetype(componentTypes.ToArray()), 2,1,names[2],
                MeshUtils.CreateQuad(new float2(.25f,.5f)));
            
            //TrashCan construction data setup
            componentTypes = new List<ComponentType>();
            componentTypes.AddRange(BuildingDefaultComps);
            componentTypes.Add(typeof(TrashCanTickDataComponent));
            _constructionData[3] = new EntityConstructionData(Resources.Load<Material>("Materials/TrashCan"),
                _entityManager.CreateArchetype(componentTypes.ToArray()), 1,0,names[3],
                MeshUtils.CreateQuad(new float2(.5f,.5f)));
            
            //Separator construction data setup
            componentTypes = new List<ComponentType>();
            componentTypes.AddRange(BuildingDefaultComps);
            componentTypes.Add(typeof(SeparatorTickDataComponent));
            _constructionData[4] = new EntityConstructionData(Resources.Load<Material>("Materials/Separator"),
                _entityManager.CreateArchetype(componentTypes.ToArray()), 1,2,names[4],
                MeshUtils.CreateQuad(new float2(.25f,.5f)));
            
            itemMesh = MeshUtils.CreateQuad(new float2(.5f, .5f));
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
            
            //set rotation of entity
            var rotation = quaternion.RotateZ(math.radians(PlacedBuildingUtility.GetRotation(data.directionID)));
            
            //set entity scale
            Texture tex = _entityManager.GetSharedComponentManaged<RenderMesh>(entity).material.GetTexture(BaseTexture);
            float3 scale = new float3((float)tex.width / PixelsPerUnit, (float)tex.height / PixelsPerUnit, 1)*EntityScaleFactor;
            
            //set Position of entity
            _entityManager.SetComponentData(entity, new LocalTransform()
            {
                Position = position,
                Scale = scale,
                
            });
            
            //set mesh/render data
            _entityManager.SetSharedComponentManaged(entity, new RenderMesh() 
                { mesh = constructionData.Mesh, material = constructionData.Material, layer = 0,layerMask = 1});
            
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
            
            //set RenderBounds
            _entityManager.SetComponentData(entity, new RenderBounds()
            {
                Value = new AABB(){Center = constructionData.Mesh.bounds.center,
                Extents = constructionData.Mesh.bounds.extents},
            });

            return entity;
        }

        public static bool CreateItemEntity(Vector3 position, Item item, EntityManager entityManager, out Entity entity)
        {
            return CreateItemEntity(position, ItemMemory.GetItemID(item), entityManager, out entity);
        }

        [GenerateTestsForBurstCompatibility]
        public static bool CreateItemEntity(Vector3 position, uint itemID, EntityManager entityManager, out Entity entity)
        {
            entity = default;
            if (!ItemMemory.GetItem(itemID, out var item)) return false;
            
            //create entity
            entity = entityManager.CreateEntity(ItemDefaultComps.ToArray());
            
            //set Position of entity
            entityManager.SetComponentData(entity, new Translation() { Value = position});
            
            //set Scale of entity
            entityManager.SetComponentData(entity, new Scale(){Value = EntityScaleFactor});
            
            //set mesh/render data
            entityManager.SetSharedComponentManaged(entity, new RenderMesh() 
                { mesh = itemMesh ,material = ItemMaterials[(int)item.ItemForm], layer = 0,layerMask = 1});
            
            //set bounds
            entityManager.SetComponentData(entity, new RenderBounds(){Value = new AABB()
            {
                Center = itemMesh.bounds.center,
                Extents = itemMesh.bounds.extents,
            }});
            
            //set Item Color Data
            entityManager.SetComponentData(entity,new ItemColor(){Value = item.Color});
            
            //set Item Data
            entityManager.SetComponentData(entity, new ItemDataComponent()
            {
                ItemID = itemID, PreviousPos = position,
                DestinationPos = position, Arrived = true, Progress = 1,
            });
            
            return true;
        }

        private struct EntityConstructionData
        {
            public EntityConstructionData(Material material, EntityArchetype archetype, int numInputs, int numOutputs, string name, Mesh mesh)
            {
                Material = material;
                Archetype = archetype;
                NumInputs = numInputs;
                NumOutputs = numOutputs;
                Name = name;
                Mesh = mesh;
            }
            public Material Material { get; }
            public EntityArchetype Archetype { get; }
            public Mesh Mesh { get; }
            public int NumInputs { get; }
            public int NumOutputs { get; }
            public string Name { get; }
        }
    }
}
