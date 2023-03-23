using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Script.Utils
{
    public class ProfileManager
    {
        public const string AuthProfileCommandLineArg = "-AuthProfile";

        private string m_Profile;

        public string Profile
        {
            get
            {
                return m_Profile ??= GetProfile();
            }
            set
            {
                m_Profile = value;
                OnProfileChanged?.Invoke();
            }
        }

        public event Action OnProfileChanged;

        private List<string> _availableProfiles;

        public ReadOnlyCollection<string> AvailableProfiles
        {
            get
            {
                if (_availableProfiles == null)
                {
                    LoadProfiles();
                }

                return _availableProfiles.AsReadOnly();
            }
        }

        public void CreateProfile(string profile)
        {
            _availableProfiles.Add(profile);
            SaveProfiles();
        }

        public void DeleteProfile(string profile)
        {
            _availableProfiles.Remove(profile);
            SaveProfiles();
        }

        private static string GetProfile()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] == AuthProfileCommandLineArg)
                {
                    string profileId = arguments[i + 1];
                    return profileId;
                }
            }

#if UNITY_EDITOR

            // When running in the Editor make a unique ID from the Application.dataPath.
            // This will work for cloning projects manually, or with Virtual Projects.
            // Since only a single instance of the Editor can be open for a specific
            // dataPath, uniqueness is ensured.
            byte[] hashedBytes = new MD5CryptoServiceProvider()
                .ComputeHash(Encoding.UTF8.GetBytes(Application.dataPath));
            Array.Resize(ref hashedBytes, 16);
            return new Guid(hashedBytes).ToString("N")[2..];
#else
            return "";
#endif
        }

        private void LoadProfiles()
        {
            _availableProfiles = new List<string>();
            string loadedProfiles = ClientPrefs.GetAvailableProfiles();
            foreach (string profile in loadedProfiles.Split(',')) // this works since we're sanitizing our input strings
            {
                if (profile.Length > 0)
                {
                    _availableProfiles.Add(profile);
                }
            }
        }

        private void SaveProfiles()
        {
            string profilesToSave = "";
            foreach (string profile in _availableProfiles)
            {
                profilesToSave += profile + ",";
            }
            ClientPrefs.SetAvailableProfiles(profilesToSave);
        }
    }
}