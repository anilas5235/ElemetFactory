using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components
{
    public struct TimeRateDataComponent : IComponentData
    {
        public float timeSinceLastTick;
        public float Rate;
    }
}