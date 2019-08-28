using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yuzu.Collections
{
    /// <remarks>
    /// 列挙中のコレクション変更に対応してないよ
    /// </remarks>
    public class AVLTree<T> : ICollection<T>
    {
        protected Node Root;
        protected Func<T, int> Selector { get; }
        protected Func<T, T, int> Comparer { get; }

        public bool IsReadOnly => false;
        public int Count { get; private set; }


        public AVLTree(Func<T, int> selector)
        {
            Selector = selector;
            Comparer = (a, b) => selector(a) - selector(b);
            Initialize();
        }

        public AVLTree(Func<T, int> selector, IEnumerable<T> collection) : this(selector)
        {
            foreach (var item in collection) Add(item);
        }

        protected void Initialize()
        {
            Root = null;
            Count = 0;
        }

        protected int GetHeight(Node v) => v?.Height ?? 0;
        protected int GetBias(Node v) => GetHeight(v.Left) - GetHeight(v.Right);
        protected void BalanceHeight(Node v)
        {
            v.Height = 1 + Math.Max(GetHeight(v.Left), GetHeight(v.Right));
        }

        protected Node RotateL(Node v)
        {
            Node u = v.Right;
            Node t2 = u.Left;
            u.Left = v;
            v.Right = t2;
            BalanceHeight(u.Left);
            BalanceHeight(u);
            return u;
        }

        protected Node RotateR(Node v)
        {
            Node u = v.Left;
            Node t2 = u.Right;
            u.Right = v;
            v.Left = t2;
            BalanceHeight(u.Right);
            BalanceHeight(u);
            return u;
        }

        protected Node RotateLR(Node v)
        {
            v.Left = RotateL(v.Left);
            return RotateR(v);
        }

        protected Node RotateRL(Node v)
        {
            v.Right = RotateR(v.Right);
            return RotateL(v);
        }

        protected Node BalanceL(Node v)
        {
            int h = GetHeight(v);
            if (GetBias(v) == 2)
            {
                v = GetBias(v.Left) >= 0 ? RotateR(v) : RotateLR(v);
            }
            else BalanceHeight(v);
            return v;
        }

        protected Node BalanceR(Node v)
        {
            int h = GetHeight(v);
            if (GetBias(v) == -2)
            {
                v = GetBias(v.Right) <= 0 ? RotateL(v) : RotateRL(v);
            }
            else BalanceHeight(v);
            return v;
        }

        protected Node Insert(Node v, T item)
        {
            if (v == null)
            {
                return new Node(item) { Height = 1 };
            }
            else if (Comparer(item, v.Value) < 0)
            {
                v.Left = Insert(v.Left, item);
                return BalanceL(v);
            }
            else if (Comparer(item, v.Value) > 0)
            {
                v.Right = Insert(v.Right, item);
                return BalanceR(v);
            }
            else
            {
                throw new ArgumentException("The key already exists.");
            }
        }

        protected Node Remove(Node v, T item)
        {
            if (v == null)
            {
                return v;
            }
            else if (Comparer(item, v.Value) < 0)
            {
                v.Left = Remove(v.Left, item);
                return BalanceR(v);
            }
            else if (Comparer(item, v.Value) > 0)
            {
                v.Right = Remove(v.Right, item);
                return BalanceL(v);
            }
            else
            {
                if (v.Left == null)
                {
                    return v.Right;
                }
                else
                {
                    (Node u, T max) = RemoveMax(v.Left);
                    v.Left = u;
                    v.Value = max;
                    return BalanceR(v);
                }
            }
        }

        protected (Node, T) RemoveMax(Node v)
        {
            if (v.Right != null)
            {
                (Node u, T max) = RemoveMax(v.Right);
                return (BalanceL(v), max);
            }
            else
            {
                return (v.Left, v.Value);
            }
        }

        public void Add(T item)
        {
            Root = Insert(Root, item);
            Count++;
        }

        public bool Remove(T item)
        {
            if (!Contains(item)) return false;
            Root = Remove(Root, item);
            Count--;
            return true;
        }

        public bool Contains(T item)
        {
            Node v = Root;
            while (v != null)
            {
                if (Comparer(item, v.Value) == 0) return true;
                v = Comparer(item, v.Value) < 0 ? v.Left : v.Right;
            }
            return false;
        }

        public void Clear()
        {
            Initialize();
        }

        public T GetFirst()
        {
            Node v = Root;
            while (v.Left != null) v = v.Left;
            return v.Value;
        }

        public T GetLast()
        {
            Node v = Root;
            while (v.Right != null) v = v.Right;
            return v.Value;
        }

        /// <summary>
        /// 指定のキー以下で最大の要素を始点に要素を列挙します。
        /// </summary>
        /// <param name="key">始点の基準となるキー</param>
        /// <returns><paramref name="key"/>以下の要素を始点とする<see cref="IEnumerable{T}"/></returns>
        public IEnumerable<T> EnumerateFrom(int key)
        {
            if (Count == 0) yield break;

            // 始点を探す
            var stack = new Stack<Node>();
            stack.Push(Root);
            while (stack.Count > 0)
            {
                Node v = stack.Peek();
                if (v == null)
                {
                    stack.Pop();
                    break;
                }
                if (key == Selector(v.Value)) break;
                stack.Push(key < Selector(v.Value) ? v.Left : v.Right);
            }

            // キー以下で最大のものまでさかのぼる
            while (stack.Count > 0)
            {
                Node v = stack.Peek();
                if (key < Selector(v.Value)) stack.Pop();
                else break;
            }

            // スタックが空なら全要素に対してキーが最小値
            if (stack.Count == 0)
            {
                // 全要素を列挙する
                foreach (var item in this)
                    yield return item;
                yield break;
            }

            Node min = stack.Peek();

            // 列挙する
            while (stack.Count > 0)
            {
                Node v = stack.Pop();
                if (Comparer(v.Value, min.Value) < 0) continue;
                yield return v.Value;
                foreach (var item in Enumerate(v.Right))
                    yield return item;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate(Root).GetEnumerator();
        }

        protected IEnumerable<T> Enumerate(Node v)
        {
            if (v == null) yield break;
            if (v.Left != null)
            {
                foreach (var item in Enumerate(v.Left))
                    yield return item;
            }
            yield return v.Value;
            if (v.Right != null)
            {
                foreach (var item in Enumerate(v.Right))
                    yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var list = new List<T>(Count);
            foreach (var item in this) list.Add(item);
            list.CopyTo(array, arrayIndex);
        }

        protected class Node
        {
            public int Height { get; set; }
            public T Value { get; set; }
            public Node Left { get; set; }
            public Node Right { get; set; }

            public Node(T value)
            {
                Value = value;
            }
        }
    }
}
