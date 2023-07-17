
namespace Project.Scripts.CellType
{
    public class CellResources
    {
        public enum ResourcesType
        {
            Hydrogen,
        }

        public CellResources(ResourcesType resourcesType)
        {
            NodeType = resourcesType;
        }

        public ResourcesType NodeType { get; protected set; }

        public override string ToString()
        {
            return NodeType.ToString();
        }
    }
}
