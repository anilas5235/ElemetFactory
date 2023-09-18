using Project.Scripts.Buildings.BuildingFoundation;
using Project.Scripts.ItemSystem;
using Project.Scripts.SlotSystem;
using Unity.Entities;

namespace Project.Scripts.Buildings.Parts
{
    public interface IHaveInput
    {
        public Slot GetInputSlot(PlacedBuilding caller, Slot destination);
    }

    public interface IEntityInput
    {
        public bool GetInput(PlacedBuildingEntity caller, out Entity entity, out int bufferIndex);
    }

    public interface IHaveOutput
    {
        public Slot GetOutputSlot(PlacedBuilding caller, Slot destination);
    }
    
    public interface IEntityOutput
    {
        public bool GetOutput(PlacedBuildingEntity caller, out Entity entity,out int bufferIndex);
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
