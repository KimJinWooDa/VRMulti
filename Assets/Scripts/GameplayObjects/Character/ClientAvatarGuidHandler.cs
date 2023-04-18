using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects.Character
{
    /// <summary>
    /// 해당 컴포넌트는 클라이언트에서 실행되며, Avatar의 Guid 상태 변경을 대기하고 해당 Guid에 매칭되는 Avatar를 AvatarRegistry에서 가져와 Graphics GameObject를 생성합니다.
    /// 즉, Avatar의 Guid 값이 변경되면 해당 Avatar를 가져와서 화면에 보여주는 역할을 합니다.
    /// </summary>
    
    [RequireComponent(typeof(NetworkAvatarGuidState))]
    public class ClientAvatarGuidHandler : NetworkBehaviour
    {
        [SerializeField]
        Animator m_GraphicsAnimator;

        [SerializeField]
        NetworkAvatarGuidState m_NetworkAvatarGuidState;

        public Animator graphicsAnimator => m_GraphicsAnimator;

        public event Action<GameObject> AvatarGraphicsSpawned;

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                InstantiateAvatar();
            }
        }

        void InstantiateAvatar()
        {
            if (m_GraphicsAnimator.transform.childCount > 0)
            {
                // 이미 해당 값에 대한 콜백을 받았는지 확인하고
                // 이전에 생성된 GameObject가 있으면 새로운 것을 생성하지 않도록 하는 것
                return;
            }
            Instantiate(m_NetworkAvatarGuidState.RegisteredAvatar.Graphics, m_GraphicsAnimator.transform);

            AvatarGraphicsSpawned?.Invoke(m_GraphicsAnimator.gameObject);
        }

    }
    
    
}

