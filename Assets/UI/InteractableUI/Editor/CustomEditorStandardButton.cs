using System;
using UI.Windows;
using UnityEditor;
using UnityEngine;

namespace UI.InteractableUI.Editor
{
    [CustomEditor(typeof(StandardButtonFunctions))]
    public class CustomEditorStandardButton : UnityEditor.Editor
    {
        protected SerializedProperty
            P_Function,
            P_WindowHandler,
            P_sceneID;

        protected StandardButtonFunctions script;

        private void OnEnable()
        {
            script = (StandardButtonFunctions)target;

            P_Function = serializedObject.FindProperty(nameof(script.myFunction));
            P_WindowHandler = serializedObject.FindProperty(nameof(script.windowHandler));
            P_sceneID = serializedObject.FindProperty(nameof(script.sceneID));
        }

        public override void OnInspectorGUI()
        {
            script = (StandardButtonFunctions)target;

            GUILayout.Space(10);

            EditorGUILayout.PropertyField(P_Function);

            switch (script.myFunction)
            {
                case StandardUIButtonFunctions.Esc:
                    break;
                case StandardUIButtonFunctions.ChangeWindow:
                    HasParameters();
                    EditorGUILayout.PropertyField(P_WindowHandler);
                    EditorGUILayout.EndVertical();
                    break;
                case StandardUIButtonFunctions.OpenWindow:
                    HasParameters();
                    EditorGUILayout.PropertyField(P_WindowHandler);
                    EditorGUILayout.EndVertical();
                    break;
                case StandardUIButtonFunctions.Quit:
                    break;
                case StandardUIButtonFunctions.ChangeScene:
                    HasParameters();
                    EditorGUILayout.PropertyField(P_sceneID);
                    EditorGUILayout.EndVertical();
                    break;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void HasParameters()
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Parameters");
            EditorGUILayout.BeginVertical("Box");
        }
    }
}
