using UnityEngine;

namespace Script.Game.Action.Input
{
    public class ChargedActionInput : BaseActionInput
    {
        protected float StartTime;

        private void Start()
        {
            Transform transform1 = transform;
            
            transform1.position = Origin;
            
            StartTime = Time.time;
            
            ActionRequestData data = new ActionRequestData()
            {
                Position = transform1.position,
                ActionID = ActionPrototypeID,
                ShouldQueue = false,
                TargetIDs = null
            };
            SendInput(data);
        }
        
        public override void OnReleaseKey()
        {
            // PlayerOwner.RecvStopChargingUpServerRpc();
            Destroy(gameObject);
        }
    }
}