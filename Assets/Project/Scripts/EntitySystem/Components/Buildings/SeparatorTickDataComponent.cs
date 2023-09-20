using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Buildings
{
    public struct SeparatorTickDataComponent : IComponentData
    {
        public static float Rate = .25f;
    }
}