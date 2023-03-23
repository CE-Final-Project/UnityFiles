using System.Collections.Generic;
using Script.Game.Action.Input;
using Script.Game.GameplayObject.RuntimeDataContainers;
using UnityEngine;
using UnityEngine.Pool;

namespace Script.Game.Action
{
    public static class ActionFactory
    {
        private static readonly Dictionary<ActionID, ObjectPool<Action>> ActionPools = new Dictionary<ActionID, ObjectPool<Action>>();
        
        private static ObjectPool<Action> GetActionPool(ActionID actionID)
        {
            if (!ActionPools.TryGetValue(actionID, out var actionPool))
            {
                actionPool = new ObjectPool<Action>(
                    createFunc: () => Object.Instantiate(GameDataSource.Instance.GetActionPrototypeByID(actionID)),
                    actionOnRelease: action => action.Reset(),
                    actionOnDestroy: Object.Destroy);
                
                ActionPools.Add(actionID, actionPool);
            }

            return actionPool;
        }
        
        public static Action CreateActionFromData(ref ActionRequestData data)
        {
            Action ret = GetActionPool(data.ActionID).Get();
            ret.Initialize(ref data);
            return ret;
        }
        
        public static void ReturnAction(Action action)
        {
            var pool = GetActionPool(action.ActionID);
            pool.Release(action);
        }
        
        public static void PurgePooledActions()
        {
            foreach (var actionPool in ActionPools.Values)
            {
                actionPool.Clear();
            }
        }
    }
}