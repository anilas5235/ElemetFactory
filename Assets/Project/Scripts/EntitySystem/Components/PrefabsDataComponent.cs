using System;
using System.Linq;
using Project.Scripts.Buildings.BuildingFoundation;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Project.Scripts.EntitySystem.Components
{
    public struct PrefabsDataComponent : IComponentData
    {
        public readonly Entity entity;

        public readonly Entity
            ItemGas,
            ItemLiquid,
            ItemSolid,
            TileVisual,
            GasTile,
            LiquidTile,
            SolidTile,
            Extractor,
            Conveyor,
            Combiner,
            Separator,
            TrashCan;

        public PrefabsDataComponent(Entity selfEntity, Entity itemGas, Entity itemLiquid, Entity itemSolid,
            Entity tileVisual, Entity gasTile, Entity liquidTile, Entity solidTile, Entity extractor, Entity conveyor,
            Entity combiner, Entity separator, Entity trashCan)
        {
            ItemGas = itemGas;
            ItemLiquid = itemLiquid;
            ItemSolid = itemSolid;
            TileVisual = tileVisual;
            GasTile = gasTile;
            LiquidTile = liquidTile;
            SolidTile = solidTile;
            Extractor = extractor;
            Conveyor = conveyor;
            Combiner = combiner;
            Separator = separator;
            TrashCan = trashCan;
            entity = selfEntity;
        }
    }
}
