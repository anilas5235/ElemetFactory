using System;
using Project.Scripts.Buildings;
using UnityEngine;

namespace Project.Scripts.Grid.DataContainers
{
    [Serializable]
    public class ChunkSave
    {
        public ChunkResourcePoint[] chunkResourcePoints;
        public PlacedBuildingData[] placedBuildingData;
        public Vector3 localPosition;
        public Vector2Int chunkPosition;
    }
}