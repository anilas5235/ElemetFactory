using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.ItemSystem;
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
            
            ConveyorDataComponent conveyorDataComponent = _entityManager.GetComponentData<ConveyorDataComponent>(BuildingEntity);
            InputSlot inputSlot = conveyorDataComponent.input;
            
            if (inputSlot.EntityToPullFrom != default) return;
            
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

                inputSlot.EntityToPullFrom = entity;
                
                _entityManager.SetComponentData(BuildingEntity, new ConveyorDataComponent()
                {
                    input = inputSlot,
                    output = conveyorDataComponent.output,
                });
                break;
            }
        }

        protected override void CheckForSlotsToPushTo()
        {
            ConveyorDataComponent conveyorDataComponent = _entityManager.GetComponentData<ConveyorDataComponent>(BuildingEntity);
            OutputSlot outputSlot = conveyorDataComponent.output;
           
            if (outputSlot.EntityToPushTo != default) return;

            Vector2Int targetPos = PlacedBuildingUtility.FacingDirectionToVector(MyPlacedBuildingData.directionID) +
                                   MyGridObject.Position;
            if (PlacedBuildingUtility.CheckForBuilding(targetPos, MyGridObject.Chunk, out PlacedBuildingEntity building)) return;
            
            IEntityInput entityInput = (IEntityInput) building;
            if(entityInput == null) return;
            if(!entityInput.GetInput(this, out Entity entity, out int index)) return;
            outputSlot.EntityToPushTo = entity;
            
            _entityManager.SetComponentData(BuildingEntity, new ConveyorDataComponent()
            {
                input = conveyorDataComponent.input,
                output = outputSlot,
            });
        }
        
        public bool GetInput(PlacedBuildingEntity caller, out Entity entity, out int inputIndex)
        {
            entity = default;
            inputIndex = default;
            InputSlot input =_entityManager.GetComponentData<ConveyorDataComponent>(BuildingEntity).input;
            if (input.EntityToPullFrom != default) return false;
            input.EntityToPullFrom = entity;
            return true;
        }

        public bool GetOutput(PlacedBuildingEntity caller, out Entity entity, out int outputIndex)
        {
            entity = default;
            outputIndex = default;
            OutputSlot output =_entityManager.GetComponentData<ConveyorDataComponent>(BuildingEntity).output;
            if (output.EntityToPushTo != default) return false;
            output.EntityToPushTo = entity;
            return true;
        }
        
    }
}
