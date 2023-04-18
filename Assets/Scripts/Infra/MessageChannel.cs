using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace disguys.Infrastructure
{
        public class MessageChannel<T> : IMessageChannel<T>
        {
        readonly List<Action<T>> m_MessageHandlers = new List<Action<T>>();

        /// <summary>
        /// 이 핸들러들을 추가하거나 제거하기 위한 딕셔너리는 구독자 목록을 즉시 수정하는 문제를 방지하기 위해 사용됩니다. 
        ///이는 메시지 핸들러에서 구독을 취소하려는 경우 등이 발생할 수 있습니다.
        /// true 값은 이 핸들러를 추가해야 하고, false 값은 제거해야 함을 나타냅니다.
        /// </summary>
        readonly Dictionary<Action<T>, bool> m_PendingHandlers = new Dictionary<Action<T>, bool>();

        public bool IsDisposed { get; private set; } = false;

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                m_MessageHandlers.Clear();
                m_PendingHandlers.Clear();
            }
        }

        public virtual void Publish(T message)
        {
            foreach (var handler in m_PendingHandlers.Keys)
            {
                if (m_PendingHandlers[handler])
                {
                    m_MessageHandlers.Add(handler);
                }
                else
                {
                    m_MessageHandlers.Remove(handler);
                }
            }
            m_PendingHandlers.Clear();

            foreach (var messageHandler in m_MessageHandlers)
            {
                messageHandler?.Invoke(message);
            }
        }

        public virtual IDisposable Subscribe(Action<T> handler)
        {
            Assert.IsTrue(!IsSubscribed(handler), "Attempting to subscribe with the same handler more than once");

            if (m_PendingHandlers.ContainsKey(handler))
            {
                if (!m_PendingHandlers[handler])
                {
                    m_PendingHandlers.Remove(handler);
                }
            }
            else
            {
                m_PendingHandlers[handler] = true;
            }

            var subscription = new DisposableSubscription<T>(this, handler);
            return subscription;
        }

        public void Unsubscribe(Action<T> handler)
        {
            if (IsSubscribed(handler))
            {
                if (m_PendingHandlers.ContainsKey(handler))
                {
                    if (m_PendingHandlers[handler])
                    {
                        m_PendingHandlers.Remove(handler);
                    }
                }
                else
                {
                    m_PendingHandlers[handler] = false;
                }
            }
        }

        bool IsSubscribed(Action<T> handler)
        {
            var isPendingRemoval = m_PendingHandlers.ContainsKey(handler) && !m_PendingHandlers[handler];
            var isPendingAdding = m_PendingHandlers.ContainsKey(handler) && m_PendingHandlers[handler];
            return m_MessageHandlers.Contains(handler) && !isPendingRemoval || isPendingAdding;
        }
    }
}