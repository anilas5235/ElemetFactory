using System;
using Project.Scripts.SlotSystem;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.Transmission
{
    [Serializable]
    public struct InOutDataComponent : IComponentData
    {
        public InputData[] inputs;
        public OutputData[] outputs;
    }
    
    [Serializable]
    public struct OutputData
    {
        public SlotDataComponent OutputSlot;
        public SlotDataComponent SlotToPushTo;

        public OutputData(SlotDataComponent outputSlot)
        {
            OutputSlot = outputSlot;
            SlotToPushTo = default;
        }
    }
    
    [Serializable]
    public struct InputData
    {
        public SlotDataComponent Slot;
        public SlotDataComponent SlotToPullFrom;

        public InputData(SlotDataComponent slot)
        {
            Slot = slot;
            SlotToPullFrom = default;
        }
    }
}
