using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XNode {
    [Serializable]
    public class NodeLink : ScriptableObject {
        [field : SerializeField, HideInNormalInspector] public Node from { get; internal set; }

        [field : SerializeField, HideInNormalInspector] public Node to { get; internal set; }

        [field : SerializeField, HideInNormalInspector] public string fromField { get; internal set; }

        [field : SerializeField, HideInNormalInspector] public string toField { get; internal set; }

        [field : SerializeField, HideInInspector] public List<Vector2> reroutePoints { get; private set; } = new List<Vector2>();

        static public T Create<T>(Node start, Node end, string startFieldName, string endFieldName) where T : NodeLink {
            return Create(typeof(T), start, end, startFieldName, endFieldName) as T;
        }
        static public NodeLink Create(Type type, Node start, Node end, string startFieldName, string endFieldName) {
            NodeLink link = CreateInstance(type) as NodeLink;
            link.from = start;
            link.to = end;
            link.fromField = startFieldName;
            link.toField = endFieldName;
            link.UpdateName();

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.AddObjectToAsset(link, start);
#endif

            return link;
        }

        [ContextMenu("Delete")]
        public void Destroy() {
            if (from) NodeDataCache.GetLinkCacheInfo(from.GetType(), fromField)?.Disconnect(from, this);
            if (to) NodeDataCache.GetLinkCacheInfo(to.GetType(), toField)?.Disconnect(to, this);

            DestroyImmediate(this, true);
#if UNITY_EDITOR
            AssetDatabase.SaveAssets();
#endif
        }

        public NodeLinkPort GetFromPort() {
            if (!from || fromField == null) return null;
            return new NodeLinkPort(from, NodeDataCache.GetLinkCacheInfo(from.GetType(), fromField));
        }

        public NodeLinkPort GetToPort() {
            if (!to || toField == null) return null;
            return new NodeLinkPort(to, NodeDataCache.GetLinkCacheInfo(to.GetType(), toField));
        }

        public void UpdateName() {
            name = $"{(from == null ? "" : (from.name + "." + fromField))} -> {(to == null ? "" : (to.name + "." + toField))}";
        }
    }
}
