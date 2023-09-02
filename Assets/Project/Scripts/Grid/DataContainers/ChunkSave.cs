using System;
using Project.Scripts.Buildings;
using Project.Scripts.Buildings.BuildingFoundation;
using UnityEngine;

namespace Project.Scripts.Grid.DataContainers
{
    [Serializable]
    public class ChunkSave
    {
        public ChunkResourcePatch[] chunkResourcePatches;
        public PlacedBuildingData[] placedBuildingData;
        public Vector2Int chunkPosition;
    }
}