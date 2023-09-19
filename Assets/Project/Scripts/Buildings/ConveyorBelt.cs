using System;
using System.Collections;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.SlotSystem;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class ConveyorBelt : PlacedBuildingEntity, IEntityInput,IEntityOutput
    {
        protected override void SetUpSlots(FacingDirection facingDirection)
        {
            CheckForSlotsToPushTo();
            CheckForSlotToPullForm();
        }

        protected override void CheckForSlotToPullForm()
        {
            var buffer = _entityManager.GetBuffer<InputDataComponent>(BuildingEntity);
            InputDataComponent inputDataComponent = buffer[0];
            
            if (inputDataComponent.EntityToPullFrom != default) return;
            
            Vector2Int[] offsets = new Vector2Int[]
            {
                PlacedBuildingUtility.FacingDirectionToVector(PlacedBuildingUtility.GetOppositeDirection(MyPlacedBuildingData.directionID)) ,
                PlacedBuildingUtility.FacingDirectionToVector(PlacedBuildingUtility.GetNextDirectionClockwise(MyPlacedBuildingData.directionID)) ,
                PlacedBuildingUtility.FacingDirectionToVector(PlacedBuildingUtility.GetNextDirectionCounterClockwise(MyPlacedBuildingData.directionID)) ,
            };

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                if (!PlacedBuildingUtility.CheckForBuilding(offset + MyGridObject.Position, MyGridObject.Chunk,
                        out PlacedBuildingEntity building)) continue;
                
                IEntityOutput entityInput = (IEntityOutput)building;
                if (entityInput == null) continue;
                if (!entityInput.GetOutput(this, out Entity entity, out int index)) continue;
                buffer[i] = new InputDataComponent(inputDataComponent.Position, inputDataComponent.MySlotBehaviour,
                    entity, (byte)index, inputDataComponent.SlotContent);
                break;
            }
        }

        protected override void CheckForSlotsToPushTo()
        {
            DynamicBuffer<OutputDataComponent> buffer = _entityManager.GetBuffer<OutputDataComponent>(BuildingEntity);
            OutputDataComponent outputDataComponent = buffer[0];
            if (outputDataComponent.EntityToPushTo != default) return;

            Vector2Int targetPos = PlacedBuildingUtility.FacingDirectionToVector(MyPlacedBuildingData.directionID) +
                                   MyGridObject.Position;
            if (PlacedBuildingUtility.CheckForBuilding(targetPos, MyGridObject.Chunk, out PlacedBuildingEntity building)) return;
            
            IEntityInput entityInput = (IEntityInput) building;
            if(entityInput == null) return;
            if(!entityInput.GetInput(this, out Entity entity, out int index)) return;
            buffer[0] = new OutputDataComponent(outputDataComponent.Position, outputDataComponent.MySlotBehaviour,
                entity,(byte)index, outputDataComponent.SlotContent);
        }
        
        public bool GetInput(PlacedBuildingEntity caller, out Entity entity, out int inputIndex)
        {
            entity = default;
            inputIndex = default;
            InputDataComponent input =_entityManager.GetBuffer<InputDataComponent>(BuildingEntity)[inputIndex];
            if (input.EntityToPullFrom != default) return false;
            input.EntityToPullFrom = entity;
            return true;
        }

        public bool GetOutput(PlacedBuildingEntity caller, out Entity entity, out int outputIndex)
        {
            entity = default;
            outputIndex = default;
            OutputDataComponent output =_entityManager.GetBuffer<OutputDataComponent>(BuildingEntity)[outputIndex];
            if (output.EntityToPushTo != default) return false;
            output.EntityToPushTo = entity;
            return true;
        }
        
    }
}
