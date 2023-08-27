using System;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Project.Scripts.Visualisation
{
    public static class VisualResources
    {
        public static readonly GameObject ItemContainer = Resources.Load<GameObject>("Prefaps/ItemContainer");

        private static readonly ResourceData[] ResourceData = new[]
        {
            new ResourceData(ResourcesType.None,Resources.Load<TileBase>("Tiles/None"),new Color(171/255f,171/255f,171/255f),ItemForm.Solid),
            new ResourceData(ResourcesType.H,Resources.Load<TileBase>("Tiles/H"),new Color(0/255f,0/255f,255/255f), ItemForm.Gas),
            new ResourceData(ResourcesType.C,Resources.Load<TileBase>("Tiles/C"),new Color(25/255f,25/255f,25/255f), ItemForm.Solid),
            new ResourceData(ResourcesType.O,Resources.Load<TileBase>("Tiles/O"),new Color(0/255f,255/255f,0/255f), ItemForm.Gas),
            new ResourceData(ResourcesType.N,Resources.Load<TileBase>("Tiles/N"),new Color(255/255f,0/255f,0/255f), ItemForm.Gas),
            new ResourceData(ResourcesType.S,Resources.Load<TileBase>("Tiles/S"),new Color(255/255f,255/255f,0/255f), ItemForm.Solid),
            new ResourceData(ResourcesType.Al,Resources.Load<TileBase>("Tiles/Al"),new Color(255/255f,125/255f,0/255f), ItemForm.Solid),
            new ResourceData(ResourcesType.Fe,Resources.Load<TileBase>("Tiles/Fe"),new Color(125/255f,0/255f,255/255f), ItemForm.Solid),
            new ResourceData(ResourcesType.Na,Resources.Load<TileBase>("Tiles/Na"),new Color(255/255f,40/255f,255/255f), ItemForm.Gas),
        };
        
        public static TileBase GetTileSource(ResourcesType resourcesType)
        {
            return GetTileSource((int)resourcesType);
        }
        public static TileBase GetTileSource(int resourceID)
        {
            return ResourceData[resourceID].tile;
        }

        public static Color GetResourceColor(ResourcesType resourcesType)
        {
            return GetResourceColor((int)resourcesType);
        }
        public static Color GetResourceColor(int resourceID)
        {
            return ResourceData[resourceID].color;
        }

        public static ItemForm GetResourceForm(ResourcesType resourcesType)
        {
            return GetResourceForm((int)resourcesType);
        }
        
        public static ItemForm GetResourceForm(int resourceID)
        {
            return ResourceData[resourceID].form;
        }

        public static ResourceData GetResourceData(ResourcesType resourcesType)
        {
            return GetResourceData((int)resourcesType);
        }
        
        public static ResourceData GetResourceData(int resourceID)
        {
            return ResourceData[resourceID];
        }
    }

    [Serializable]
    public struct ResourceData
    {
        public string Name => resourcesType.ToString();
        public ResourcesType resourcesType;
        public TileBase tile; 
        public Color color;
        public ItemForm form;

        public ResourceData(ResourcesType resourcesType, TileBase tile, Color color, ItemForm form)
        {
            this.tile = tile;
            this.color = color;
            this.form = form;
            this.resourcesType = resourcesType;
        }
    }
}
