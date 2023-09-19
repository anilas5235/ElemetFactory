using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using TMPro;
using UI.Windows;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Project.Scripts.UI
{
    public class ItemInspector : SingleWindow<ItemInspector>
    {
        private ItemContainer _inspectedItem;
        [SerializeField] private RawImage image;
        [SerializeField] private TextMeshProUGUI nameText;

        public void InspectItem(ItemContainer item)
        {
            _inspectedItem = item;
            UpdateUIFields();
            OpenSingleWindow();
        }

        private void UpdateUIFields()
        {
            image.material = VisualResources.GetItemMaterial(_inspectedItem.MyColor, _inspectedItem.MyItemForm);
            nameText.text = _inspectedItem.ToString();
        }
    }
}
