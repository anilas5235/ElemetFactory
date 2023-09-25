using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Grid
{
    public struct CellObject
    {
        public CellObject(int2 position, int2 chunkPosition, Entity visuals, NativeList<ResourcePatch> resourcePatches)
        {
            Position = position;
            ChunkPosition = chunkPosition;
            Visuals = visuals;
            ResourcePatches = resourcePatches;
            Building = default;
            Resource = default;
        }

        public int2 ChunkPosition { get; }
        public int2 Position { get; }
        public Entity Building;
        public bool IsOccupied => Building != default;
        public Item Resource;
        public NativeList<ResourcePatch> ResourcePatches { get; }
        public Entity Visuals { get; }
    }
}