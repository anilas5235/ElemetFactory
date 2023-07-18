
using System;
using Random = UnityEngine.Random;

namespace Project.Scripts.CellType
{
    public class CellResources
    {
        public enum ResourcesType
        {
            None,
            Hydrogen,
            Helium,
            Lithium,
            Beryllium,
            Boron,
            Carbon,
            Nitrogen,
            Oxygen,
        }

        public CellResources(ResourcesType resourcesType)
        {
            NodeType = resourcesType;
        }
        
        public CellResources()
        {
            NodeType = (ResourcesType) Random.Range(0,Enum.GetNames(typeof(ResourcesType)).Length);
        }

        public ResourcesType NodeType { get; protected set; }

        public override string ToString()
        {
            return NodeType.ToString();
        }
    }
}
