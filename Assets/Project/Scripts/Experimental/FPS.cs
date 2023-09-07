using TMPro;
using UnityEngine;

namespace Project.Scripts.Experimental
{
    public class FPS : MonoBehaviour
    {
        public TextMeshProUGUI TextMeshPro;

        void Update()
        {
            TextMeshPro.text = ((int) (1f / Time.unscaledDeltaTime)).ToString();
        }
    }
}
