using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.ItemSystem;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.EntitySystem.Baker
{
    public class BuildingMono : MonoBehaviour
    {
        public int Inputs, Outputs;
        public class BuildingMonoBaker : Baker<BuildingMono>
        {
            public override void Bake(BuildingMono authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BuildingDataComponent());

                var inBuff = AddBuffer<InputSlot>(entity);
                for (var i = 0; i < authoring.Inputs; i++)
                {
                    inBuff.Add(new InputSlot());
                }

                var outBuff = AddBuffer<OutputSlot>(entity);
                for (var i = 0; i < authoring.Outputs; i++)
                {
                    outBuff.Add(new OutputSlot());
                }
            }
        }
    }
}