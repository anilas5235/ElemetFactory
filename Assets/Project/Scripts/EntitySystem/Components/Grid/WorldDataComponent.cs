using System;
using Project.Scripts.EntitySystem.Aspects;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    public struct WorldDataComponent : IComponentData
    {
        public Entity entity;
        public NativeList<PositionChunkPair> ChunkDataBank;
    }

    [Serializable]
    public struct PositionChunkPair
    {
        public PositionChunkPair(ChunkDataAspect chunk, int2 position)
        {
            Chunk = chunk;
            Position = position;
        }

        public int2 Position { get; }
        public ChunkDataAspect Chunk { get; }
    }
}
