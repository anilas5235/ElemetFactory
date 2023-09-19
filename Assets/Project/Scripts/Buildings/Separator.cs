using System;
using System.Collections;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Separator : PlacedBuildingEntity, IEntityInput,IEntityOutput
    {

        protected override void SetUpSlots(FacingDirection facingDirection)
        {
            mySlotValidationHandler = MyPlacedBuildingData.directionID switch
            {
                0 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Separator/SeparatorUp"),
                1 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Separator/SeparatorRight"),
                2 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Separator/SeparatorDown"),
                3 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Separator/SeparatorLeft"),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            CheckForSlotToPullForm();
            CheckForSlotsToPushTo();
        }
        
        public bool GetInput(PlacedBuildingEntity caller, out Entity entity, out int inputIndex)
        {
            entity = default;
            inputIndex = default;
            if (!mySlotValidationHandler.ValidateInputSlotRequest(this, caller, out inputIndex)) return false;
            InputDataComponent input =_entityManager.GetBuffer<InputDataComponent>(BuildingEntity)[inputIndex];
            if (input.EntityToPullFrom != default) return false;
            input.EntityToPullFrom = entity;
            return true;
        }

        public bool GetOutput(PlacedBuildingEntity caller, out Entity entity, out int outputIndex)
        {
            entity = default;
            outputIndex = default;
            if (!mySlotValidationHandler.ValidateInputSlotRequest(this, caller, out outputIndex)) return false;
            OutputDataComponent output =_entityManager.GetBuffer<OutputDataComponent>(BuildingEntity)[outputIndex];
            if (output.EntityToPushTo != default) return false;
            output.EntityToPushTo = entity;
            return true;
        }
    }
}
