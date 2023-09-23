using System;
using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.EntitySystem.Components;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Components.MaterialModify;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using Project.Scripts.Utilities;
using Unity.Collections;
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

        private static readonly ComponentType[] BuildingDefaultComps = new ComponentType[]
        {
            typeof(LocalTransform), typeof(RenderMesh), typeof(RenderBounds), 
            typeof(LocalToWorld),
        };

        private static readonly ComponentType[] ItemDefaultComps = new ComponentType[]
        {
            typeof(LocalTransform), typeof(RenderMesh), typeof(ItemColor),
            typeof(RenderBounds), typeof(LocalToWorld), typeof(ItemDataComponent)
        };

        private static EntityArchetype itemEntityArchetype;

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
            itemEntityArchetype = _entityManager.CreateArchetype(ItemDefaultComps);
            
            string[] names = Enum.GetNames(typeof(PossibleBuildings));
            _constructionData = new EntityConstructionData[names.Length];
          
            
            //extractor construction data setup
            _constructionData[0] = new EntityConstructionData(Resources.Load<Material>("Materials/Excavator"),
                _entityManager.CreateArchetype(BuildingDefaultComps.Union(new ComponentType[]
                    { typeof(ExtractorDataComponent)}).ToArray()),
                0, 1, names[0], new float2(.5f, .4f));
            
            //conveyor construction data setup
            _constructionData[1] = new EntityConstructionData(Resources.Load<Material>("Materials/ConveyorUp"),
                _entityManager.CreateArchetype(BuildingDefaultComps.Union(new ComponentType[]
                    { typeof(ConveyorDataComponent)}).ToArray())
                ,1,1,names[1], new float2(.5f,.5f));
            
            //Combiner construction data setup
            _constructionData[2] = new EntityConstructionData(Resources.Load<Material>("Materials/Combiner"),
                _entityManager.CreateArchetype(BuildingDefaultComps.Union(new ComponentType[]
                    { typeof(CombinerDataComponent)}).ToArray()),
                2,1,names[2], new float2(.25f,.5f));
            
            //TrashCan construction data setup
            _constructionData[3] = new EntityConstructionData(Resources.Load<Material>("Materials/TrashCan"),
                _entityManager.CreateArchetype(BuildingDefaultComps.Union(new ComponentType[]
                    { typeof(TrashCanDataComponent)}).ToArray()),
                1,0,names[3], new float2(.5f,.5f));
            
            //Separator construction data setup
            _constructionData[4] = new EntityConstructionData(Resources.Load<Material>("Materials/Separator"),
                _entityManager.CreateArchetype(BuildingDefaultComps.Union(new ComponentType[]
                    { typeof(SeparatorDataComponent)}).ToArray()), 
                1,2,names[4], new float2(.25f,.5f));
            
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
            
            //set mesh/render data
            _entityManager.SetSharedComponentManaged(entity, new RenderMesh() 
                {mesh = constructionData.Mesh, material = constructionData.Material});
            
            //set RenderBounds
            _entityManager.SetComponentData(entity, new RenderBounds()
            {
                Value = new AABB(){Center = constructionData.Mesh.bounds.center, 
                    Extents = constructionData.Mesh.bounds.extents},
            });
            
            //set localTransform
            var rotation = quaternion.RotateZ(math.radians(PlacedBuildingUtility.GetRotation(data.directionID)));
            _entityManager.SetComponentData(entity, new LocalTransform()
            {
                Position = position,
                Scale = EntityScaleFactor,
                Rotation = rotation,
            });

            //set entity Name
            _entityManager.SetName(entity, constructionData.Name);

            return entity;
        }

        public static bool CreateItemEntity(Vector3 position, Item item, out Entity entity)
        {
            return CreateItemEntity(position, ItemMemory.GetItemID(item), out entity);
        }
        
        public static bool CreateItemEntity(Vector3 position, uint itemID, out Entity entity)
        {
            var ecb = new EntityCommandBuffer(Allocator.TempJob);
            
            entity = default;
            if (!ItemMemory.GetItem(itemID, out var item)) return false;
            
            //create entity

            switch (item.ItemForm)
            {
                case ItemForm.Gas:
                    break;
                case ItemForm.Fluid:
                    break;
                case ItemForm.Solid:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            
            
            entity = ecb.CreateEntity(itemEntityArchetype);
            
            //set localTransform of entity
            ecb.SetComponent(entity, new LocalTransform()
            {
                Position = position,
                Scale = EntityScaleFactor,
            });
            
            //set mesh/render data
            ecb.SetSharedComponentManaged(entity, new RenderMesh() 
                { mesh = itemMesh ,material = ItemMaterials[(int)item.ItemForm],});
            
            //set bounds
            ecb.SetComponent(entity, new RenderBounds(){Value = new AABB()
            {
                Center = itemMesh.bounds.center,
                Extents = itemMesh.bounds.extents,
            }});
            
            //set Item Color Data
            ecb.SetComponent(entity,new ItemColor(){Value = item.Color});
            
            //set Item Data
            ecb.SetComponent(entity, new ItemDataComponent()
            {
                ItemID = itemID, PreviousPos = position,
                DestinationPos = position, Arrived = true, Progress = 1,
            });
            
            return true;
        }

        private struct EntityConstructionData
        {
            public EntityConstructionData(Material material, EntityArchetype archetype, int numInputs, int numOutputs, string name, float2 center)
            {
                Material = material;
                Archetype = archetype;
                NumInputs = numInputs;
                NumOutputs = numOutputs;
                Name = name;
                Texture tex = Material.GetTexture(BaseTexture);
                Mesh = MeshUtils.CreateQuad(center, tex.width / PixelsPerUnit, tex.height / PixelsPerUnit);
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
