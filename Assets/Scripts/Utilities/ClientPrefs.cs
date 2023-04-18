using UnityEngine;

namespace disguys.Utilities
{
     /// <summary>
    /// 이 클래스는 로컬 클라이언트 설정을 저장하고 로드하는 싱글톤 클래스입니다.
    /// (이 클래스는 PlayerPrefs 시스템을 감싸는 래퍼 역할을 합니다.
    /// 즉, 모든 호출이 같은 위치에 있습니다.)
    public static class ClientPrefs
    {
        const string k_MasterVolumeKey = "MasterVolume";
        const string k_MusicVolumeKey = "MusicVolume";
        const string k_ClientGUIDKey = "client_guid";
        const string k_AvailableProfilesKey = "AvailableProfiles";

        const float k_DefaultMasterVolume = 0.5f;
        const float k_DefaultMusicVolume = 0.8f;

        public static float GetMasterVolume()
        {
            return PlayerPrefs.GetFloat(k_MasterVolumeKey, k_DefaultMasterVolume);
        }

        public static void SetMasterVolume(float volume)
        {
            PlayerPrefs.SetFloat(k_MasterVolumeKey, volume);
        }

        public static float GetMusicVolume()
        {
            return PlayerPrefs.GetFloat(k_MusicVolumeKey, k_DefaultMusicVolume);
        }

        public static void SetMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat(k_MusicVolumeKey, volume);
        }

        /// <summary>
        /// Either loads a Guid string from Unity preferences, or creates one and checkpoints it, then returns it.
        /// </summary>
        /// <returns>The Guid that uniquely identifies this client install, in string form. </returns>
        public static string GetGuid()
        {
            if (PlayerPrefs.HasKey(k_ClientGUIDKey))
            {
                return PlayerPrefs.GetString(k_ClientGUIDKey);
            }

            var guid = System.Guid.NewGuid();
            var guidString = guid.ToString();

            PlayerPrefs.SetString(k_ClientGUIDKey, guidString);
            return guidString;
        }

        public static string GetAvailableProfiles()
        {
            return PlayerPrefs.GetString(k_AvailableProfilesKey, "");
        }

        public static void SetAvailableProfiles(string availableProfiles)
        {
            PlayerPrefs.SetString(k_AvailableProfilesKey, availableProfiles);
        }

    }
}