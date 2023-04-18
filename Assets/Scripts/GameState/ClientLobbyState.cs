using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.GameState
{
    public class ClientLobbyState : GameStateBehaviour
    {
        public override GameState ActiveState
        {
            get { return GameState.Lobby; }
        }

        [SerializeField] private AudioSource m_AudioSource;

        [SerializeField] private AudioClip m_ClientConnectedSound;
        
        private void Awake()
        {
            base.Awake();
        }

        [ClientRpc]
        public void OnClientConnectedClientRpc()
        {
            m_AudioSource.PlayOneShot(m_ClientConnectedSound);
        }

    }
}