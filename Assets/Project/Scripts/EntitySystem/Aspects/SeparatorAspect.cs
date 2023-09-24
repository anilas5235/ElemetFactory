using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Aspects
{
    public readonly partial struct SeparatorAspect : IAspect
    {
        private readonly RefRW<SeparatorDataComponent> data;
        public NativeArray<OutputSlot> Outputs
        {
            get => data.ValueRO.outputs;
            set => data.ValueRW.outputs = value;
        }

        public OutputSlot OutputSlot0
        {
            get => data.ValueRO.outputs[0];
            set => data.ValueRW.outputs[0] = value;
        }
        
        public OutputSlot OutputSlot1
        {
            get => data.ValueRO.outputs[1];
            set => data.ValueRW.outputs[1] = value;
        }

        public InputSlot Input => data.ValueRW.input;

        public Entity ItemEntityInInput
        {
            get => data.ValueRO.input.SlotContent;
            set => data.ValueRW.input.SlotContent = value;
        }
    }
}
