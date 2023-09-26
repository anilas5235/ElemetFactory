using System;
using Project.Scripts.ItemSystem;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Buildings
{
    public struct ExtractorDataComponent : IComponentData
    {
        public Item item;
        public OutputSlot output;
    }
}
