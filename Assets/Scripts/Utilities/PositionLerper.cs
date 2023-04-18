using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace disguys.Utilities
{
    /// <summary>
    /// 이것은 두 개의 Vector3 값 사이에서 선형 보간(linear interpolation)을 수행하는
    /// 유틸리티 구조체입니다. 현재(current)와 목표(target)가 시간에 따라 변경되는
    /// 유연한 선형 보간을 가능하게 합니다. 보간값은 t(0과 1 사이의 값)로 결정되며,
    /// t가 0일 때는 현재값이 반환되고, t가 1일 때는 목표값이 반환됩니다.
    /// t가 0과 1 사이인 경우에는 현재값과 목표값 사이에서 보간값이 계산됩니다.
    /// </summary>
    public struct PositionLerper
    {
        //가장 최근 보간의 시작점을 계산합니다.
        Vector3 m_LerpStart;
        
        // 가장 최근 보간에 대한 경과 시간을 계산한 것을 의미합니다.
        // 이 값은 일반적으로 보간이 업데이트될 때마다 계산되며,
        // 현재 시간과 보간이 시작된 시간을 비교하여 얻어집니다.
        // 보간이 업데이트될 때마다 경과 시간을 계산하는 이유는
        // 보간이 지속되는 시간이 계속해서 변하기 때문입니다.
        float m_CurrentLerpTime;

        // 선형 보간(interpolation)의 지속 시간을 초 단위로 나타내는 값입니다.
        // 이 값은 두 점 간의 거리와 이동 속도에 따라 결정됩니다.
        // 일반적으로 이 값은 보간을 시작할 때 계산됩니다.
        float m_LerpTime;

        public PositionLerper(Vector3 start, float lerpTime)
        {
            m_LerpStart = start;
            m_CurrentLerpTime = 0f;
            m_LerpTime = lerpTime;
        }
        
        
        /// <summary>
        /// 두 개의 Vector3 값을 선형 보간합니다.
        /// </summary>
        /// <param name="current"> 보간의 시작 값. </param>
        /// <param name="target"> 보간의 끝 값. </param>
        /// <returns> current와 target 사이의 Vector3 값. </returns>
        public Vector3 LerpPosition(Vector3 current, Vector3 target)
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

            return Vector3.Lerp(m_LerpStart, target, lerpPercentage);
        }
    }

}
