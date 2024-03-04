using Unity.Entities;

namespace Project.Scripts.EntitySystem.Buffer
{
    public struct EntityIDPair : IBufferElementData
    {
        public int ID;
        public Entity Entity;
    }
}