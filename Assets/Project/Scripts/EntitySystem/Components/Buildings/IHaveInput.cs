using Project.Scripts.ItemSystem;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components.Buildings
{
    public interface IHaveInput
    {
        public bool TrySetInput(int index, Entity entityToPullFrom);
    }
}