using System.Collections.Generic;
using Script.Game.Actions;
using Script.Game.Actions.ActionPlayers;
using Script.Game.Actions.Input;
using Script.Game.GameplayObject.RuntimeDataContainers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script.Game.GameplayObject.Character.AI
{
    public class GroupAIState : AIState
    {
        private AIBrain m_Brain;
        private ServerActionPlayer m_ServerActionPlayer;
        private ServerCharacter m_Foe;
        private Action m_CurAttackAction;
        private List<ServerCharacter> m_Squad;



        List<Action> m_AttackActions;

        public GroupAIState(AIBrain brain)
        {
            m_Brain = brain;
            m_Squad = new List<ServerCharacter>();

        }

        public override bool IsEligible()
        {
            return m_Squad.Count < 4;
        }

        public override int Priority()
        {
            return 120;
        }


        public override void Initialize()
        {
            m_Brain.GetMyServerCharacter().Movement.SetMovementTarget(ChooseTarget().transform.position);

            Debug.Log("GROUP");

        }

        public override void Update()
        {
            AddToSquad();
        }

        private ServerCharacter ChooseTarget()
        {
            Vector3 myPosition = m_Brain.GetMyServerCharacter().PhysicsWrapper.Transform.position;

            float closestDistanceSqr = int.MaxValue;
            ServerCharacter closestFoe = null;

            foreach (var foe in EnemyServerCharacter.GetEnemyServerCharacters())
            {
                float distanceSqr = (myPosition - foe.PhysicsWrapper.Transform.position).sqrMagnitude;
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    
                }

                if (!m_Squad.Contains(foe)){
                    closestFoe = foe;
                }
            }
            return closestFoe;
        }

        private void AddToSquad()
        {
            float detectionRange = 0.5f;
            // we are doing this check every Update, so we'll use square-magnitude distance to avoid the expensive sqrt (that's implicit in Vector3.magnitude)
            float detectionRangeSqr = detectionRange * detectionRange;
            Vector3 position = m_Brain.GetMyServerCharacter().PhysicsWrapper.Transform.position;

            // in this game, NPCs only attack players (and never other NPCs), so we can just iterate over the players to see if any are nearby
            foreach (var character in EnemyServerCharacter.GetEnemyServerCharacters())
            {
                if (!m_Brain.IsAppropriateFoe(character) && (character.PhysicsWrapper.Transform.position - position).sqrMagnitude <= detectionRangeSqr)
                {
                    m_Squad.Add(character);
                }
            }

            Debug.Log(m_Squad.Count);

            if(m_Squad.Count >= 4)
            {

                foreach (var character in PlayerServerCharacter.GetPlayerServerCharacters())
                {
                    if (m_Brain.IsAppropriateFoe(character) && (character.PhysicsWrapper.Transform.position - position).sqrMagnitude <= m_Brain.DetectRange * m_Brain.DetectRange)
                    {
                        m_Brain.Hate(character);
                    }
                }
            }

            




        }





    }
}
