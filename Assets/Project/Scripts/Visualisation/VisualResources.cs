using System;
using Project.Scripts.CellType;
using Project.Scripts.General;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Project.Scripts.Visualisation
{
    public class VisualResources : Singleton<VisualResources>
    {
        private TileBase[] tiles;

        protected override void Awake()
        {
            base.Awake();
            tiles = new[]
            {
                Resources.Load<TileBase>("Tiles/Gray"),
                Resources.Load<TileBase>("Tiles/Red"),
                Resources.Load<TileBase>("Tiles/Blue"),
            };
        }

        public TileBase GetTileSource(CellResources.ResourcesType resourcesType)
        {
            int index = (int)resourcesType;
            if(index < tiles.Length)return tiles[index];
            return tiles[0];
        }
    }
}
