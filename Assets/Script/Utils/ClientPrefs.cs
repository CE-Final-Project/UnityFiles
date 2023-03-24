using System;
using UnityEngine;

namespace Script.Utils
{
    public static class ClientPrefs
    {
        private const string MasterVolumeKey = "MasterVolume";
        private const string MusicVolumeKey = "MusicVolume";
        private const string ClientGuidKey = "client_guid";
        private const string AvailableProfilesKey = "AvailableProfiles";
        
        private const float DefaultMasterVolume = 0.5f;
        private const float DefaultMusicVolume = 0.8f;
        
        public static float GetMasterVolume()
        {
            return PlayerPrefs.GetFloat(MasterVolumeKey, DefaultMasterVolume);
        }
        
        public static void SetMasterVolume(float volume)
        {
            PlayerPrefs.SetFloat(MasterVolumeKey, volume);
        }
        
        public static float GetMusicVolume()
        {
            return PlayerPrefs.GetFloat(MusicVolumeKey, DefaultMusicVolume);
        }
        
        public static void SetMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        }
        
        public static string GetGuid()
        {
            if (PlayerPrefs.HasKey(ClientGuidKey))
            {
                return PlayerPrefs.GetString(ClientGuidKey);
            }
            
            Guid guid = System.Guid.NewGuid();
            string guidString = guid.ToString();
            
            PlayerPrefs.SetString(ClientGuidKey, guidString);
            return guidString;
        }
        
        public static string GetAvailableProfiles()
        {
            return PlayerPrefs.GetString(AvailableProfilesKey, "");
        }
        
        public static void SetAvailableProfiles(string availableProfiles)
        {
            PlayerPrefs.SetString(AvailableProfilesKey, availableProfiles);
        }
    }
}