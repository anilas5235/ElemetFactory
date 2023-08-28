using Project.Scripts.SlotSystem;

namespace Project.Scripts.Buildings.Parts
{
    public interface IHaveInput
    {
        public Slot GetInputSlot(PlacedBuilding caller, Slot destination);
    }

    public interface IHaveOutput
    {
        public Slot GetOutputSlot(PlacedBuilding caller, Slot destination);
    }
}
