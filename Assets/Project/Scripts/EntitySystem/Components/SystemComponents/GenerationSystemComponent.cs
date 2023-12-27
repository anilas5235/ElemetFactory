using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components
{
    public struct GenerationSystemComponent : IComponentData
    {
        public int PlayerViewRadius , ViewSize;
        public int2 ChunkPosWithPlayer;
        public NativeList<int2> LoadedChunks;

        public bool FirstUpdate; 
    }
}