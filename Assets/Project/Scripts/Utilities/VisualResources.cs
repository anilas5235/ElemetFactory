using System;
using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using UnityEngine;
using static Project.Scripts.Utilities.GeneralUtilities;
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

        private static Dictionary<Color, ItemVisual> _itemVisuals;
        private const string BasePath = "Materials/ItemMaterials";

        private static void LoadItemVisuals()
        {
            _itemVisuals = new Dictionary<Color, ItemVisual>();

            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 11; j++)
                {
                    for (int k = 0; k < 11; k++)
                    {
                        string fileName = TurnToHex(i) + TurnToHex(j) + TurnToHex(k);
                        string path = BasePath +"/"+ fileName+"/";
                        Material[] materials = Resources.LoadAll<Material>(path);
                        ItemVisual itemVisual = new ItemVisual(materials);
                        _itemVisuals.Add(new Color(i / 10f, j / 10f, k / 10f, 1f),itemVisual);
                    }
                }
            }
        }

        public static Material GetItemMaterial(Color color, ItemForm form)
        {
            if(_itemVisuals == null) LoadItemVisuals();

            color.r = Mathf.Round(color.r * 10f)/10f;
            color.g = Mathf.Round(color.g * 10f)/10f;
            color.b = Mathf.Round(color.b * 10f)/10f;
            color.a = 1f;
            
            Material material =_itemVisuals[color].materials[(int)form];

            material ??= _itemVisuals.First().Value.materials[(int)form];

            return material;
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

        public ItemVisual(Material[] materials)
        {
            this.materials = materials;
        }
    }
}
