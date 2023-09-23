using System;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.Buildings.Parts;
using Project.Scripts.EntitySystem.Components.Buildings;
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
        public ResourceType GeneratedResource { get; private set; }

        protected override void OnCreate()
        {
            if(GeneratedResource == ResourceType.None) return;
            NativeArray<uint> ids = new NativeArray<uint>(1, Allocator.Temp);
            ids[0] = (uint)GeneratedResource;

            ItemMemory.GetItemID(ResourcesUtility.CreateItemData(ids));
            ids.Dispose();
        }

        public bool GetOutput(PlacedBuildingEntity caller, out Entity entity, out int outputIndex)
        {
            entity = default;
            outputIndex = default;
            if (!mySlotValidationHandler.ValidateInputSlotRequest(this, caller, out outputIndex)) return false;
            OutputSlot output =_entityManager.GetComponentData<ExtractorDataComponent>(BuildingEntity).output;
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
