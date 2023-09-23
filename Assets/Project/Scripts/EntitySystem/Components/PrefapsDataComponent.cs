using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components
{
    public struct PrefapsDataComponent : IComponentData
    {
        public readonly Entity Excavator, Conveyor, Separator, Combiner, TrashCan, ItemGas, ItemLiquid, ItemSolid;

        public PrefapsDataComponent(Entity excavator, Entity separator, Entity conveyor, Entity combiner,
            Entity trashCan, Entity itemGas, Entity itemLiquid, Entity itemSolid)
        {
            Excavator = excavator;
            Separator = separator;
            Conveyor = conveyor;
            Combiner = combiner;
            TrashCan = trashCan;
            ItemGas = itemGas;
            ItemLiquid = itemLiquid;
            ItemSolid = itemSolid;
        }
    }
}
