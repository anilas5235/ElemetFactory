using Project.Scripts.EntitySystem.Buffer;
using Unity.Entities;

namespace Project.Scripts.EntitySystem.Systems
{
    [ UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct WorldSetUpSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            Entity worldDataEntity = state.EntityManager.CreateEntity();
            state.EntityManager.AddBuffer<PositionChunkPair>(worldDataEntity);
            
            state.EntityManager.SetName(worldDataEntity,"WorldData");
            GenerationSystem.worldDataEntity = worldDataEntity;
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
        }
    }
}
