using System;
using System.Collections;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.EntitySystem.Components.Transmission;
using Project.Scripts.Grid;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Extractor : PlacedBuildingEntity,IEntityOutput
    {
        public ResourceType GeneratedResource { get; private set; }
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
            
            GeneratedResource = MyGridObject.Chunk.ChunkBuildingGrid.GetCellData(MyPlacedBuildingData.origin).ResourceNode;
            if (GeneratedResource != ResourceType.None)
            {
                //TODO: link with dataComponent
            }
            
            CheckForSlotsToPushTo();
            CheckForSlotToPullForm();
        }
    }
}
