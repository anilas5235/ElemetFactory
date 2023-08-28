using System;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Project.Scripts.Visualisation
{
    public static class VisualResources
    {
        public static readonly GameObject ItemContainer = Resources.Load<GameObject>("Prefaps/ItemContainer");

        private static readonly ResourceData[] ResourceData = new[]
        {
            new ResourceData(ResourceType.None,Resources.Load<TileBase>("Tiles/None"),new Color(171/255f,171/255f,171/255f),ItemForm.Solid),
            new ResourceData(ResourceType.H,Resources.Load<TileBase>("Tiles/H"),new Color(20/255f,200/255f,255/255f), ItemForm.Gas),
            new ResourceData(ResourceType.C,Resources.Load<TileBase>("Tiles/C"),new Color(25/255f,25/255f,25/255f), ItemForm.Solid),
            new ResourceData(ResourceType.O,Resources.Load<TileBase>("Tiles/O"),new Color(0/255f,255/255f,0/255f), ItemForm.Gas),
            new ResourceData(ResourceType.N,Resources.Load<TileBase>("Tiles/N"),new Color(255/255f,0/255f,0/255f), ItemForm.Gas),
            new ResourceData(ResourceType.S,Resources.Load<TileBase>("Tiles/S"),new Color(255/255f,255/255f,0/255f), ItemForm.Solid),
            new ResourceData(ResourceType.Al,Resources.Load<TileBase>("Tiles/Al"),new Color(255/255f,125/255f,0/255f), ItemForm.Solid),
            new ResourceData(ResourceType.Fe,Resources.Load<TileBase>("Tiles/Fe"),new Color(125/255f,0/255f,255/255f), ItemForm.Solid),
            new ResourceData(ResourceType.Na,Resources.Load<TileBase>("Tiles/Na"),new Color(255/255f,40/255f,255/255f), ItemForm.Gas),
        };
        
        public static TileBase GetTileSource(ResourceType resourceType)
        {
            return GetTileSource((int)resourceType);
        }
        public static TileBase GetTileSource(int resourceID)
        {
            return ResourceData[resourceID].tile;
        }

        public static Color GetResourceColor(ResourceType resourceType)
        {
            return GetResourceColor((int)resourceType);
        }
        public static Color GetResourceColor(int resourceID)
        {
            return ResourceData[resourceID].color;
        }

        public static ItemForm GetResourceForm(ResourceType resourceType)
        {
            return GetResourceForm((int)resourceType);
        }
        
        public static ItemForm GetResourceForm(int resourceID)
        {
            return ResourceData[resourceID].form;
        }

        public static ResourceData GetResourceData(ResourceType resourceType)
        {
            return GetResourceData((int)resourceType);
        }
        
        public static ResourceData GetResourceData(int resourceID)
        {
            return ResourceData[resourceID];
        }
    }

    [Serializable]
    public struct ResourceData
    {
        public string Name => resourceType.ToString();
        [FormerlySerializedAs("resourcesType")] public ResourceType resourceType;
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
