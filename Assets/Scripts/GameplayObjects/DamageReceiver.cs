using System;
using disguys.Gameplay.GameplayObjects.Character;
using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects
{
    public class DamageReceiver : NetworkBehaviour, IDamageable
    {
        public event Action<ServerCharacter, bool> DamageReceived;

        public event Action<Collision> CollisionEntered;

        [SerializeField]
        NetworkLifeState m_NetworkLifeState;
        
        public void GetHit(ServerCharacter inflicter, bool GetHit)
        {
            if (IsDamageable())
            {
                DamageReceived?.Invoke(inflicter, true);
            }
        }
        
        public IDamageable.SpecialDamageFlags GetSpecialDamageFlags()
        {
            return IDamageable.SpecialDamageFlags.None;
        }

        public bool IsDamageable()
        {
            return m_NetworkLifeState.LifeState.Value == LifeState.Alive;
        }
        
        void OnCollisionEnter(Collision other)
        {
            CollisionEntered?.Invoke(other);
        }
    }

}
