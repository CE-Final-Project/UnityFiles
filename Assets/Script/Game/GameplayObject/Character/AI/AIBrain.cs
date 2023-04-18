using System;
using System.Collections.Generic;
using Script.Configuration;
using Script.Game.Actions.ActionPlayers;
using Script.Game.GameplayObject.RuntimeDataContainers;
using UnityEngine;

namespace Script.Game.GameplayObject.Character.AI
{
    /// <summary>
    /// Handles enemy AI. Contains AIStateLogics that handle some of the details,
    /// and has various utility functions that are called by those AIStateLogics
    /// </summary>
    public class AIBrain
    {
        private enum AIStateType
        {
            IDLE,
            ATTACK,
            ATTACK_WEAK,
            //FLEE,
            
        }

        static readonly AIStateType[] k_AIStates = (AIStateType[])Enum.GetValues(typeof(AIStateType));

        private ServerCharacter m_ServerCharacter;
        private ServerActionPlayer m_ServerActionPlayer;
        private AIStateType m_CurrentState;
        private Dictionary<AIStateType, AIState> m_Logics;
        private List<ServerCharacter> m_HatedEnemies;

        /// <summary>
        /// If we are created by a spawner, the spawner might override our detection radius
        /// -1 is a sentinel value meaning "no override"
        /// </summary>
        private float m_DetectRangeOverride = -1;

        public AIBrain(ServerCharacter me, ServerActionPlayer myServerActionPlayer)
        {
            m_ServerCharacter = me;
            m_ServerActionPlayer = myServerActionPlayer;

            m_Logics = new Dictionary<AIStateType, AIState>
            {
                [AIStateType.IDLE] = new IdleAIState(this),
                [AIStateType.ATTACK] = new AttackAIState(this, m_ServerActionPlayer),
                [AIStateType.ATTACK_WEAK] = new AttackWeakAIState(this, m_ServerActionPlayer),
                //[AIStateType.FLEE] = new FleeAIState(this),
                

            };
            m_HatedEnemies = new List<ServerCharacter>();
            m_CurrentState = AIStateType.IDLE;
        }

        /// <summary>
        /// Should be called by the AIBrain's owner each Update()
        /// </summary>
        public void Update()
        {
            AIStateType newState = FindBestEligibleAIState();
            if (m_CurrentState != newState)
            {
                m_Logics[newState].Initialize();
            }
            m_CurrentState = newState;
            m_Logics[m_CurrentState].Update();
        }

        /// <summary>
        /// Called when we received some HP. Positive HP is healing, negative is damage.
        /// </summary>
        /// <param name="inflicter">The person who hurt or healed us. May be null. </param>
        /// <param name="amount">The amount of HP received. Negative is damage. </param>
        public void ReceiveHP(ServerCharacter inflicter, int amount)
        {
            if (inflicter != null && amount < 0)
            {
                Hate(inflicter);
            }
        }

        private AIStateType FindBestEligibleAIState()
        {
            // for now we assume the AI states are in order of appropriateness,
            // which may be nonsensical when there are more states
            AIStateType bestAiStateType = m_CurrentState;
            foreach (AIStateType aiStateType in k_AIStates)
            {
                if (!m_Logics[bestAiStateType].IsEligible())
                {
                    bestAiStateType = AIStateType.IDLE;
                }

                if (!m_Logics[aiStateType].IsEligible())
                {
                    continue;
                }

                if (m_Logics[bestAiStateType].Priority() < m_Logics[aiStateType].Priority())
                {
                    bestAiStateType = aiStateType;
                }
            }

            return bestAiStateType;
        }

        /// <summary>
        /// Returns true if it be appropriate for us to murder this character, starting right now!
        /// </summary>
        public bool IsAppropriateFoe(ServerCharacter potentialFoe)
        {
            if (potentialFoe == null ||
                potentialFoe.IsNpc ||
                potentialFoe.LifeState != LifeState.Alive) //||
                // potentialFoe.IsStealthy.Value)
            {
                return false;
            }

            // Also, we could use NavMesh.Raycast() to see if we have line of sight to foe?
            return true;
        }

        /// <summary>
        /// Notify the AIBrain that we should consider this character an enemy.
        /// </summary>
        /// <param name="character"></param>
        public void Hate(ServerCharacter character)
        {
            if (!m_HatedEnemies.Contains(character))
            {
                m_HatedEnemies.Add(character);
            }
        }

        /// <summary>
        /// Return the raw list of hated enemies -- treat as read-only!
        /// </summary>
        public List<ServerCharacter> GetHatedEnemies()
        {
            // first we clean the list -- remove any enemies that have disappeared (became null), are dead, etc.
            for (int i = m_HatedEnemies.Count - 1; i >= 0; i--)
            {
                if (!IsAppropriateFoe(m_HatedEnemies[i]))
                {
                    m_HatedEnemies.RemoveAt(i);
                }
            }
            return m_HatedEnemies;
        }

        /// <summary>
        /// Retrieve info about who we are. Treat as read-only!
        /// </summary>
        /// <returns></returns>
        public ServerCharacter GetMyServerCharacter()
        {
            return m_ServerCharacter;
        }

        /// <summary>
        /// Convenience getter that returns the CharacterData associated with this creature.
        /// </summary>
        public CharacterClass CharacterData
        {
            get
            {
                return GameDataSource.Instance.CharacterDataByType[m_ServerCharacter.CharacterType];
            }
        }

        /// <summary>
        /// The range at which this character can detect enemies, in meters.
        /// This is usually the same value as is indicated by our game data, but it
        /// can be dynamically overridden.
        /// </summary>
        public float DetectRange
        {
            get
            {
                return (m_DetectRangeOverride == -1) ? CharacterData.DetectRange : m_DetectRangeOverride;
            }

            set
            {
                m_DetectRangeOverride = value;
            }
        }
    }
}
