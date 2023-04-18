using System;
using System.Collections;
using disguys.Auth;
using disguys.ConnectionManagement;
using disguys.Gameplay.Configuration;
using disguys.Gameplay.UI;
using disguys.Looby;
using disguys.Utilities;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace disguys.Gameplay.GameState
{
    public class ClientMainMenuState : GameStateBehaviour
    {
        public override GameState ActiveState { get { return GameState.MainMenu; } }

        [SerializeField] NameGenerationData m_NameGenerationData;
        [SerializeField] LobbyUIMediator m_LobbyUIMediator;
        [SerializeField] GameObject m_SignInSpinner;

        [Inject] AuthenticationServiceFacade m_AuthServiceFacade;
        [Inject] LocalLobbyUser m_LocalUser;
        [Inject] LocalLobby m_LocalLobby;
        [Inject] ProfileManager m_ProfileManager;
      
        protected override void Awake()
        {
            base.Awake();

            m_LobbyUIMediator.Hide();

            if (string.IsNullOrEmpty(Application.cloudProjectId))
            {
                OnSignInFailed();
                return;
            }

            TrySignIn();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(m_NameGenerationData);
            builder.RegisterComponent(m_LobbyUIMediator);
        }


        private async void TrySignIn()
        {
            try
            {
                var unityAuthenticationInitOptions = new InitializationOptions();
                var profile = m_ProfileManager.Profile;
                if (profile.Length > 0)
                {
                    unityAuthenticationInitOptions.SetProfile(profile);
                }

                await m_AuthServiceFacade.InitializeAndSignInAsync(unityAuthenticationInitOptions);
                OnAuthSignIn();
                m_ProfileManager.onProfileChanged += OnProfileChanged;
            }
            catch (Exception)
            {
                OnSignInFailed();
            }
        }
        
        private void OnAuthSignIn()
        {
            m_SignInSpinner.SetActive(false);

            m_LocalUser.ID = AuthenticationService.Instance.PlayerId;
            m_LocalLobby.AddUser(m_LocalUser);
            StartCoroutine(WaitServerEnter());
        }

        IEnumerator WaitServerEnter()
        {
            yield return new WaitForEndOfFrame();
            OnStartClicked();
        }
        
        private void OnSignInFailed()
        {
            if (m_SignInSpinner)
            {
                m_SignInSpinner.SetActive(false);
            }
        }

        protected override void OnDestroy()
        {
            m_ProfileManager.onProfileChanged -= OnProfileChanged;
            base.OnDestroy();
        }

        async void OnProfileChanged()
        {
            m_SignInSpinner.SetActive(true);
            await m_AuthServiceFacade.SwitchProfileAndReSignInAsync(m_ProfileManager.Profile);

            m_SignInSpinner.SetActive(false);

            Debug.Log($"Signed in. Unity Player ID {AuthenticationService.Instance.PlayerId}");

            // Updating LocalUser and LocalLobby
            m_LocalLobby.RemoveUser(m_LocalUser);
            m_LocalUser.ID = AuthenticationService.Instance.PlayerId;
            m_LocalLobby.AddUser(m_LocalUser);
        }

        public void OnStartClicked()
        {
            m_LobbyUIMediator.ToggleJoinLobbyUI();
            m_LobbyUIMediator.Show();
        }

        public void OnDirectIPClicked()
        {
            m_LobbyUIMediator.Hide();
            //m_IPUIMediator.Show();
        }

        public void OnChangeProfileClicked()
        {
           // m_UIProfileSelector.Show();
        }
    }
}