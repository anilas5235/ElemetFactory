using System;
using UnityEngine;

namespace Project.Scripts.Grid.DataContainers
{
    [Serializable]
    public struct ChunkResourcePatch
    {
        public Vector2Int[] positions;
        public int resourceID;
    }
}