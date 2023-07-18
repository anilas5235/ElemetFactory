using System;
using Project.Scripts.General;
using Project.Scripts.Grid;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Project.Scripts.Visualisation
{
    [RequireComponent(typeof(Tilemap))]
    public class ResourcesTileMap : Singleton<ResourcesTileMap>
    {
        public Tilemap ResourceMap { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            ResourceMap = GetComponent<Tilemap>();
            ResourceMap.layoutGrid.cellSize = new Vector3(GridBuildingSystem.CellSize,GridBuildingSystem.CellSize);
        }
    }
}
