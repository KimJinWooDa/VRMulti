using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace disguys.Utilities
{
    public struct RotationLerper
    {
        /// <summary>
        ///이것은 두 개의 Quaternion 값을 선형 보간하는 유틸리티 구조체입니다.
        /// 현재(current)와 대상(target)이 시간이 지남에 따라 변경되는 유연한 선형 보간을 허용합니다.
        
        Quaternion m_LerpStart;

        float m_CurrentLerpTime;


        float m_LerpTime;

        public RotationLerper(Quaternion start, float lerpTime)
        {
            m_LerpStart = start;
            m_CurrentLerpTime = 0f;
            m_LerpTime = lerpTime;
        }

        public Quaternion LerpRotation(Quaternion current, Quaternion target)
        {
            if (current != target)
            {
                m_LerpStart = current;
                m_CurrentLerpTime = 0f;
            }

            m_CurrentLerpTime += Time.deltaTime;
            if (m_CurrentLerpTime > m_LerpTime)
            {
                m_CurrentLerpTime = m_LerpTime;
            }

            var lerpPercentage = m_CurrentLerpTime / m_LerpTime;

            return Quaternion.Slerp(m_LerpStart, target, lerpPercentage);
        }
    }
}

