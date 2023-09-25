using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    public struct WorldDataComponent : IComponentData
    {
        public NativeList<PositionChunkPair> ChunkDataBank;
    }

    public struct PositionChunkPair
    {
        public PositionChunkPair(Entity chunk, int2 position)
        {
            Chunk = chunk;
            Position = position;
        }

        public int2 Position { get; }
        public Entity Chunk { get; }
    }
}
