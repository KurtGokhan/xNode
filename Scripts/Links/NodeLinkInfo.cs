using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XNode {
    public class NodeLinkDefinition {
        public bool IsList;
        public Type LinkType;
        public Type FieldType;
        public string FieldName;
        public FieldInfo FieldInfo;

        public Node.InputAttribute InputAttribute;
        public Node.OutputAttribute OutputAttribute;

        public bool IsInput => InputAttribute != null;
        public bool IsOutput => OutputAttribute != null;

        public void Connect(Node node, NodeLink link) {
            object value = FieldInfo.GetValue(node);
            if (IsList) {
                if (value == null) {
                    Type listType = typeof(List<>);
                    Type constructedListType = listType.MakeGenericType(LinkType);
                    value = Activator.CreateInstance(constructedListType);
                    FieldInfo.SetValue(node, value);
                }
                FieldType.GetMethod("Remove").Invoke(value, new object[] { null });
                FieldType.GetMethod("Add").Invoke(value, new [] { link });
            } else FieldInfo.SetValue(node, link);

            if (IsInput) {
                link.to = node;
                link.toField = FieldName;
            } else {
                link.from = node;
                link.fromField = FieldName;
            }

            link.UpdateName();
        }
        public void Disconnect(Node node, NodeLink link) {
            bool result;
            object value = FieldInfo.GetValue(node);
            if (IsList) {
                result = (bool) FieldType.GetMethod("Remove").Invoke(value, new [] { link });
            } else {
                result = Equals(value, link);
                if (result) FieldInfo.SetValue(node, null);
            }

            if (result && link) {
                if (IsInput) {
                    link.to = null;
                    link.toField = null;
                } else {
                    link.from = null;
                    link.fromField = null;
                }

                link.UpdateName();
            }
        }

        public List<NodeLink> GetConnections(Node node) {
            object value = FieldInfo.GetValue(node);

            if (value == null) return new List<NodeLink>();
            if (IsList) {
                var newList = new List<NodeLink>();
                var collection = value as IEnumerable;
                foreach (var obj in collection) newList.Add(obj as NodeLink);
                return newList;
            } else {
                return new List<NodeLink> { value as NodeLink };
            }
        }
    }
}
