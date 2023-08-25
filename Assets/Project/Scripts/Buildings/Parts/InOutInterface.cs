using Project.Scripts.SlotSystem;

namespace Project.Scripts.Buildings.Parts
{
    public interface IHaveInput
    {
        public Slot GetInputSlot(PlacedBuildingData caller, Slot destination);
    }

    public interface IHaveOutput
    {
        public Slot GetOutputSlot(PlacedBuildingData caller, Slot destination);
    }
}
