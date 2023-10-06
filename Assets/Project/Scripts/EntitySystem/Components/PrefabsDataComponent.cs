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
            SolidTile;

        public PrefabsDataComponent(Entity selfEntity, Entity itemGas, Entity itemLiquid, Entity itemSolid,
            Entity tileVisual, Entity gasTile, Entity liquidTile, Entity solidTile)
        {
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
