using System;
using System.Collections.Generic;
using Project.Scripts.Buildings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Project.Scripts.Grid
{
    public class GridChunk : MonoBehaviour
    {
        public GridField<GridObject> BuildingGrid { get; private set; }
        public List<PlacedBuilding> Buildings { get; private set; }
        public Tilemap ChunkTilemap { get; private set; }
        private TilemapRenderer chunkTilemapRenderer;
        public Vector3 LocalPosition { get; private set; }
        public Vector2Int ChunkPosition { get; private set; }

        public bool Loaded { get; private set; } = true;

        public void Initialization(Vector2Int chunkPosition, Vector3 localPosition)
        {
            ChunkPosition = chunkPosition;
            LocalPosition = localPosition;
            transform.localPosition = LocalPosition;
            ChunkTilemap = GetComponentInChildren<Tilemap>();
            chunkTilemapRenderer = GetComponentInChildren<TilemapRenderer>();
            Buildings = new List<PlacedBuilding>();
            BuildingGrid = new GridField<GridObject>(GridBuildingSystem.GridSize,GridBuildingSystem.CellSize, transform,
                (field, pos) => new GridObject(this, pos));
        }

        public void Load()
        {
            if(Loaded) return;
            Loaded = true;
            chunkTilemapRenderer.enabled = Loaded;
        }
        
        public void UnLoad()
        {
            if(!Loaded) return;
            Loaded = false;
            chunkTilemapRenderer.enabled = Loaded;
        }
    }
}
