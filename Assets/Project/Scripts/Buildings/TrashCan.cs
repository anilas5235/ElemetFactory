using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.SlotSystem;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class TrashCan : PlacedBuildingEntity, IEntityInput
    {
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

        protected override void SetUpSlots(FacingDirection facingDirection)
        {
            mySlotValidationHandler = MyPlacedBuildingData.directionID switch
            {
                0 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/TrashCan/TrashCanUp"),
                1 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/TrashCan/TrashCanRight"),
                2 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/TrashCan/TrashCanDown"),
                3 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/TrashCan/TrashCanLeft"),
                _ => throw new ArgumentOutOfRangeException()
            };
            
            CheckForSlotToPullForm();
        }
    }
}
