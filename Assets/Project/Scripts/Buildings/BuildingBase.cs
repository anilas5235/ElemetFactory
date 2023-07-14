using UnityEngine;

namespace Project.Scripts.Buildings
{
    public class BuildingBase : MonoBehaviour
    {
        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
