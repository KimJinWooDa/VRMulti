using System;
using disguys.Gameplay.Configuration;
using disguys.Infrastructure;
using Unity.Netcode;
using UnityEngine;
using Avatar = disguys.Gameplay.Configuration.Avatar;

namespace disguys.Gameplay.GameplayObjects.Character
{
    //서버에서 클라이언트로 Avatar GUID를 보내고 받기 위한 NetworkBehaviour 구성 요소입니다
    public class NetworkAvatarGuidState : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<NetworkGuid> AvatarGuid = new NetworkVariable<NetworkGuid>();

        [SerializeField]
        AvatarRegistry m_AvatarRegistry;

        Avatar m_Avatar;

        public Avatar RegisteredAvatar
        {
            get
            {
                if (m_Avatar == null)
                {
                    RegisterAvatar(AvatarGuid.Value.ToGuid()); //NetworkGuid -> Guid로 변환
                }
                
                return m_Avatar;
            }
        }
        
        public void SetRandomAvatar()
        {
            // 레지스터에 등록되어 있는 캐릭터 하나 가져온 뒤 Guid를 NetworkGuid로 변환해준다음 NetworkAvatarGuid로 저장해줌
            var value =  m_AvatarRegistry.GetRandomAvatar().Guid.ToNetworkGuid();
            AvatarGuid.Value = value;
        }

        void RegisterAvatar(Guid guid)
        {
            if (guid.Equals(Guid.Empty))
            {
                // not a valid Guid
                return;
            }

            // 현재 저장되어 있는 Guid에 기반하여, AvatarRegistry에서 Avatar가 검색됩니다.
            // 예를 들어, A라는 캐릭터가 SetRandomAvatar에서 저장되어졌으면 지금 NetworkAvatar의 guid는 A라는 캐릭터의 guid가 NetworkGuid로 변환되어 있는 상태임
            // 따라서, RegisterAvatar를 호출할 땐 NetworkGuid로 되어 있는것을 Guid로 바꿔서 등록되어 있는 A라는 캐릭터를 Register해주어야함
            
            // 즉, 플레이어가 캐릭터를 바꾸고 싶을 때는 SetAvatarNetworkGuid를 통해 A라는 캐릭터에서 B라는 캐릭터의 Guid를 등록시켜두고
            // m_Avatar = out var avatar를 등록시켜두면 됨
            if (!m_AvatarRegistry.TryGetAvatar(guid, out var avatar))
            {
                Debug.LogError("Avatar not found!");
                return;
            }

            m_Avatar = avatar;
        }
    }
}
