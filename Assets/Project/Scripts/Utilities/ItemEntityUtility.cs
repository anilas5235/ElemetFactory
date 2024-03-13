using Project.Scripts.EntitySystem.Buffer;
using Project.Scripts.EntitySystem.Components.Item;
using Project.Scripts.ItemSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Project.Scripts.Utilities
{
    public static class ItemEntityUtility
    {
        public static NativeKeyValueArrays<int, Entity> ItemPrefabs { get; private set;}
        public static NativeKeyValueArrays<int, Entity> TilePrefabs { get; private set;}

        public static void SetItemPrefabs(NativeKeyValueArrays<int, Entity> itemPrefabs)
        {
            ItemPrefabs = itemPrefabs;
        }
        
        public static void SetTilePrefabs(NativeKeyValueArrays<int, Entity> tilePrefabs)
        {
            TilePrefabs = tilePrefabs;
        }
        
        public static Entity CreateItemEntity(int index, Entity building, OutputSlot outputSlot, int itemID,
            EntityCommandBuffer.ParallelWriter ecb, int worldScale)
        {

            var itemEntity = InstantiateItemEntity(index, ecb, itemID);

            ecb.SetComponent(index, itemEntity, new ItemEntityStateDataComponent()
            {
                Arrived = true,
                DestinationPos = outputSlot.WorldPosition,
                PreviousPos = outputSlot.WorldPosition,
            });

            ecb.SetComponent(index, itemEntity, new LocalTransform()
            {
                Position = outputSlot.WorldPosition,
                Scale = worldScale * .7f,
            });

            ecb.SetComponent(index, itemEntity, new ItemDataComponent()
            {
                ItemID = itemID,
            });

            ecb.SetName(index, itemEntity, "Item");

            ecb.AddComponent(index, building, new NewItemRefHandelDataComponent()
            {
                entity = itemEntity,
                SlotNumber = outputSlot.OwnIndex,
            });
            return itemEntity;
        }

        private static Entity InstantiateItemEntity(int index, EntityCommandBuffer.ParallelWriter ecb, int itemID)
        {
            var prefab = ItemPrefabs.Values[0];
            for (var i = 0; i < ItemPrefabs.Keys.Length; i++)
            {
                var itemPrefabsKey = ItemPrefabs.Keys[i];
                if (itemPrefabsKey == itemID)
                {
                    prefab = ItemPrefabs.Values[i];
                }
            }
            return ecb.Instantiate(index, prefab);
        }
    }
}