using System.Collections.Generic;
using Script.Game.Actions;
using Script.Game.Actions.ActionPlayers;
using Script.Game.Actions.Input;
using Script.Game.GameplayObject.RuntimeDataContainers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Script.Game.GameplayObject.Character.AI
{
    public class FleeAIState : AIState
    {
        private AIBrain m_Brain;
        private ServerActionPlayer m_ServerActionPlayer;
        private ServerCharacter m_Foe;
        private Action m_CurAttackAction;
      


        List<Action> m_AttackActions;

        public FleeAIState(AIBrain brain)
        {
            m_Brain = brain;
            
        }

        public override bool IsEligible()
        {
            //return true;
            return m_Brain.GetMyServerCharacter().HitPoints < 10;
        }

        public override int Priority()
        {
            return 200;
        }
        

        public override void Initialize()
        {
           m_Brain.GetMyServerCharacter().Movement.SetMovementTarget(new Vector3(0,0,0));

            Debug.Log("FLEE");
           
        }

        public override void Update()
        {
                    
            

            
        }

        



    }
}
