using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace XNode {
    public class NodeLinkPort : Tuple<Node, NodeLinkDefinition> {
        public bool IsOutput => Item2.OutputAttribute != null;
        public bool IsInput => Item2.InputAttribute != null;
        public bool IsConnected => GetConnections().Count > 0;

        public Node Node => Item1;
        public NodeLinkDefinition Link => Item2;

        public NodeLinkPort(Node item1, NodeLinkDefinition item2) : base(item1, item2) { }

        public List<NodeLink> GetConnections() {
            return Item2.GetConnections(Item1);
        }

        public NodeLink GetConnectionTo(Node other) {
            return Item2.GetConnections(Item1).FirstOrDefault(x => IsInput ? x.from == other : x.to == other);
        }

        public NodeLink GetConnectionTo(NodeLinkPort other) {
            return Item2.GetConnections(Item1).FirstOrDefault(x => IsInput ? (x.from == other.Node && x.fromField == other.Link.FieldName) : (x.to == other.Node && x.toField == other.Link.FieldName));
        }

        public NodeLink GetEmptyLink() {
            var existing = GetConnections().Find(x => IsInput ? x.from == null : x.to == null);
            if (existing) return existing;

            var linkType = Link.LinkType;

            NodeLink link;
            if (IsOutput) link = NodeLink.Create(linkType, Node, null, Link.FieldName, null);
            else link = NodeLink.Create(linkType, null, Node, null, Link.FieldName);

            Link.Connect(Node, link);

            return link;
        }

        public bool IsConnectedTo(NodeLinkPort hoveredLink) {
            List<NodeLink> thisConnections = GetConnections();
            List<NodeLink> otherConnections = hoveredLink.GetConnections();

            foreach (var con in thisConnections) {
                foreach (var oth in otherConnections) {
                    if (con == oth) return true;
                }
            }

            return false;
        }

        public bool CanConnectTo(NodeLinkPort other) {
            List<NodeLink> thisConnections = GetConnections();
            List<NodeLink> otherConnections = other.GetConnections();

            bool canConnectThis = Link.IsList || thisConnections.Count < 1;
            bool canConnectOther = other.Link.IsList || otherConnections.Count < 1;

            return canConnectThis && canConnectOther;
        }

        public NodeLink Connect(NodeLinkPort other) {
            NodeLink link = GetEmptyLink();
            other.Link.Connect(other.Node, link);
            return link;
        }
        public void Disconnect(NodeLinkPort other) {
            var existing = GetConnections().Find(x => x.to == other.Node || x.from == other.Node);
            if (existing) other.Link.Disconnect(other.Node, existing);
        }

        /// <summary>
        /// Disconnects the last link from this Node.
        /// </summary>
        /// <returns>The disconnected node, or null if nothing was found to disconnect.</returns>
        public NodeLink DisconnectSelf() {
            var link = GetConnections().LastOrDefault();
            if (!link) return null;

            Link.Disconnect(Node, link);

            return link;
        }

        public void VerifyConnections() {
            var nullLinks = GetConnections().Select((x, i) => Tuple.Create(x, i)).Where(y => y.Item1 == null).ToList();

            for (int i = 0; i < nullLinks.Count; i++) {
                Debug.LogWarning($"A link was null or destroyed for node '{Node.name}'. Removing it from the node.");
                Link.Disconnect(Node, null);
            }

            var missingThis = GetConnections().Where(x => IsInput ? x.to != Node : x.from != Node);

            foreach (var link in missingThis) {
                Debug.LogWarning($"{link.name} was not connected to correct node. Destroying.");
                link.Destroy();
            }

            var missingOther = GetConnections().Where(x => IsInput ? x.from == null : x.to == null);

            foreach (var link in missingOther) {
                Debug.LogWarning($"{link.name} was not connected to any node. Destroying.");
                link.Destroy();
            }
        }
    }
}
