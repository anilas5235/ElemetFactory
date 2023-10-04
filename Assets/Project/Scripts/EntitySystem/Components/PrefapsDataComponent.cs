using Unity.Entities;

namespace Project.Scripts.EntitySystem.Components
{
    public struct PrefapsDataComponent : IComponentData
    {
        public readonly Entity entity;

        public readonly Entity Excavator,
            Conveyor,
            Separator,
            Combiner,
            TrashCan,
            ItemGas,
            ItemLiquid,
            ItemSolid,
            TileVisual,
            GasTile,
            LiquidTile,
            SolidTile;

        public PrefapsDataComponent(Entity selfEntity, Entity excavator, Entity separator, Entity conveyor,
            Entity combiner, Entity trashCan, Entity itemGas, Entity itemLiquid, Entity itemSolid,
            Entity tileVisual, Entity gasTile, Entity liquidTile, Entity solidTile)
        {
            Excavator = excavator;
            Separator = separator;
            Conveyor = conveyor;
            Combiner = combiner;
            TrashCan = trashCan;
            ItemGas = itemGas;
            ItemLiquid = itemLiquid;
            ItemSolid = itemSolid;
            TileVisual = tileVisual;
            GasTile = gasTile;
            LiquidTile = liquidTile;
            SolidTile = solidTile;
            entity = selfEntity;
        }
    }
}
