using Project.Scripts.EntitySystem.Aspects;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Buffer
{
    public struct EntityRefBufferElement : IBufferElementData
    {
        public Entity Entity;
    }
}