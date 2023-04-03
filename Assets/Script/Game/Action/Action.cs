using System;
using Script.Game.Action.Input;
using Script.Game.GameplayObject.Character;
using Script.Game.GameplayObject.RuntimeDataContainers;
using Unity.Netcode;
using UnityEngine;

namespace Script.Game.Action
{
    public abstract class Action : ScriptableObject
    {
        [NonSerialized]
        public ActionID ActionID;

        public const string DefaultHitReact = "HitReact1";

        protected ActionRequestData m_Data;
        
        public float TimeStarted { get; set; }

        public float TimeRunning => Time.time - TimeStarted;
        
        public ref ActionRequestData Data => ref m_Data;

        public ActionConfig Config;

        public void Initialize(ref ActionRequestData data)
        {
            m_Data = data;
            ActionID = data.ActionID;
        }

        public void Reset()
        {
            m_Data = default;
            ActionID = default;
            TimeStarted = 0;
        }

        /// <summary>
        /// Called when the Action starts actually playing (which may be after it is created, because of queueing).
        /// </summary>
        /// <returns>false if the action decided it doesn't want to run after all, true otherwise. </returns>
        public abstract bool OnStart(ServerCharacter serverCharacter);

        /// <summary>
        /// Called each frame while the action is running.
        /// </summary>
        /// <returns>true to keep running, false to stop. The Action will stop by default when its duration expires, if it has a duration set. </returns>
        public abstract bool OnUpdate(ServerCharacter clientCharacter);

        // public virtual bool ShouldBecomeNoneBlocking()
        // {
        //     return Config.BlockingMode == BlockingModeType.OnlyDuringExecTime ? TimeRunning > Config.ExpecTimeSeconds : false;
        // }

        public virtual void End(ServerCharacter serverCharacter)
        {
            Cancel(serverCharacter);
        }
        
        public virtual void Cancel(ServerCharacter serverCharacter) { }

        public virtual bool ChainIntoNewAction(ref ActionRequestData newAction)
        {
            return false;
        }

        public virtual void CollisionEntered(ServerCharacter serverCharacter, Collision collision) { }

        public enum BuffableValue
        {
            PercentHealingReceived, // unbuffed value is 1.0. Reducing to 0 would mean "no healing". 2 would mean "double healing"
            PercentDamageReceived,  // unbuffed value is 1.0. Reducing to 0 would mean "no damage". 2 would mean "double damage"
            ChanceToStunTramplers,  // unbuffed value is 0. If > 0, is the chance that someone trampling this character becomes stunned
        }
        
        public virtual void BuffValue(BuffableValue buffType, ref float buffedValue) { }

        public static float GetUnbuffedValue(Action.BuffableValue buffType)
        {
            switch (buffType)
            {
                case BuffableValue.PercentDamageReceived: return 1;
                case BuffableValue.PercentHealingReceived: return 1;
                case BuffableValue.ChanceToStunTramplers: return 0;
                default: throw new System.Exception($"Unknown buff type {buffType}");
            }
        }
        
        public enum GameplayActivity
        {
            AttackedByEnemy,
            Healed,
            StoppedChargingUp,
            UsingAttackAction, // called immediately before we perform the attack Action
        }
        
        public virtual void OnGameplayActivity(ServerCharacter serverCharacter, GameplayActivity activityType) { }

        public bool AnticipatedClient { get; protected set; }
        
        public virtual bool OnStartClient(ClientCharacter clientCharacter)
        {
            AnticipatedClient = false; //once you start for real you are no longer an anticipated action.
            TimeStarted = UnityEngine.Time.time;
            return true;
        }
        
        public virtual bool OnUpdateClient(ClientCharacter clientCharacter)
        {
            return ActionConclusion.Continue;
        }
        
        public virtual void EndClient(ClientCharacter clientCharacter)
        {
            CancelClient(clientCharacter);
        }
        
        public virtual void CancelClient(ClientCharacter clientCharacter) { }

        public static bool ShouldClientAnticipate(ClientCharacter clientCharacter, ref ActionRequestData data)
        {
            if (!clientCharacter.CanPerformActions) { return false; }

            var actionDescription = GameDataSource.Instance.GetActionPrototypeByID(data.ActionID).Config;

            //for actions with ShouldClose set, we check our range locally. If we are out of range, we shouldn't anticipate, as we will
            //need to execute a ChaseAction (synthesized on the server) prior to actually playing the skill.
            bool isTargetEligible = true;
            if (data.ShouldClose == true)
            {
                ulong targetId = (data.TargetIDs != null && data.TargetIDs.Length > 0) ? data.TargetIDs[0] : 0;
                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject networkObject))
                {
                    float rangeSquared = actionDescription.Range * actionDescription.Range;
                    isTargetEligible = (networkObject.transform.position - clientCharacter.transform.position).sqrMagnitude < rangeSquared;
                }
            }

            //at present all Actionts anticipate except for the Target action, which runs a single instance on the client and is
            //responsible for action anticipation on its own.
            return isTargetEligible && actionDescription.Logic != ActionLogic.Target;
        }
        
        public virtual void OnAnimEventClient(ClientCharacter clientCharacter, string id) { }

        public virtual void OnStoppedChargingUpClient(ClientCharacter clientCharacter, float finalChargeUpPercentage) { }

        
        public virtual void AnticipateActionClient(ClientCharacter clientCharacter)
        {
            AnticipatedClient = true;
            TimeStarted = UnityEngine.Time.time;

            if (!string.IsNullOrEmpty(Config.AnimAnticipation))
            {
                clientCharacter.OurAnimator.SetTrigger(Config.AnimAnticipation);
            }
        }
    }
}