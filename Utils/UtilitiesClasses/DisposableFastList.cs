using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kwytto.Utils
{
    public class DisposableFastList<T> : IEnumerable<T> where T : IDisposable
    {
        public T[] m_buffer;

        public int m_size;

        public T this[int i]
        {
            get
            {
                return m_buffer[i];
            }
            set
            {
                m_buffer[i]?.Dispose();
                m_buffer[i] = value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (m_buffer != null)
            {
                for (int i = 0; i < m_size; i++)
                {
                    yield return m_buffer[i];
                }
            }
        }

        public void Trim()
        {
            SetCapacity(m_size);
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity > 0 && (m_buffer == null || capacity > m_buffer.Length))
            {
                m_size = Mathf.Min(m_size, capacity);
                T[] array = new T[capacity];
                for (int i = 0; i < m_size; i++)
                {
                    array[i] = m_buffer[i];
                }

                m_buffer = array;
            }
        }

        public void SetCapacity(int capacity)
        {
            if (capacity > 0)
            {
                if (m_buffer == null || capacity != m_buffer.Length)
                {
                    m_size = Mathf.Min(m_size, capacity);
                    T[] array = new T[capacity];
                    for (int i = 0; i < m_size; i++)
                    {
                        array[i] = m_buffer[i];
                    }
                    if (m_buffer != null)
                    {
                        for (int i = m_size; i < m_buffer.Length; i++)
                        {
                            m_buffer[i]?.Dispose();
                        }
                    }
                    m_buffer = array;
                }
            }
            else if (m_buffer != null)
            {
                for (int i = 0; i < m_buffer.Length; i++)
                {
                    m_buffer[i]?.Dispose();
                }
                m_buffer = null;
            }
            m_size = Math.Min(m_size, capacity);
        }

        public void Clear()
        {
            m_size = 0;
            if (m_buffer != null)
            {
                for (int i = 0; i < m_buffer.Length; i++)
                {
                    m_buffer[i]?.Dispose();
                }
            }
        }

        public void Release()
        {
            m_size = 0;
            if (m_buffer != null)
            {
                for (int i = 0; i < m_buffer.Length; i++)
                {
                    m_buffer[i]?.Dispose();
                }
            }
            m_buffer = null;
        }

        public void Add(T item)
        {
            if (m_buffer == null || m_size == m_buffer.Length)
            {
                SetCapacity((m_buffer != null) ? Mathf.Max(m_buffer.Length << 1, 32) : 32);
            }

            m_buffer[m_size++] = item;
        }

        public void Remove(T item)
        {
            if (m_buffer == null)
            {
                return;
            }

            EqualityComparer<T> @default = EqualityComparer<T>.Default;
            for (int i = 0; i < m_size; i++)
            {
                if (@default.Equals(m_buffer[i], item))
                {
                    m_size--;
                    for (int j = i; j < m_size; j++)
                    {
                        m_buffer[j] = m_buffer[j + 1];
                    }
                    m_buffer[m_size]?.Dispose();
                    m_buffer[m_size] = default;
                    break;
                }
            }
        }

        public void RemoveAt(int index)
        {
            if (m_buffer != null && index < m_size)
            {
                m_size--;
                for (int i = index; i < m_size; i++)
                {
                    m_buffer[i] = m_buffer[i + 1];
                }
                m_buffer[m_size]?.Dispose();
                m_buffer[m_size] = default(T);
            }
        }

        public T[] ToArray()
        {
            Trim();
            return m_buffer;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
