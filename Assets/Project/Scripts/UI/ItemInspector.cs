using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using TMPro;
using UI.Windows;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.UI
{
    public class ItemInspector : SingleWindow<ItemInspector>
    {
        private ItemContainer inspectedItem;
        [SerializeField] private RawImage image;
        [SerializeField] private TextMeshProUGUI name;

        public void InspectItem(ItemContainer item)
        {
            inspectedItem = item;
            UpdateUIFields();
            OpenSingleWindow();
        }

        private void UpdateUIFields()
        {
            image.material = VisualResources.GetItemMaterial(inspectedItem.MyColor, inspectedItem.MyItemForm);
            name.text = inspectedItem.ToString();
        }
    }
}
