using System;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

namespace disguys.Looby
{
    /// <summary>
    /// 이 클래스는 로컬 로비 사용자 인스턴스의 데이터를 포함합니다.
    /// 로컬 사용자의 변경 사항을 전체 로비에 푸시해야 할 때 업데이트 데이터를 관찰합니다.
    /// 즉, 이 클래스는 로컬 사용자의 데이터를 캡슐화하고 변경 사항을 관찰하여 로비 상태를 동기화하는 데 사용됩니다. 이 클래스는 Unity
    /// MonoBehaviour를 상속하며 로컬 사용자 데이터에 대한 로컬 UI 업데이트 및 이벤트 발생 기능을 제공합니다.
    /// </summary>
    [Serializable]
    public class LocalLobbyUser
    {
        public event Action<LocalLobbyUser> changed;

        public LocalLobbyUser()
        {
            m_UserData = new UserData(isHost: false, displayName: null, id: null);
        }

        public struct UserData
        {
            public bool IsHost { get; set; }
            public string DisplayName { get; set; }
            public string ID { get; set; }

            public UserData(bool isHost, string displayName, string id)
            {
                IsHost = isHost;
                DisplayName = displayName;
                ID = id;
            }
        }

        UserData m_UserData;

        public void ResetState()
        {
            m_UserData = new UserData(false, m_UserData.DisplayName, m_UserData.ID);
        }

        /// <summary>
        /// Used for limiting costly OnChanged actions to just the members which actually changed.
        /// </summary>
        [Flags]
        public enum UserMembers
        {
            IsHost = 1,
            DisplayName = 2,
            ID = 4,
        }

        UserMembers m_LastChanged;

        public bool IsHost
        {
            get { return m_UserData.IsHost; }
            set
            {
                if (m_UserData.IsHost != value)
                {
                    m_UserData.IsHost = value;
                    m_LastChanged = UserMembers.IsHost;
                    OnChanged();
                }
            }
        }

        public string DisplayName
        {
            get => m_UserData.DisplayName;
            set
            {
                if (m_UserData.DisplayName != value)
                {
                    m_UserData.DisplayName = value;
                    m_LastChanged = UserMembers.DisplayName;
                    OnChanged();
                }
            }
        }

        public string ID
        {
            get => m_UserData.ID;
            set
            {
                if (m_UserData.ID != value)
                {
                    m_UserData.ID = value;
                    m_LastChanged = UserMembers.ID;
                    OnChanged();
                }
            }
        }


        public void CopyDataFrom(LocalLobbyUser lobby)
        {
            var data = lobby.m_UserData;
            int lastChanged = // Set flags just for the members that will be changed.
                (m_UserData.IsHost == data.IsHost ? 0 : (int)UserMembers.IsHost) |
                (m_UserData.DisplayName == data.DisplayName ? 0 : (int)UserMembers.DisplayName) |
                (m_UserData.ID == data.ID ? 0 : (int)UserMembers.ID);

            if (lastChanged == 0) // Ensure something actually changed.
            {
                return;
            }

            m_UserData = data;
            m_LastChanged = (UserMembers)lastChanged;

            OnChanged();
        }

        void OnChanged()
        {
            changed?.Invoke(this);
        }

        public Dictionary<string, PlayerDataObject> GetDataForUnityServices() =>
            new Dictionary<string, PlayerDataObject>()
            {
                {"DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, DisplayName)},
            };
    }
}