using System;
using Random = UnityEngine.Random;

namespace Project.Scripts.Grid.CellType
{
    public class CellResources
    {
        public enum ResourcesType
        {
            H,
            He,
            Li,
            Be,
            Bor,
            C,
            N,
            O,
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
