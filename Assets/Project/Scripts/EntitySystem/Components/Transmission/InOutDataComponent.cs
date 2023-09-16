using System;
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
        public SlotDataComponent slot;
        public SlotDataComponent slotToPushTo;

        public OutputData(SlotDataComponent outputSlot)
        {
            slot = outputSlot;
            slotToPushTo = default;
        }
    }
    
    [Serializable]
    public struct InputData
    {
        public SlotDataComponent slot;
        public SlotDataComponent slotToPullFrom;

        public InputData(SlotDataComponent inputSlot)
        {
            slot = inputSlot;
            slotToPullFrom = default;
        }
    }
}
