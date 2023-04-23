using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script.Game.GameplayObject.RuntimeDataContainers
{
    public class GameStats : MonoBehaviour
    {
        public static GameStats Instance { get; private set; }

        private DateTime _startTime;
        private DateTime _playTime;
        
        
        [SerializeField] private PlayersStats m_PlayersStats; 
        [SerializeField] private EnemiesStats m_EnemiesStats;
        
        private DynamicDiffStat m_DynamicDiffStat;
        
        public DynamicDiffStat DynamicDiffStat => m_DynamicDiffStat;

        public PlayersStats PlayersStats => m_PlayersStats;
        public EnemiesStats EnemiesStats => m_EnemiesStats;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            m_DynamicDiffStat = new DynamicDiffStat(0, 0, 0, 0, 0);

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public void StartTracking()
        {   
            // Reset Stats if already tracking
            StopTracking();
            
            Debug.Log("Starting Tracking");
            // Start new tracking stats
            m_PlayersStats.StartTracking();
            m_EnemiesStats.StartTracking();
        }
        
        public void StopTracking()
        {
            Debug.Log("Stop Tracking");
            m_PlayersStats.StopTracking();
            m_EnemiesStats.StopTracking();
        }
    }
}