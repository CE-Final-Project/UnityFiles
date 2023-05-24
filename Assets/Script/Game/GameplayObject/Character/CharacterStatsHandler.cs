using System;
using Script.UI;
using Script.Utils;
using Unity.Netcode;
using UnityEngine;

namespace Script.Game.GameplayObject.Character
{
    [RequireComponent(
        typeof(ServerCharacter), 
        typeof(NetworkHealthState), 
        typeof(NetworkNameState))]
    public class CharacterStatsHandler : NetworkBehaviour
    {
        private NetworkHealthState m_NetworkHealthState;
        
        private NetworkNameState m_NetworkNameState;
        
        private ServerCharacter m_ServerCharacter;
        
        private ClientAvatarGuidHandler m_ClientAvatarGuidHandler;
        
        private NetworkAvatarGuidState m_NetworkAvatarGuidState;
        
        private IntVariable m_BaseHP;

        private PlayerStatsBarUI m_PlayerStatsBarUI;

        private void Awake()
        {
            m_NetworkHealthState = GetComponent<NetworkHealthState>();
            m_NetworkNameState = GetComponent<NetworkNameState>();
            m_ServerCharacter = GetComponent<ServerCharacter>();

            m_PlayerStatsBarUI = FindObjectOfType<PlayerStatsBarUI>();
            
            if (m_PlayerStatsBarUI is null)
            {
                Debug.LogError("PlayerStatsBarUI not found");
            }
        }
        
        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }
            m_BaseHP = m_ServerCharacter.CharacterClass.BaseHP;
            m_PlayerStatsBarUI.SetMaxHealth(m_BaseHP.Value);
        
            // Track health state
            m_NetworkHealthState.HitPoints.OnValueChanged += OnHitPointsChanged;

            // Set Character Image
            m_PlayerStatsBarUI.SetCharacterImage(m_ServerCharacter.CharacterClass.ClassBannerLit);
            
            // Add Skills 
            if (m_ServerCharacter.CharacterClass.Skill1 != null)
            {
                m_PlayerStatsBarUI.AddSkill(m_ServerCharacter.CharacterClass.Skill1.Config.Icon);
            }
            if (m_ServerCharacter.CharacterClass.Skill2 != null)
            {
                m_PlayerStatsBarUI.AddSkill(m_ServerCharacter.CharacterClass.Skill2.Config.Icon);
            }
            if (m_ServerCharacter.CharacterClass.Skill3 != null)
            {
                m_PlayerStatsBarUI.AddSkill(m_ServerCharacter.CharacterClass.Skill3.Config.Icon);
            }
        }

        private void OnDisable()
        {
            if (m_NetworkHealthState is not null)
            {
                m_NetworkHealthState.HitPoints.OnValueChanged -= OnHitPointsChanged;
            }
        }

        private void OnHitPointsChanged(int previousValue, int newValue)
        {
            // Update UI to reflect new HP
            m_PlayerStatsBarUI.SetHealth(newValue);
        }
    }
}