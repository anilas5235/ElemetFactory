using System;
using Project.Scripts.Buildings;
using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.ItemSystem;
using Unity.Entities;
using UnityEngine;

namespace Project.Scripts.SlotSystem
{
    public enum SlotBehaviour
    {
        InAndOutput,
        Input,
        Output
    }

    public class Slot : MonoBehaviour
    {
        [SerializeField] private SlotBehaviour mySlotBehaviour;
        [SerializeField] private ItemContainer slotContent;
        public ItemContainer SlotContent => slotContent;
        public bool IsOccupied => slotContent != null;
        public Action<bool> OnSlotContentChanged;

        public PlacedBuilding MyBuilding;

        public Slot(SlotBehaviour slotBehaviour = SlotBehaviour.InAndOutput, ItemContainer item = null)
        {
            mySlotBehaviour = slotBehaviour;
            slotContent = item;
        }

        public void PutIntoSlot(ItemContainer item)
        {
            if (IsOccupied || mySlotBehaviour == SlotBehaviour.Output) return;
            slotContent = item;
            slotContent.SetSlot(this);
            OnSlotContentChanged?.Invoke(IsOccupied);
        }

        public ItemContainer  ExtractFromSlot()
        {
            if (!IsOccupied|| mySlotBehaviour == SlotBehaviour.Input) return null;
            ItemContainer item = slotContent;
            slotContent = null;
            OnSlotContentChanged?.Invoke(IsOccupied);
            return item;
        }

        public ItemContainer EmptySlot()
        {
            ItemContainer result = slotContent;
            slotContent = null;
            OnSlotContentChanged?.Invoke(IsOccupied);
            return result;
        }

        public void FillSlot(ItemContainer item)
        {
            slotContent = item;
            slotContent.SetSlot(this);
            slotContent.transform.position = transform.position;
            OnSlotContentChanged?.Invoke(IsOccupied);
        }

        private void OnDestroy()
        {
            if (slotContent) Destroy(slotContent.gameObject);
        }
    }

    public struct SlotDataComponent : IComponentData
    {
        private SlotBehaviour mySlotBehaviour;

        public Entity SlotContent;
        public bool IsOccupied => SlotContent != null;
        
        public Entity MyBuilding;

        public SlotDataComponent(SlotBehaviour mySlotBehaviour, Entity slotContent, Entity myBuilding)
        {
            this.mySlotBehaviour = mySlotBehaviour;
            SlotContent = slotContent;
            MyBuilding = myBuilding;
        }
    }
}
