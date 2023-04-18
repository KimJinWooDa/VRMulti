using System;
using UnityEngine;

namespace disguys.Gameplay.Configuration
{
    /// <summary>
    /// 이 ScriptableObject은 사용 가능한 아바타들을 포함하는 컨테이너 역할을 합니다.
    /// </summary>
    [CreateAssetMenu]
    public class AvatarRegistry : ScriptableObject
    {
        [SerializeField]
        Avatar[] m_Avatars;

        public bool TryGetAvatar(Guid guid, out Avatar avatarValue) 
        {
            avatarValue = Array.Find(m_Avatars, avatar => avatar.Guid == guid);
            
            return avatarValue != null;
        }

        public Avatar ReturnAvatar(Guid guid)
        {
            return Array.Find(m_Avatars, avatar => avatar.Guid == guid);
        }

        public Avatar GetRandomAvatar()
        {
            if (m_Avatars == null || m_Avatars.Length == 0)
            {
                return null;
            }
            return m_Avatars[UnityEngine.Random.Range(0, m_Avatars.Length)];
        }
    }

}
