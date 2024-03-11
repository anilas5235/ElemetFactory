using UnityEngine;

namespace Project.Scripts.ItemSystem
{
    [CreateAssetMenu(fileName = "ItemScriptableData", menuName = "ElementFactory/Data/Item", order = 0)]
    public class ItemScriptableData : ScriptableObject
    {
        public string trivialName;
        public Sprite sprite;
        public int uniqueID;
    }
}