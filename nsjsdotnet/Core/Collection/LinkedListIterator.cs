namespace nsjsdotnet.Core.Collection
{
    using System;
    using System.Collections.Generic;

    public class LinkedListIterator<T>
    {
        private LinkedListNode<T> current; // 当前节点
        private LinkedList<T> list; // 链首指针
        private object cp; // 临界点
        private bool MoveNext()
        {
            lock (cp)
            {
                if (current == null)
                {
                    current = list.First;
                }
                else
                {
                    current = current.Next;
                }
                return current != null;
            }
        }

        private bool MovePrevious()
        {
            lock (cp)
            {
                if (current == null)
                {
                    current = list.Last;
                }
                else
                {
                    current = current.Previous;
                }
                return current != null;
            }
        }

        public void Reset()
        {
            lock (cp)
            {
                current = list.First;
            }
        }

        public LinkedListNode<T> Node
        {
            get // 线程安全与增删节安全
            {
                lock (cp)
                {
                    return current;
                }
            }
        }

        public T Value
        {
            get
            {
                lock (cp)
                {
                    LinkedListNode<T> node = this.Node;
                    if (node == null)
                        return default(T);
                    return node.Value;
                }
            }
        }

        public bool Remove(LinkedListNode<T> node)
        {
            lock (cp)
            {
                if (node == null)
                {
                    return false;
                }
                if (current == node)
                {
                    current = current.Next;
                }
                return current != null;
            }
        }

        public LinkedListIterator(object cp, LinkedList<T> list)
        {
            if (cp == null || list == null)
            {
                throw new ArgumentNullException();
            }
            this.cp = cp;
            this.list = list;
        }

        public static LinkedListIterator<T> operator ++(LinkedListIterator<T> iterator) // 移动指针到下一个节点
        {
            if (iterator != null)
            {
                lock (iterator.cp)
                {
                    if (!iterator.MoveNext())
                    {
                        iterator.MoveNext();
                    }
                }
            }
            return iterator;
        }

        public static LinkedListIterator<T> operator --(LinkedListIterator<T> iterator) // 移动指针到上一个节点
        {
            if (iterator != null)
            {
                lock (iterator.cp)
                {
                    if (!iterator.MovePrevious())
                    {
                        iterator.MovePrevious();
                    }
                }
            }
            return iterator;
        }
    }
}
