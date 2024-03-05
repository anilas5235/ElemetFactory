using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components.DataObject
{
    public struct TilePrefabsDataComponent : IComponentData
    {
        public Entity Entity;
        public Entity TilePrefab;
        public Entity TileBackGroundPrefab;
    }
}