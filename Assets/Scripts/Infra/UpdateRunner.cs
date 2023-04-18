using System;
using System.Collections.Generic;
using UnityEngine;

namespace disguys.Infrastructure
{
    /// <summary>
    /// 이것은 일반적인 MonoBehaviour의 Update보다 느린 업데이트 루프에서 실행하려는
    /// 몇 가지 객체들이 있을 수 있으며, 그것들은 정확한 타이밍 없이 서비스에서 데이터를 업데이트하려는 경우가
    /// 있을 수 있습니다. 또한 Unity 객체에 결합되지 않으면서도 업데이트 루프가 필요한 객체들도 있을 수 있습니다.
    /// 이 클래스는 그러한 경우를 위한 추상화 레이어를 제공합니다.
    public class UpdateRunner : MonoBehaviour
    {
        class SubscriberData
        {
            public float Period;
            public float NextCallTime;
        }

        readonly Queue<Action> m_PendingHandlers = new Queue<Action>();
        readonly HashSet<Action<float>> m_Subscribers = new HashSet<Action<float>>();
        readonly Dictionary<Action<float>, SubscriberData> m_SubscriberData = new Dictionary<Action<float>, SubscriberData>();

        public void OnDestroy()
        {
            m_PendingHandlers.Clear();
            m_Subscribers.Clear();
            m_SubscriberData.Clear();
        }

        /// <summary>
        /// Subscribe in order to have onUpdate called approximately every period seconds (or every frame, if period <= 0).
        /// Don't assume that onUpdate will be called in any particular order compared to other subscribers.
        /// </summary>
        public void Subscribe(Action<float> onUpdate, float updatePeriod)
        {
            if (onUpdate == null)
            {
                return;
            }

            if (onUpdate.Target == null) // Detect a local function that cannot be Unsubscribed since it could go out of scope.
            {
                Debug.LogError("Can't subscribe to a local function that can go out of scope and can't be unsubscribed from");
                return;
            }

            if (onUpdate.Method.ToString().Contains("<")) // Detect
            {
                Debug.LogError("Can't subscribe with an anonymous function that cannot be Unsubscribed, by checking for a character that can't exist in a declared method name.");
                return;
            }

            if (!m_Subscribers.Contains(onUpdate))
            {
                m_PendingHandlers.Enqueue(() =>
                {
                    if (m_Subscribers.Add(onUpdate))
                    {
                        m_SubscriberData.Add(onUpdate, new SubscriberData() { Period = updatePeriod, NextCallTime = 0 });
                    }
                });
            }
        }

        /// <summary>
        /// Safe to call even if onUpdate was not previously Subscribed.
        /// </summary>
        public void Unsubscribe(Action<float> onUpdate)
        {
            m_PendingHandlers.Enqueue(() =>
            {
                m_Subscribers.Remove(onUpdate);
                m_SubscriberData.Remove(onUpdate);
            });
        }

        /// <summary>
        /// Each frame, advance all subscribers. Any that have hit their period should then act, though if they take too long they could be removed.
        /// </summary>
        void Update()
        {
            while (m_PendingHandlers.Count > 0)
            {
                m_PendingHandlers.Dequeue()?.Invoke();
            }

            foreach (var subscriber in m_Subscribers)
            {
                var subscriberData = m_SubscriberData[subscriber];

                if (Time.time >= subscriberData.NextCallTime)
                {
                    subscriber.Invoke(Time.deltaTime);
                    subscriberData.NextCallTime = Time.time + subscriberData.Period;
                }
            }
        }
    }
}