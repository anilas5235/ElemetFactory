using System;
using UnityEngine;

namespace Project.Scripts.Grid.DataContainers
{
    [Serializable]
    public struct ChunkResourcePoint
    {
        public Vector2Int position;
        public BuildingGridResources.ResourcesType resource;
    }
}