using System;
using System.Collections;
using System.Collections.Generic;
using disguys.Gameplay.GameplayObjects.Character;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects
{
    /// 이 함수는 체력 피해 또는 회복을 받을 때 호출됩니다.
    /// 이 게임에선 공격당할때 뿐
    public interface IDamageable
    {
        void GetHit(ServerCharacter inflicter, bool GetHit);
        
        ulong NetworkObjectId { get; }

        Transform transform { get; }

        [Flags]
        public enum SpecialDamageFlags
        {
            None = 0,
            UnusedFlag = 1 << 0, // does nothing; see comments below
            StunOnTrample = 1 << 1,
            NotDamagedByPlayers = 1 << 2,
        }
        
        SpecialDamageFlags GetSpecialDamageFlags();
        
        /// <summary>
        /// 이 함수는 캐릭터가 아직 피해를 받을 수 있는 상태인지 여부를 반환합니다.
        /// 만약 캐릭터가 이미 파괴되었거나 죽은 상태라면 false를 반환해야 합니다.
        /// </summary>
        bool IsDamageable();
    }
}

