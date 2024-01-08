using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Flags
{
    public struct NewChunkDataComponent : IComponentData
    {
        public int2 Position;
        public int PatchNum;

        public NewChunkDataComponent(int2 position, int patchNum)
        {
            Position = position;
            PatchNum = patchNum;
        }
    }
}