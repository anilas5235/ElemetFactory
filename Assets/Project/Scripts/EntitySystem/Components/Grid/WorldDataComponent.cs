using System;
using Project.Scripts.EntitySystem.Aspects;
using Project.Scripts.EntitySystem.Systems;
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
        public PositionChunkPair(Entity chunkEntity, int2 position)
        {
            ChunkEntity = chunkEntity;
            Position = position;
        }

        public int2 Position { get; }
        
        public ChunkDataAspect Chunk => GenerationSystem.entityManager.GetAspect<ChunkDataAspect>(ChunkEntity);
        
        public Entity ChunkEntity { get; }
    }
}
