using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace DynamicAtlas
{
    [CustomEditor(typeof(DynamicImage))]
    public class DynamicImageEditor : ImageEditor
    {
        private string mEditorLoadingSpriteName;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var dynamicImage = (DynamicImage)target;
            var serializedObject = new SerializedObject(target);

            EditorGUILayout.LabelField("Runtime Debug");
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            mEditorLoadingSpriteName = EditorGUILayout.TextField("SpriteName", mEditorLoadingSpriteName);
            if (GUILayout.Button("Append Sprite To Atlas"))
            {
                dynamicImage.SetDynamicSprite(mEditorLoadingSpriteName);
            }
            EditorGUI.EndDisabledGroup();
        }

        [MenuItem("GameObject/UI/DynamicImage", false, 11)]
        public static void CreateInstance(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            GameObject go = new GameObject("DynamicImage");
            go.AddComponent<DynamicImage>();
            go.transform.SetParent(parent.transform, false);

            Selection.activeGameObject = go;
        }

    }
}

