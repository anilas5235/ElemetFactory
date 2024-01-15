using System;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.Grid.DataContainers
{
    [Serializable]
    public struct ChunkResourcePatch
    {
        public int2[] positions;
        public uint resourceID;
    }
}