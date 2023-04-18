using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace disguys.Utilities
{
    public class SceneLoaderWrapper : NetworkBehaviour
    {
        /// 씬 관리 API를 감싸면서 로딩 화면을 관리합니다. SceneManager를 사용하여 씬을 로드하거나,
        /// 씬 관리가 활성화된 수신 서버의 경우 NetworkSceneManager를 사용하여 씬을 로드하며, 로딩 화면의 시작 및 중지를 처리합니다.
        [SerializeField]
        ClientLoadingScreen m_ClientLoadingScreen;

        [SerializeField]
        LoadingProgressManager m_LoadingProgressManager;

        private bool IsNetworkSceneManagementEnabled => NetworkManager != null && NetworkManager.SceneManager != null &&
                                                        NetworkManager.NetworkConfig.EnableSceneManagement;
        
        public static SceneLoaderWrapper Instance { get; protected set; }

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            DontDestroyOnLoad(this);
        }
        
        public virtual void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            base.OnDestroy();
        }
        
        public override void OnNetworkDespawn()
        {
            if (NetworkManager != null && NetworkManager.SceneManager != null)
            {
                NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
            }
        }
        
        /// <summary>
        /// 이 함수는 scene 이벤트에 대한 콜백을 초기화합니다.
        /// 이 함수는 NetworkManager를 초기화 한 후
        /// (StartHost, StartClient 또는 StartServer 이후) 즉시 호출되어야 합니다.
        /// 이 함수를 호출하지 않으면 scene 이벤트가 올바르게 처리되지 않을 수 있습니다.
        /// </summary>
        public virtual void AddOnSceneEventCallback()
        {
            if (IsNetworkSceneManagementEnabled)
            {
                NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
            }
        }
        
        /// <summary>
        /// 이 함수는 지정된 loadSceneMode를 사용하여 비동기적으로 씬을 로드합니다. 만약 SceneManagement가 활성화된 듣기 서버인 경우 NetworkSceneManager를 사용하고,
        /// 그렇지 않은 경우 SceneManager를 사용합니다. SceneManager를 통해 씬이 로드되면 이 메서드는 로딩 화면의 시작도 트리거합니다.

        ///매개변수로는 로드할 씬의 이름 또는 경로인 sceneName, NetworkSceneManager를 사용할 것인지 아니면 SceneManager를 사용할 것인지를 결정하는 useNetworkSceneManager,
        ///LoadSceneMode.Single이면 로드하기 전에 현재 씬이 모두 언로드되는 loadSceneMode가 있습니다.
        public virtual void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            if (useNetworkSceneManager)
            {
                if (IsSpawned && IsNetworkSceneManagementEnabled && !NetworkManager.ShutdownInProgress)
                {
                    if (NetworkManager.IsServer)
                    {
                        // 이 코드는, 현재 액티브한 서버이고, NetworkManager가 scene management를 사용하는 경우, NetworkManager의 SceneManager를 사용하여 씬을 로드합니다.
                        // SceneManager는 Unity의 씬 관리 도구로, 씬 로드/언로드를 담당합니다.
                        // LoadSceneMode.Single이면, 로드하기 전에 현재 씬이 모두 언로드됩니다. 따라서 이 메서드는 로딩 화면을 시작합니다.
                        NetworkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                    }
                }
            }
            else
            {
                // Load using SceneManager
                var loadOperation = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                if (loadSceneMode == LoadSceneMode.Single)
                {
                    m_ClientLoadingScreen.StartLoadingScreen(sceneName);
                    m_LoadingProgressManager.LocalLoadOperation = loadOperation;
                }
            }
        }
        
        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!IsSpawned || NetworkManager.ShutdownInProgress)
            {
                m_ClientLoadingScreen.StopLoadingScreen();
            }
        }
        
         void OnSceneEvent(SceneEvent sceneEvent)
        {
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.Load: // Server told client to load a scene
                    // Only executes on client
                    if (NetworkManager.IsClient)
                    {
                        // Only start a new loading screen if scene loaded in Single mode, else simply update
                        if (sceneEvent.LoadSceneMode == LoadSceneMode.Single)
                        {
                            m_ClientLoadingScreen.StartLoadingScreen(sceneEvent.SceneName);
                            m_LoadingProgressManager.LocalLoadOperation = sceneEvent.AsyncOperation;
                        }
                        else
                        {
                            m_ClientLoadingScreen.UpdateLoadingScreen(sceneEvent.SceneName);
                            m_LoadingProgressManager.LocalLoadOperation = sceneEvent.AsyncOperation;
                        }
                    }
                    break;
                case SceneEventType.LoadEventCompleted: // Server told client that all clients finished loading a scene
                    // Only executes on client
                    if (NetworkManager.IsClient)
                    {
                        m_ClientLoadingScreen.StopLoadingScreen();
                        m_LoadingProgressManager.ResetLocalProgress();
                    }
                    break;
                case SceneEventType.Synchronize: // Server told client to start synchronizing scenes
                {
                    // todo: this is a workaround that could be removed once MTT-3363 is done
                    // Only executes on client that is not the host
                    if (NetworkManager.IsClient && !NetworkManager.IsHost)
                    {
                        // unload all currently loaded additive scenes so that if we connect to a server with the same
                        // main scene we properly load and synchronize all appropriate scenes without loading a scene
                        // that is already loaded.
                        UnloadAdditiveScenes();
                    }
                    break;
                }
                case SceneEventType.SynchronizeComplete: // Client told server that they finished synchronizing
                    // Only executes on server
                    if (NetworkManager.IsServer)
                    {
                        // Send client RPC to make sure the client stops the loading screen after the server handles what it needs to after the client finished synchronizing, for example character spawning done server side should still be hidden by loading screen.
                        StopLoadingScreenClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { sceneEvent.ClientId } } });
                    }
                    break;
            }
        }
         
         void UnloadAdditiveScenes()
         {
             var activeScene = SceneManager.GetActiveScene();
             for (var i = 0; i < SceneManager.sceneCount; i++)
             {
                 var scene = SceneManager.GetSceneAt(i);
                 if (scene.isLoaded && scene != activeScene)
                 {
                     SceneManager.UnloadSceneAsync(scene);
                 }
             }
         }

         [ClientRpc]
         void StopLoadingScreenClientRpc(ClientRpcParams clientRpcParams = default)
         {
             m_ClientLoadingScreen.StopLoadingScreen();
         }
    }
}

