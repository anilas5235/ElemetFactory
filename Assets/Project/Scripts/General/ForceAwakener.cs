using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Scripts.General
{
    /// <summary>
    ///   <para>All classes that implement the "IShouldForceAwake" interface will have the "ForceAwake" function called called in the unity awake context by this class</para>   
    /// </summary>
    public class ForceAwakener : MonoBehaviour
    {
        private void Awake()
        {
            List<IShouldForceAwake> scripts = new List<IShouldForceAwake>();
            Scene scene = SceneManager.GetActiveScene();
 
            GameObject[] rootObjects = scene.GetRootGameObjects();
 
            foreach (GameObject go in rootObjects)
                scripts.AddRange(go.GetComponentsInChildren<IShouldForceAwake>(true));
            foreach (IShouldForceAwake script in scripts)
                script.ForceAwake();
        }
    }
}
