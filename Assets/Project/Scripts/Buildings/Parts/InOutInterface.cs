using Project.Scripts.Buildings.BuildingFoundation;
using Unity.Entities;

namespace Project.Scripts.Buildings.Parts
{
    public interface IEntityInput
    {
        public bool GetInput(PlacedBuildingEntity caller, out Entity entity, out int inputIndex);
    }
    public interface IEntityOutput
    {
        public bool GetOutput(PlacedBuildingEntity caller, out Entity entity,out int outputIndex);
    }
    
    public interface IContainable<T>
    {
        public Container<T> GetContainer();
    }   
}
