using Project.Scripts.Grid;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Project.Scripts.Visualisation
{
    public static class VisualResources
    {
        public static TileBase DefaultTile { get; } = Resources.Load<TileBase>("Tiles/Gray");
        
        private static readonly TileBase[] ResourceTiles =
        {
            DefaultTile,
            Resources.Load<TileBase>("Tiles/Blue"),
            Resources.Load<TileBase>("Tiles/Black"),
            Resources.Load<TileBase>("Tiles/Red"),
            Resources.Load<TileBase>("Tiles/Green"),
            Resources.Load<TileBase>("Tiles/Lila"),
            Resources.Load<TileBase>("Tiles/Orange"),
            Resources.Load<TileBase>("Tiles/Yellow"),
            Resources.Load<TileBase>("Tiles/Violet"),
        };

        public static readonly GameObject ItemContainer = Resources.Load<GameObject>("Prefaps/ItemContainer");
        
        public static TileBase GetTileSource(BuildingGridResources.ResourcesType resourcesType)
        {
            return  ResourceTiles[(int) resourcesType];
        }
    }
}
