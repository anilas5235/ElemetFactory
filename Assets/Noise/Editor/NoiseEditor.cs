/*
 * based on the work of Sebastian Lague https://www.youtube.com/@SebastianLague 
 */
using UnityEditor;
using UnityEngine;

namespace Noise.Editor
{
    [CustomEditor(typeof(NoiseDrawer))]
    public class NoiseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            NoiseDrawer script = (NoiseDrawer)target;

            if (DrawDefaultInspector()&& script.autoUpdate) script.GenerateMap();

            if(GUILayout.Button("Generate")) script.GenerateMap();
        }

        private void OnValidate()
        {
            NoiseDrawer script = (NoiseDrawer)target;
            if (script.size.x < 5) script.size.x = 5;
            if (script.size.y < 5) script.size.y = 5;
            if (script.octaves < 1) script.octaves = 1;
            if (script.lacunarity < 1) script.lacunarity = 1;
        }
    }
}
