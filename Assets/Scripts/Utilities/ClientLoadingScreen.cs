using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace disguys.Utilities
{
    public class ClientLoadingScreen : MonoBehaviour
    {
        /// 이 스크립트는 진행 표시 막대와 로드된 씬의 이름을 로딩 화면에 표시합니다.
        /// 이 스크립트는 외부에서 시작 및 중지해야 합니다.
        /// 또한 로딩 화면이 중지되기 전에 새로운 로드 작업이 시작되면, 로딩 화면을 업데이트할 수 있습니다.

        protected class LoadingProgressBar
        {
            public Slider ProgressBar { get; set; }
            public Text NameText { get; set; }

            public LoadingProgressBar(Slider otherPlayerProgressBar, Text otherPlayerNameText)
            {
                ProgressBar = otherPlayerProgressBar;
                NameText = otherPlayerNameText;
            }

            public void UpdateProgress(float value, float newValue)
            {
                ProgressBar.value = newValue;
            }
        }

        [SerializeField] private CanvasGroup m_CanvasGroup;

        [SerializeField] float m_DelayBeforeFadeOut = 0.5f;

        [SerializeField] float m_FadeOutDuration = 0.1f;

        [SerializeField] Slider m_ProgressBar;

        [SerializeField] Text m_SceneName;

        [SerializeField] List<Slider> m_OtherPlayersProgressBars;

        [SerializeField] List<Text> m_OtherPlayerNamesTexts;

        [SerializeField] protected LoadingProgressManager m_LoadingProgressManager;

        protected Dictionary<ulong, LoadingProgressBar> m_LoadingProgressBars =
            new Dictionary<ulong, LoadingProgressBar>();

        bool m_LoadingScreenRunning;

        Coroutine m_FadeOutCoroutine;

        void Awake()
        {
            DontDestroyOnLoad(this);
            Assert.AreEqual(m_OtherPlayersProgressBars.Count, m_OtherPlayerNamesTexts.Count,
                "There should be the same number of progress bars and name labels");
        }

        void Start()
        {
            SetCanvasVisibility(false);
            m_LoadingProgressManager.onTrackersUpdated += OnProgressTrackersUpdated;
        }

        void OnDestroy()
        {
            m_LoadingProgressManager.onTrackersUpdated -= OnProgressTrackersUpdated;
        }

        void Update()
        {
            if (m_LoadingScreenRunning)
            {
                m_ProgressBar.value = m_LoadingProgressManager.LocalProgress;
            }
        }

        void OnProgressTrackersUpdated()
        {
            // deactivate progress bars of clients that are no longer tracked
            var clientIdsToRemove = new List<ulong>();
            foreach (var clientId in m_LoadingProgressBars.Keys)
            {
                if (!m_LoadingProgressManager.ProgressTrackers.ContainsKey(clientId))
                {
                    clientIdsToRemove.Add(clientId);
                }
            }

            foreach (var clientId in clientIdsToRemove)
            {
                RemoveOtherPlayerProgressBar(clientId);
            }

            // Add progress bars for clients that are now tracked
            foreach (var progressTracker in m_LoadingProgressManager.ProgressTrackers)
            {
                var clientId = progressTracker.Key;
                if (clientId != NetworkManager.Singleton.LocalClientId &&
                    !m_LoadingProgressBars.ContainsKey(clientId))
                {
                    AddOtherPlayerProgressBar(clientId, progressTracker.Value);
                }
            }
        }

        public void StopLoadingScreen()
        {
            if (m_LoadingScreenRunning)
            {
                if (m_FadeOutCoroutine != null)
                {
                    StopCoroutine(m_FadeOutCoroutine);
                }

                m_FadeOutCoroutine = StartCoroutine(FadeOutCoroutine());
            }
        }

        public void StartLoadingScreen(string sceneName)
        {
            SetCanvasVisibility(true);
            m_LoadingScreenRunning = true;
            UpdateLoadingScreen(sceneName);
            ReinitializeProgressBars();
        }

        void ReinitializeProgressBars()
        {
            // deactivate progress bars of clients that are no longer tracked
            var clientIdsToRemove = new List<ulong>();
            foreach (var clientId in m_LoadingProgressBars.Keys)
            {
                if (!m_LoadingProgressManager.ProgressTrackers.ContainsKey(clientId))
                {
                    clientIdsToRemove.Add(clientId);
                }
            }

            foreach (var clientId in clientIdsToRemove)
            {
                RemoveOtherPlayerProgressBar(clientId);
            }

            for (var i = 0; i < m_OtherPlayersProgressBars.Count; i++)
            {
                m_OtherPlayersProgressBars[i].gameObject.SetActive(false);
                m_OtherPlayerNamesTexts[i].gameObject.SetActive(false);
            }

            var index = 0;

            foreach (var progressTracker in m_LoadingProgressManager.ProgressTrackers)
            {
                var clientId = progressTracker.Key;
                if (clientId != NetworkManager.Singleton.LocalClientId)
                {
                    UpdateOtherPlayerProgressBar(clientId, index++);
                }
            }
        }

        protected virtual void UpdateOtherPlayerProgressBar(ulong clientId, int progressBarIndex)
        {
            m_LoadingProgressBars[clientId].ProgressBar = m_OtherPlayersProgressBars[progressBarIndex];
            m_LoadingProgressBars[clientId].ProgressBar.gameObject.SetActive(true);
            m_LoadingProgressBars[clientId].NameText = m_OtherPlayerNamesTexts[progressBarIndex];
            m_LoadingProgressBars[clientId].NameText.gameObject.SetActive(true);
        }

        protected virtual void AddOtherPlayerProgressBar(ulong clientId,
            NetworkedLoadingProgressTracker progressTracker)
        {
            if (m_LoadingProgressBars.Count < m_OtherPlayersProgressBars.Count &&
                m_LoadingProgressBars.Count < m_OtherPlayerNamesTexts.Count)
            {
                var index = m_LoadingProgressBars.Count;
                m_LoadingProgressBars[clientId] = new LoadingProgressBar(m_OtherPlayersProgressBars[index],
                    m_OtherPlayerNamesTexts[index]);
                progressTracker.Progress.OnValueChanged += m_LoadingProgressBars[clientId].UpdateProgress;
                m_LoadingProgressBars[clientId].ProgressBar.value = progressTracker.Progress.Value;
                m_LoadingProgressBars[clientId].ProgressBar.gameObject.SetActive(true);
                m_LoadingProgressBars[clientId].NameText.gameObject.SetActive(true);
                m_LoadingProgressBars[clientId].NameText.text = $"Client {clientId}";
            }
            else
            {
                throw new Exception("There are not enough progress bars to track the progress of all the players.");
            }
        }

        void RemoveOtherPlayerProgressBar(ulong clientId, NetworkedLoadingProgressTracker progressTracker = null)
        {
            if (progressTracker != null)
            {
                progressTracker.Progress.OnValueChanged -= m_LoadingProgressBars[clientId].UpdateProgress;
            }

            m_LoadingProgressBars[clientId].ProgressBar.gameObject.SetActive(false);
            m_LoadingProgressBars[clientId].NameText.gameObject.SetActive(false);
            m_LoadingProgressBars.Remove(clientId);
        }

        public void UpdateLoadingScreen(string sceneName)
        {
            if (m_LoadingScreenRunning)
            {
                m_SceneName.text = sceneName;
                if (m_FadeOutCoroutine != null)
                {
                    StopCoroutine(m_FadeOutCoroutine);
                }
            }
        }

        void SetCanvasVisibility(bool visible)
        {
            m_CanvasGroup.alpha = visible ? 1 : 0;
            m_CanvasGroup.blocksRaycasts = visible;
        }

        IEnumerator FadeOutCoroutine()
        {
            yield return new WaitForSeconds(m_DelayBeforeFadeOut);
            m_LoadingScreenRunning = false;

            float currentTime = 0;
            while (currentTime < m_FadeOutDuration)
            {
                m_CanvasGroup.alpha = Mathf.Lerp(1, 0, currentTime / m_FadeOutDuration);
                yield return null;
                currentTime += Time.deltaTime;
            }

            SetCanvasVisibility(false);
        }
    }
}



