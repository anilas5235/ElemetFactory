using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Project.Scripts.Utilities
{
    public static class ResourcesUtility
    {
        //data arrays
        private static readonly BuildingScriptableData[] PossibleBuildingData =
        {
            Resources.Load<BuildingScriptableData>("Buildings/Data/Extractor"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/Conveyor"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/Combiner"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/TrashCan"),
            Resources.Load<BuildingScriptableData>("Buildings/Data/Separator"),
        };
        
        private static readonly ResourceData[] ResourceDataBank = new[]
        {
            new ResourceData(ResourceType.None,new Color(.7f,.7f,.7f),ItemForm.Solid),
            new ResourceData(ResourceType.H,new Color(.1f,.8f,1f), ItemForm.Gas),
            new ResourceData(ResourceType.C,new Color(.1f,.1f,.1f), ItemForm.Solid),
            new ResourceData(ResourceType.O,new Color(0f,1f,0f), ItemForm.Gas),
            new ResourceData(ResourceType.N,new Color(1f,0f,0f), ItemForm.Gas),
            new ResourceData(ResourceType.S,new Color(1f,1f,0f), ItemForm.Solid),
            new ResourceData(ResourceType.Al,new Color(1f,.5f,0f), ItemForm.Solid),
            new ResourceData(ResourceType.Fe,new Color(.5f,0f,1f), ItemForm.Solid),
            new ResourceData(ResourceType.Na,new Color(1f,.2f,1f), ItemForm.Gas),
        };

        private static readonly TileBase[] Tiles = new[]
        {
            Resources.Load<TileBase>("Tiles/None"),
            Resources.Load<TileBase>("Tiles/H"),
            Resources.Load<TileBase>("Tiles/C"),
            Resources.Load<TileBase>("Tiles/O"),
            Resources.Load<TileBase>("Tiles/N"),
            Resources.Load<TileBase>("Tiles/S"),
            Resources.Load<TileBase>("Tiles/Al"),
            Resources.Load<TileBase>("Tiles/Fe"),
            Resources.Load<TileBase>("Tiles/Na"),
        };

        public static Item CreateItemData(NativeArray<uint> resourceIDs)
        {
            float4 color = float4.zero;
            float form =0;
            foreach (var id in resourceIDs)
            {
                ResourceData data = GetResourceData(id);
                color += new float4(data.color.r, data.color.g, data.color.b,0);
                form += (int)data.form;
            }
            
            color *= 1f / resourceIDs.Length;
            color.w = 1f;
            form *= 1f / resourceIDs.Length;
            form = Mathf.RoundToInt(form);
            
            return new Item(resourceIDs,(ItemForm)form,color);
        }
        
        public static ResourceData GetResourceData(ResourceType resourceType)
        {
            return GetResourceData((uint)resourceType);
        }
        
        public static ResourceData GetResourceData(uint resourceID)
        {
            return ResourceDataBank[resourceID];
        }

        public static TileBase GetResourceTile(ResourceType resourceType)
        {
            return GetResourceTile((uint)resourceType);
        }
        
        public static TileBase GetResourceTile(uint resourceID)
        {
            return Tiles[resourceID];
        }
        
        #region BuildingHandeling
        public static BuildingScriptableData GetBuildingDataBase(PossibleBuildings buildingType)
        {
            return GetBuildingDataBase((int)buildingType);
        }
        
        public static BuildingScriptableData GetBuildingDataBase(int buildingTypeID)
        {
            return PossibleBuildingData[buildingTypeID];
        }
        #endregion
    }

    [Serializable]
    public struct ResourceData
    {
        public string Name => resourceType.ToString();
        public ResourceType resourceType;
        public Color color;
        public ItemForm form;

        public ResourceData(ResourceType resourceType, Color color, ItemForm form)
        {
            this.color = color;
            this.form = form;
            this.resourceType = resourceType;
        }
    }
}
