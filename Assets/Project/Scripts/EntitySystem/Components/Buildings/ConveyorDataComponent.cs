using System;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Buildings
{
    [Serializable]
    public struct ConveyorDataComponent : IComponentData
    {
        public Entity head;
    }
}
