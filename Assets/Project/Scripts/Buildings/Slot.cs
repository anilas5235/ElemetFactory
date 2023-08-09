using System;
using UnityEngine;

namespace Project.Scripts.Buildings
{
    [Serializable]
    public class Slot
    {
        [SerializeField] private SlotBehaviour mySlotBehaviour;
        [SerializeField] private Item slotContent;

        public Item SlotContent => slotContent;
        public bool IsOccupied => slotContent != null;
        public Action<bool> OnSlotContentChanged;
        
        public enum SlotBehaviour
        {
            InAndOutput,
            Input,
            Output
        }

        public Slot(SlotBehaviour slotBehaviour = SlotBehaviour.InAndOutput, Item item = null)
        {
            mySlotBehaviour = slotBehaviour;
            slotContent = item;
        }

        public bool PutIntoSlot(Item item)
        {
            if (IsOccupied|| mySlotBehaviour == SlotBehaviour.Output) return false;
            slotContent = item;
            OnSlotContentChanged?.Invoke(IsOccupied);
            return true;
        }

        public bool ExtractFromSlot(out Item item)
        {
            item = null;
            if (!IsOccupied|| mySlotBehaviour == SlotBehaviour.Input) return false;
            item = slotContent;
            slotContent = null;
            OnSlotContentChanged?.Invoke(IsOccupied);
            return true;
        }

        public Item EmptySlot()
        {
            Item result = slotContent;
            slotContent = null;
            OnSlotContentChanged?.Invoke(IsOccupied);
            return result;
        }

        public void FillSlot(Item item)
        {
            slotContent = item;
            OnSlotContentChanged?.Invoke(IsOccupied);
        }
    }
}
