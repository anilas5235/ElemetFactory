using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UI.InteractableUI.Editor
{
    [CustomPropertyDrawer(typeof(SceneIndexAttribute))]
    public class SceneIndexPropertyDrawer : PropertyDrawer
    {
        private string[] buildIndexSceneNames;
    
        public SceneIndexPropertyDrawer()
        {
            buildIndexSceneNames = EditorBuildSettings.scenes
                .Select(scene => scene.path.Split('/').Last().Split('.').First()).ToArray();
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isIntField = property.propertyType == SerializedPropertyType.Integer;
            if (!isIntField)
            {
                EditorGUI.HelpBox(position, "SceneIndex attribute can only be used in combination with int fields.", MessageType.Warning);
                return;
            }

            position = EditorGUI.PrefixLabel(position, label);
        
            property.intValue = EditorGUI.Popup(position, property.intValue, buildIndexSceneNames);
        }
    }
}
