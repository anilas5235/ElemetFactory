using System;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Transmission
{
    [Serializable]
    public struct ConveyorTickDataComponent : IComponentData
    {
        public static float Rate = 2;
    }
}
