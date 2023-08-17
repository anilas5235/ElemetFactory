using System;
using Project.Scripts.Visualisation;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class Slot : MonoBehaviour
    {
        [SerializeField] private SlotBehaviour mySlotBehaviour;
        [SerializeField] private ItemContainer slotContent;

        public ItemContainer SlotContent => slotContent;
        public bool IsOccupied => slotContent != null;
        public Action<bool> OnSlotContentChanged;
        
        public enum SlotBehaviour
        {
            InAndOutput,
            Input,
            Output
        }

        public Slot(SlotBehaviour slotBehaviour = SlotBehaviour.InAndOutput, ItemContainer item = null)
        {
            mySlotBehaviour = slotBehaviour;
            slotContent = item;
        }

        public void PutIntoSlot(ItemContainer item)
        {
            if (IsOccupied || mySlotBehaviour == SlotBehaviour.Output) return;
            slotContent = item;
            slotContent.transform.position = transform.position;
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
            slotContent.transform.position = transform.position;
            OnSlotContentChanged?.Invoke(IsOccupied);
        }
    }
}
