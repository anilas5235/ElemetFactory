using Project.Scripts.Grid;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Project.Scripts.Visualisation
{
    public static class VisualResources
    {
        private static TileBase[] _resourceTiles =
            new[]
            {
                Resources.Load<TileBase>("Tiles/Red"),
                Resources.Load<TileBase>("Tiles/Blue"),
                Resources.Load<TileBase>("Tiles/Green"),
                Resources.Load<TileBase>("Tiles/Lila"),
                Resources.Load<TileBase>("Tiles/Orange"),
                Resources.Load<TileBase>("Tiles/Yellow"),
                Resources.Load<TileBase>("Tiles/Violet"),
            };

        public static TileBase DefaultTile { get; } = Resources.Load<TileBase>("Tiles/Gray");

        public static TileBase GetTileSource(BuildingGridResources.ResourcesType resourcesType)
        {
            int index = (int)resourcesType;
            return index < _resourceTiles.Length ? _resourceTiles[index] : DefaultTile;
        }
    }
}
