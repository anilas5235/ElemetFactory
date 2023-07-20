using Project.Scripts.Grid.CellType;
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
            };

        public static TileBase DefaultTile { get; } = Resources.Load<TileBase>("Tiles/Gray");

        public static TileBase GetTileSource(CellResources.ResourcesType resourcesType)
        {
            int index = (int)resourcesType;
            return index < _resourceTiles.Length ? _resourceTiles[index] : DefaultTile;
        }
    }
}
