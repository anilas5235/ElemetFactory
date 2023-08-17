using Project.Scripts.Buildings;
using Project.Scripts.Grid;
using UnityEngine;

namespace Project.Scripts.Visualisation
{
    public class ItemContainer : MonoBehaviour
    {
        private Item item;
        public Item Item => item;

        public void SetItem(Item newItem)
        {
            item = newItem;
        }

        public static ItemContainer CreateNewContainer(Item content)
        {
            ItemContainer container = Instantiate(VisualResources.ItemContainer).GetComponent<ItemContainer>();
            container.SetItem(content);
            container.transform.localScale = new Vector3(GridBuildingSystem.CellSize, GridBuildingSystem.CellSize);
            return container;
        }

        public void OnDestroy()
        {
            Destroy(gameObject);
        }
    }
}
