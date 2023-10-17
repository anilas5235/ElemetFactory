using System;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Experimental.Editor
{
    [CustomEditor(typeof(MeshCreator))]
    public class MeshCreatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            MeshCreator script = (MeshCreator)target;
            base.OnInspectorGUI();
            if (GUILayout.Button("Generate"))
            {
                script.CreateMesh();
            }
        }
    }
}
