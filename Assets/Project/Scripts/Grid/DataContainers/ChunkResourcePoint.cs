using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.Grid.DataContainers
{
    [Serializable]
    public struct ChunkResourcePoint
    {
        public Vector2Int position;
        public int resourceID;
    }
}