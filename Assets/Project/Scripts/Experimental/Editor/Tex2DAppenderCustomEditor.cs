using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Experimental.Editor
{
    [CustomEditor(typeof(Tex2DAppender))]
    public class Tex2DAppenderCustomEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            Tex2DAppender script = (Tex2DAppender)target;
            if (GUILayout.Button("Generate"))
            {
                script.MakeAtlas();
            }
        }
    }
}