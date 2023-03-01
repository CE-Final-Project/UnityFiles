﻿using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Services.Lobbies.Models;

namespace Script.GameFramework.Data
{
    public class LobbyData
    {
        private bool _isGameStarted { get; set; }
        private string _hostIp { get; set; }
        
        public bool IsGameStarted
        {
            get => _isGameStarted;
            set => _isGameStarted = value;
        }
        
        public string HostIp
        {
            get => _hostIp;
            set => _hostIp = value;
        }
        
        public void Initialize(Dictionary<string,DataObject> lobbyData)
        {
            UpdateState(lobbyData);
        }
        
        private void UpdateState([CanBeNull] IReadOnlyDictionary<string, DataObject> lobbyData)
        {
            if (lobbyData?.ContainsKey("isGameStarted") != null)
            {
                _isGameStarted = bool.Parse(lobbyData["isGameStarted"].Value);
            }
            
            if (lobbyData?.ContainsKey("hostIp") != null)
            {
                _hostIp = lobbyData["hostIp"].Value;
            }
        }
        
        public Dictionary<string, DataObject> Serialize()
        {
            var data = new Dictionary<string, DataObject>();
            data.Add("isGameStarted", new DataObject(DataObject.VisibilityOptions.Member, _isGameStarted.ToString()));
            data.Add("hostIp", new DataObject(DataObject.VisibilityOptions.Member, _hostIp));
            return data;
        }
    }
}