using Project.Scripts.ItemSystem;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Buildings
{
    public struct ExtractorDataComponent : IComponentData
    {
        public uint itemID;
        public OutputSlot output;
    }
}
