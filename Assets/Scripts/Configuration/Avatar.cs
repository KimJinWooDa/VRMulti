using System;
using disguys.Infrastructure;
using UnityEngine;

namespace disguys.Gameplay.Configuration
{
    /// <summary>
    /// 이 ScriptableObject는 플레이어 캐릭터를 정의합니다.
    /// 캐릭터 클래스 필드를 정의하여 관련된 게임 특정 속성과 그래픽 표현을 정의합니다.
    /// </summary>
    [CreateAssetMenu]
    [Serializable]
    public class Avatar : GuidScriptableObject
    {
        public GameObject Graphics;
    }
}

