using Project.Scripts.ItemSystem;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class PortInterfaceMono : MonoBehaviour
    {
        public int Inputs, Outputs;
    }
    
    public class PortInterfaceBaker : Baker<PortInterfaceMono>
    {
        public override void Bake(PortInterfaceMono authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            
            DynamicBuffer<InputSlot> inBuff = AddBuffer<InputSlot>(entity);
            for (int i = 0; i < authoring.Inputs; i++)
            {
                inBuff.Add(new InputSlot());
            }

            DynamicBuffer<OutputSlot> outBuff = AddBuffer<OutputSlot>(entity);
            for (int i = 0; i < authoring.Outputs; i++)
            {
                outBuff.Add(new OutputSlot());
            }
        }
    }
}