using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.EntitySystem.Components.Buildings;
using Project.Scripts.EntitySystem.Systems;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using Project.Scripts.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Extractor : PlacedBuildingEntity,IEntityOutput
    {
        public Item GeneratedResource { get; private set; }

        protected override void OnCreate()
        {
            if(GeneratedResource.ResourceIDs.Length < 1) return;
        }

        public bool GetOutput(PlacedBuildingEntity caller, out Entity entity, out int outputIndex)
        {
            entity = default;
            outputIndex = default;
            if (!mySlotValidationHandler.ValidateInputSlotRequest(this, caller, out outputIndex)) return false;
            OutputSlot output =entityManager.GetComponentData<ExtractorDataComponent>(BuildingEntity).output;
            if (output.EntityToPushTo != default) return false;
            output.EntityToPushTo = entity;
            return true;
        }

        protected override void SetUpSlots(FacingDirection facingDirection)
        {
            mySlotValidationHandler = MyPlacedBuildingData.directionID switch
            {
                0 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Extractor/ExtractorUp"),
                1 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Extractor/ExtractorRight"),
                2 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Extractor/ExtractorDown"),
                3 => Resources.Load<SlotValidationHandler>("Buildings/SlotValidation/Extractor/ExtractorLeft"),
                _ => throw new ArgumentOutOfRangeException()
            };

            GeneratedResource = MyCellObject.Resource;
            if (GeneratedResource.ResourceIDs.Length > 0)
            {
                //TODO: link with dataComponent
            }
            
            CheckForSlotsToPushTo();
            CheckForSlotToPullForm();
        }

        protected override void CheckForSlotToPullForm()
        {
            throw new NotImplementedException();
        }

        protected override void CheckForSlotsToPushTo()
        {
            throw new NotImplementedException();
        }
    }
}
