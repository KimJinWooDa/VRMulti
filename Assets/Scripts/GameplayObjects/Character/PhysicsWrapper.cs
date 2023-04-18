using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace disguys.Gameplay.GameplayObjects.Character
{
    /// <summary>
    /// 이 클래스는 물리학과 관련된 컴포넌트에 대한 직접적인 참조를 감싸는 래퍼 클래스입니다.
    /// PhysicsWrapper의 각 인스턴스는 NetworkObject의 ID로 색인화된 정적 사전에 등록됩니다.

    /// PC와 NPC의 루트 GameObject는 세계를 통해 이동하는 객체가 아니므로,
    /// 다른 클래스들은 PC/NPC의 게임 내 위치에 대한 빠른 참조가 필요합니다.
    ///
    /// 이것은 게임에서 PC와 NPC를 나타내는 GameObject는 실제로 게임 내에서 움직이는 객체가 아니기 때문에,
    /// 이동과 관련된 물리학적인 계산이 필요할 때에는 PC와 NPC의 다른 GameObject나 컴포넌트를 사용해야 하는데,
    /// 이 때문에 다른 클래스에서 빠른 참조를 얻을 수 있는 래퍼 클래스(PhysicsWrapper)가 필요합니다.
    /// 이 래퍼 클래스는 NetworkObject의 ID에 의해 인덱싱되는 정적 딕셔너리에 등록되며,
    /// 이를 통해 빠른 참조를 얻을 수 있습니다.
    /// 이 클래스는 PC와 NPC의 실제 위치나 이동 상태 등을 쉽게 액세스할 수 있도록 도와줍니다.
    /// </summary>
    public class PhysicsWrapper : NetworkBehaviour
    {
        private static Dictionary<ulong, PhysicsWrapper> m_PhysicsWrappers = new Dictionary<ulong, PhysicsWrapper>();

        [SerializeField] private Transform m_Transform;
        
        public Transform Transform => m_Transform;

        private Rigidbody m_Rigidbody;
        public Rigidbody Rigidbody => m_Rigidbody;

        [SerializeField]
        Collider m_ContactCollider;

        public Collider DamageCollider => m_ContactCollider;

        ulong m_NetworkObjectID;
        
        public override void OnNetworkSpawn()
        {
            m_PhysicsWrappers.Add(NetworkObjectId, this);

            m_NetworkObjectID = NetworkObjectId;
        }
        
        public override void OnNetworkDespawn()
        {
            RemovePhysicsWrapper();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            RemovePhysicsWrapper();
        }

        void RemovePhysicsWrapper()
        {
            m_PhysicsWrappers.Remove(m_NetworkObjectID);
        }
        
        public static bool TryGetPhysicsWrapper(ulong networkObjectID, out PhysicsWrapper physicsWrapper)
        {
            return m_PhysicsWrappers.TryGetValue(networkObjectID, out physicsWrapper);
        }
    }
}

