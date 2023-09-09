using Project.Scripts.ItemSystem;
using Project.Scripts.Utilities;
using UnityEngine;

namespace Project.Scripts.Experimental
{
    public class LoadMaterialTest : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Material material =  VisualResources.GetItemMaterial(new Color(0.3f, 0.1f, 0.5f), ItemForm.Fluid);
            Debug.Log(material);
        }
    }
}
