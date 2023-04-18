using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace disguys.Gameplay.GameplayObjects
{
    /// <summary>
    /// Skills이 대상으로하는 Entity는
    /// 해당 Entity의 공유 NetworkState 구성 요소가 이 인터페이스를 구현해야합니다.
    /// </summary>
    public interface ITargetable
    {
        /// <summary>
        /// 이 타겟팅 가능한 개체는 NPC인지 PC인지를 나타내는지 여부를 확인합니다.
        /// </summary>
        bool IsNpc { get; }

        /// <summary>
        /// 현재 대상이 유효한지 여부를 나타냅니다.
        /// </summary>
        bool IsValidTarget { get; }
    }
}
