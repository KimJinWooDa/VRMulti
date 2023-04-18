using System;
using System.Collections;
using disguys.Gameplay.Configuration;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects.Character
{
    public class ServerAnimationHandler : NetworkBehaviour
    {
        [SerializeField] NetworkAnimator m_NetworkAnimator;

        [SerializeField] VisualizationConfiguration m_VisualizationConfiguration;

        [SerializeField] NetworkLifeState m_NetworkLifeState;

        public NetworkAnimator NetworkAnimator => m_NetworkAnimator;

        public VisualizationConfiguration VisualizationConfiguration => m_VisualizationConfiguration;
        
        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                StartCoroutine(WaitToRegisterOnLifeStateChanged());
            }

        }

        IEnumerator WaitToRegisterOnLifeStateChanged()
        {
            yield return new WaitForEndOfFrame();
            m_NetworkLifeState.LifeState.OnValueChanged += OnLifeStateChanged;
            if (m_NetworkLifeState.LifeState.Value != LifeState.Alive)
            {
                OnLifeStateChanged(LifeState.Alive, m_NetworkLifeState.LifeState.Value);
            }
        }

        void OnLifeStateChanged(LifeState previousValue, LifeState newValue)
        {
            switch (newValue)
            {
                case LifeState.Dead:
                    NetworkAnimator.SetTrigger(VisualizationConfiguration.DeadStateTriggerID);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null);
            }
        }
        
        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                m_NetworkLifeState.LifeState.OnValueChanged -= OnLifeStateChanged;
            }
        }
    }
}

