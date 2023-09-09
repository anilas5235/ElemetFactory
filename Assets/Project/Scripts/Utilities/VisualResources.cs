using System;
using System.Collections.Generic;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Project.Scripts.Utilities
{
    public static class VisualResources
    {
        public static readonly GameObject ItemContainer = Resources.Load<GameObject>("Prefaps/ItemContainer");

        private static readonly ResourceData[] ResourceData = new[]
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

        private static Dictionary<Color, ItemVisual> ItemVisuals;

        private static void LoadItemVisuals()
        {
            ItemVisuals = new Dictionary<Color, ItemVisual>();

            int limit =(int) Mathf.Pow(11f, 3f);
            for (int i = 0; i < limit; i++)
            {
                
            }
        }

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

    [Serializable]
    public struct ItemVisual
    {
        public Material[] materials;

        public ItemVisual(Color color, Material[] materials)
        {
            this.materials = materials;
        }
    }
}
