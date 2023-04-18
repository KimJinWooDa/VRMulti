using System;

namespace disguys.Infrastructure
{
    /// <summary>
    /// 이 클래스는 활성화된 Message Channel 구독을 제어하는 핸들(handle)로,
    /// dispose되면 해당 채널에서 구독을 취소합니다. 제네릭 타입 T를 사용합니다.
    public class DisposableSubscription<T> : IDisposable
    {
        Action<T> m_Handler;
        bool m_IsDisposed;
        IMessageChannel<T> m_MessageChannel;

        public DisposableSubscription(IMessageChannel<T> messageChannel, Action<T> handler)
        {
            m_MessageChannel = messageChannel;
            m_Handler = handler;
        }

        public void Dispose()
        {
            if (!m_IsDisposed)
            {
                m_IsDisposed = true;

                if (!m_MessageChannel.IsDisposed)
                {
                    m_MessageChannel.Unsubscribe(m_Handler);
                }

                m_Handler = null;
                m_MessageChannel = null;
            }
        }
    }
}