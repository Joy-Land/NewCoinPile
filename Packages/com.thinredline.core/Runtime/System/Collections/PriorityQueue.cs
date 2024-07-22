using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Tools
{
    internal class Node<T>
    {
        public T Item { get; set; }

        public float Priority { get; set; }

        public int Index { get; set; }

        public override string ToString()
        {
            return Item.ToString();
        }

        public static bool operator <=(Node<T> leftNode, Node<T> rightNode) => leftNode.Priority <= rightNode.Priority;
        public static bool operator >=(Node<T> leftNode, Node<T> rightNode) => leftNode.Priority >= rightNode.Priority;
        public static bool operator <(Node<T> leftNode, Node<T> rightNode) => leftNode.Priority < rightNode.Priority;
        public static bool operator >(Node<T> leftNode, Node<T> rightNode) => leftNode.Priority > rightNode.Priority;

    }

    internal class Nodes<T> : IEnumerable<T>
    {
        List<Node<T>> nodes = new List<Node<T>>();

        public int Count => nodes.Count - 1;

        public Node<T> First => (Count <= 0) ? null : nodes[1];

        public Node<T> Last => (Count <= 0) ? null : nodes[nodes.Count - 1];

        public Node<T> this[int idx]
        {
            get
            {
                if (idx == 0)
                    throw new IndexOutOfRangeException("0?? Index? ???? ????!");

                return nodes[idx];
            }

            set
            {
                if (idx == 0)
                    throw new IndexOutOfRangeException("0?? Index? ???? ????!");

                nodes[idx] = value;
            }
        }

        public Nodes()
        {
            nodes.Add(null);
        }

        public void Add(Node<T> node) => nodes.Add(node);

        public void Clear()
        {
            nodes.Clear();
            nodes.Add(null);
        }

        public void Remove(Node<T> node) => nodes.Remove(node);

        public Node<T> Find(T item)
        {
            return nodes.Find(node => node?.Item.Equals(item) == true);

        }

        public IEnumerator<T> GetEnumerator()
        {
            return new NodesEnumeraotr(nodes);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new NodesEnumeraotr(nodes);
        }

        class NodesEnumeraotr : IEnumerator<T>
        {
            List<Node<T>> nodes;
            int position = 0;

            public T Current
            {
                get
                {
                    try
                    {
                        return nodes[position].Item;
                    }
                    catch (IndexOutOfRangeException)
                    {

                        throw new InvalidOperationException();
                    }
                }
            }

            object IEnumerator.Current => Current;


            public NodesEnumeraotr(List<Node<T>> nodes)
            {
                this.nodes = nodes;
            }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                position++;
                return position < nodes.Count;
            }

            public void Reset()
            {
                position = 0;
            }
        }

    }

    public class PriorityQueue<T> : IEnumerable<T>
    {
        Nodes<T> nodes = new Nodes<T>();

        public PriorityQueue()
        {

        }

        public int Count => nodes.Count;

        public void Enqueue(T item, float value)
        {
            var node = new Node<T>()
            {
                Item = item,
                Priority = value,
                Index = nodes.Count + 1
            };

            nodes.Add(node);
            CascadeUp(node);
        }

        public T Dequeue()
        {
            var firstNode = nodes.First;
            if (firstNode == null)
                return default(T);

            if (nodes.Count < 2)
            {
                nodes.Remove(firstNode);
                return firstNode.Item;
            }

            var lastNode = nodes.Last;

            nodes.Remove(lastNode);

            lastNode.Index = firstNode.Index;
            nodes[lastNode.Index] = lastNode;

            CascadeDown(lastNode);

            return firstNode.Item;
        }

        public T Peek()
        {
            var firstNode = nodes.First;
            if (firstNode == null)
                return default(T);
            //if (nodes.Count < 2)
            {
                return firstNode.Item;
            }

        }

        public void UpdatePriority(T item, float prioirty)
        {
            if (!Contains(item))
                throw new InvalidOperationException("itme? ?? Queue? ????");

            var node = nodes.Find(item);
            node.Priority = prioirty;

            Update(node);
        }

        public void Remove(T item)
        {
            if (!Contains(item))
                throw new InvalidOperationException("itme? ?? Queue? ????");

            var node = nodes.Find(item);
            var changedNode = nodes.Last;
            nodes.Remove(changedNode);

            if (node == changedNode)
                return;

            changedNode.Index = node.Index;
            nodes[changedNode.Index] = changedNode;

            Update(changedNode);
        }

        public bool Contains(T item) => nodes.Contains(item);

        public void Clear() => nodes.Clear();

        void Update(Node<T> node)
        {
            var parentNodeIdx = node.Index >> 1;

            if (parentNodeIdx > 0 && node < nodes[parentNodeIdx])
                CascadeUp(node);
            else
                CascadeDown(node);
        }

        void CascadeUp(Node<T> node)
        {
            if (nodes.Count <= 1)
                return;

            var parentIdx = node.Index;
            while (parentIdx > 1)
            {
                parentIdx >>= 1;
                var parentNode = nodes[parentIdx];

                if (parentNode <= node)
                    break;

                nodes[node.Index] = parentNode;
                parentNode.Index = node.Index;

                node.Index = parentIdx;
            }

            nodes[node.Index] = node;
        }

        void CascadeDown(Node<T> node)
        {
            var leftParentIdx = node.Index * 2;

            while (leftParentIdx <= nodes.Count)
            {
                var rightParentIdx = leftParentIdx + 1;

                var leftParentNode = nodes[leftParentIdx];
                var rightParentNode = (rightParentIdx <= nodes.Count) ? nodes[rightParentIdx] : null;

                var targetParentNode = (rightParentNode != null && rightParentNode <= leftParentNode) ? rightParentNode : leftParentNode;
                if (targetParentNode >= node)
                    break;

                var targetParentIdx = targetParentNode.Index;
                targetParentNode.Index = node.Index;
                nodes[targetParentNode.Index] = targetParentNode;

                node.Index = targetParentIdx;

                leftParentIdx = node.Index * 2;
            }

            nodes[node.Index] = node;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

    }

}
