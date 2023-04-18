using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace disguys.Utilities
{
    public class LoadingProgressManager : NetworkBehaviour
    {
        [SerializeField]
        GameObject m_ProgressTrackerPrefab;

        
        //각 클라이언트의 loading progress를 추적함, Key는 ClientID
        public Dictionary<ulong, NetworkedLoadingProgressTracker> ProgressTrackers { get; } =
            new Dictionary<ulong, NetworkedLoadingProgressTracker>();

        /// 현재 로드 작업의 AsyncOperation입니다. 이 속성은 새로운 로드 작업이 시작될 때마다 설정해야 합니다
        public AsyncOperation LocalLoadOperation
        {
            set
            {
                LocalProgress = 0;
                m_LocalLoadOperation = value;
            }
        }

        private AsyncOperation m_LocalLoadOperation;
        
        float m_LocalProgress;

        /// 로컬 클라이언트의 현재 로딩 진행 상황입니다. 네트워크 세션이 아닌 경우 로컬 필드에 의해 처리되며,
        /// 딕셔너리(ProgressTrackers)의 진행 추적기에 의해 처리됩니다.

        public event Action onTrackersUpdated;
        public float LocalProgress
        {
            get => IsSpawned && ProgressTrackers.ContainsKey(NetworkManager.LocalClientId)
                ? ProgressTrackers[NetworkManager.LocalClientId].Progress.Value
                : m_LocalProgress;
            private set
            {
                if (IsSpawned && ProgressTrackers.ContainsKey(NetworkManager.LocalClientId))
                {
                    ProgressTrackers[NetworkManager.LocalClientId].Progress.Value = value;
                }
                else
                {
                    m_LocalProgress = value;
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback += AddTracker;
                NetworkManager.OnClientDisconnectCallback += RemoveTracker;
                AddTracker(NetworkManager.LocalClientId);
            }
        }
        
        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback -= AddTracker;
                NetworkManager.OnClientDisconnectCallback -= RemoveTracker;
            }
            ProgressTrackers.Clear();
            onTrackersUpdated?.Invoke();
        }

        private void Update()
        {
            if (m_LocalLoadOperation != null)
            {
                LocalProgress = m_LocalLoadOperation.isDone ? 1 : m_LocalLoadOperation.progress;
            }
        }

        [ClientRpc]
        void UpdateTrackersClientRpc()
        {
            if (!IsHost)
            {
                ProgressTrackers.Clear();
                //모든 클라이언트의 진행도를 tracking함
                foreach (var tracker in FindObjectsOfType<NetworkedLoadingProgressTracker>())
                {
                    //tracker가 스폰되어있다면
                    if (tracker.IsSpawned)
                    {
                        //tracker의 소유자에게 tracker를 할당시킴
                        ProgressTrackers[tracker.OwnerClientId] = tracker;
                        if (tracker.OwnerClientId == NetworkManager.LocalClientId)
                        {
                            //로컬 진행도와 remote진행도를 비교해서 큰 값을 LocalProgress에 집어 넣음
                            LocalProgress = Mathf.Max(m_LocalProgress, LocalProgress);
                        }
                    }
                }
            }
            onTrackersUpdated?.Invoke();
        }
        
        
        void AddTracker(ulong clientId)
        {
            if (IsServer)
            {
                var tracker = Instantiate(m_ProgressTrackerPrefab);
                var networkObject = tracker.GetComponent<NetworkObject>();
                networkObject.SpawnWithOwnership(clientId); //양도
                ProgressTrackers[clientId] = tracker.GetComponent<NetworkedLoadingProgressTracker>();
                UpdateTrackersClientRpc();
            }
        }
        
        void RemoveTracker(ulong clientId)
        {
            if (IsServer)
            {
                if (ProgressTrackers.ContainsKey(clientId))
                {
                    var tracker = ProgressTrackers[clientId];
                    ProgressTrackers.Remove(clientId);
                    tracker.NetworkObject.Despawn();
                    UpdateTrackersClientRpc();
                }
            }
        }

        public void ResetLocalProgress()
        {
            LocalProgress = 0;
        }
    }
}

