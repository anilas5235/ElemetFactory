using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Experimental.Editor
{
    [CustomEditor(typeof(MaterialCreator))]
    public class MaterialCreatorCustomEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            MaterialCreator materialCreator = (MaterialCreator)target;
            base.OnInspectorGUI();
            if (GUILayout.Button("Create"))
            {
                materialCreator.CreateMaterials();
            }
        }
    }
}
