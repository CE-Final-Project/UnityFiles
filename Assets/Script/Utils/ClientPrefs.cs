using UnityEngine;

namespace Survival.Game.Utils
{
    
    /// <summary>
    /// Singleton class which saves/loads local-client settings.
    /// (This is just a wrapper around the PlayerPrefs system,
    /// so that all the calls are in the same place.)
    /// </summary>
    /// 
    public static class ClientPrefs
    {
        private const string k_MasterVolumnKey = "MasterVolume";
        private const string k_MusicVolumeKey = "MusicVolume";
        private const string k_ClientGUIDKey = "client_guid";
        private const string k_AvailableProfilesKey = "AvailableProfiles";

        private const float k_DefaultMasterVolume = 0.5f;
        private const float k_DefaultMusicVolume = 0.8f;

        public static float GetMasterVolume()
        {
            return PlayerPrefs.GetFloat(k_MasterVolumnKey, k_DefaultMasterVolume);
        }

        public static void SetMasterVolume(float volume)
        {
            PlayerPrefs.SetFloat(k_MasterVolumnKey, volume);
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