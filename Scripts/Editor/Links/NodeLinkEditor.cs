using UnityEditor;
using UnityEngine;
using XNode;

namespace XNodeEditor {
    [CustomEditor(typeof(NodeLink))]
    [CanEditMultipleObjects]
    public class NodeLinkEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();

            if (GUILayout.Button("Delete", GUILayout.Height(40))) {
                (target as NodeLink).Destroy();
                return;
            }

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
