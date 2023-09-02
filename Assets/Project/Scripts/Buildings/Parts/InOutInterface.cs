using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.ItemSystem;
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
    
    public interface IContainable<T>
    {
        public Container<T> GetContainer();
    }

    public interface IConveyorDestination
    {
        public void StartConveyorChainTickUpdate();
    }

    public interface IReceiveConveyorChainTickUpdate
    {
        public void ConveyorChainTickUpdate();
    }
}
