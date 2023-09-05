using UnityEngine;

namespace Project.Scripts.Experimental
{
    public class Col : MonoBehaviour
    {
        void Start()
        {
            GetComponent<Renderer>()?.material.SetColor("_ContentColor", new Color(Random.Range(0f,1f),Random.Range(0f,1f),Random.Range(0f,1f)));
        }
    }
}
