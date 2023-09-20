using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
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
            new ResourceData(ResourceType.None,Resources.Load<TileBase>("Tiles/None"),new Color(.7f,.7f,.7f),ItemForm.Solid),
            new ResourceData(ResourceType.H,Resources.Load<TileBase>("Tiles/H"),new Color(.1f,.8f,1f), ItemForm.Gas),
            new ResourceData(ResourceType.C,Resources.Load<TileBase>("Tiles/C"),new Color(.1f,.1f,.1f), ItemForm.Solid),
            new ResourceData(ResourceType.O,Resources.Load<TileBase>("Tiles/O"),new Color(0f,1f,0f), ItemForm.Gas),
            new ResourceData(ResourceType.N,Resources.Load<TileBase>("Tiles/N"),new Color(1f,0f,0f), ItemForm.Gas),
            new ResourceData(ResourceType.S,Resources.Load<TileBase>("Tiles/S"),new Color(1f,1f,0f), ItemForm.Solid),
            new ResourceData(ResourceType.Al,Resources.Load<TileBase>("Tiles/Al"),new Color(1f,.5f,0f), ItemForm.Solid),
            new ResourceData(ResourceType.Fe,Resources.Load<TileBase>("Tiles/Fe"),new Color(.5f,0f,1f), ItemForm.Solid),
            new ResourceData(ResourceType.Na,Resources.Load<TileBase>("Tiles/Na"),new Color(1f,.2f,1f), ItemForm.Gas),
        };

        public static Item CreateItemData(int[] resourceIDs)
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
            return GetResourceData((int)resourceType);
        }
        
        public static ResourceData GetResourceData(int resourceID)
        {
            return ResourceDataBank[resourceID];
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
        public TileBase tile; 
        public Color color;
        public ItemForm form;

        public ResourceData(ResourceType resourceType, TileBase tile, Color color, ItemForm form)
        {
            this.tile = tile;
            this.color = color;
            this.form = form;
            this.resourceType = resourceType;
        }
    }
}
