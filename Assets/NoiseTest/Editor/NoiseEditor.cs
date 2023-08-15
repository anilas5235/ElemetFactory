using UnityEditor;
using UnityEngine;

namespace NoiseTest.Editor
{
    [CustomEditor(typeof(NoiseTest))]
    public class NoiseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            NoiseTest script = (NoiseTest)target;

            if (DrawDefaultInspector()&& script.autoUpdate) script.GenerateMap();

            if(GUILayout.Button("Generate")) script.GenerateMap();
        }
    }
}
