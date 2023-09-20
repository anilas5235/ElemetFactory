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
        private Item _inspectedItem;
        [SerializeField] private RawImage image;
        [SerializeField] private TextMeshProUGUI nameText;

        public void InspectItem(Item item)
        {
            _inspectedItem = item;
            UpdateUIFields();
            OpenSingleWindow();
        }

        private void UpdateUIFields()
        {
            nameText.text = _inspectedItem.ToString();
        }
    }
}
