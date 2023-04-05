using System;
using System.Collections.Generic;
using Script.Configuration;
using Script.Game.Actions;
using Script.Game.GameplayObject.Character;
using UnityEngine;
using Action = Script.Game.Actions.Action;

namespace Script.Game.GameplayObject.RuntimeDataContainers
{
    public class GameDataSource : MonoBehaviour
    {
        public static GameDataSource Instance { get; private set; }
        
        [Header("Character classes")]
        [Tooltip("All CharacterClass data should be slotted in here")]
        [SerializeField] 
        private CharacterClass[] characterData;
        
        private Dictionary<CharacterTypeEnum, CharacterClass> _characterDataMap;
        
        //Actions that are directly listed here will get automatically assigned ActionIDs and they don't need to be a part of m_ActionPrototypes array
        [Header("Common action prototypes")]
        [SerializeField]
        private Action m_GeneralChaseActionPrototype;

        [SerializeField] private Action m_GeneralTargetActionPrototype;

        [SerializeField] private Action m_Emote1ActionPrototype;

        [SerializeField] private Action m_Emote2ActionPrototype;

        [SerializeField] private Action m_Emote3ActionPrototype;

        [SerializeField] private Action m_Emote4ActionPrototype;

        [SerializeField] private Action m_ReviveActionPrototype;

        [SerializeField] private Action m_StunnedActionPrototype;

        [SerializeField] private Action m_DropActionPrototype;

        [SerializeField] private Action m_PickUpActionPrototype;

        [Tooltip("All Action prototype scriptable objects should be slotted in here")]
        [SerializeField]
        private Action[] m_ActionPrototypes;

        public Action GeneralChaseActionPrototype => m_GeneralChaseActionPrototype;

        public Action GeneralTargetActionPrototype => m_GeneralTargetActionPrototype;

        public Action Emote1ActionPrototype => m_Emote1ActionPrototype;

        public Action Emote2ActionPrototype => m_Emote2ActionPrototype;

        public Action Emote3ActionPrototype => m_Emote3ActionPrototype;

        public Action Emote4ActionPrototype => m_Emote4ActionPrototype;

        public Action ReviveActionPrototype => m_ReviveActionPrototype;

        public Action StunnedActionPrototype => m_StunnedActionPrototype;

        public Action DropActionPrototype => m_DropActionPrototype;
        public Action PickUpActionPrototype => m_PickUpActionPrototype;

        private List<Action> m_AllActions;

        public Action GetActionPrototypeByID(ActionID index)
        {
            return m_AllActions[index.ID];
        }

        public bool TryGetActionPrototypeByID(ActionID index, out Action action)
        {
            for (int i = 0; i < m_AllActions.Count; i++)
            {
                if (m_AllActions[i].ActionID == index)
                {
                    action = m_AllActions[i];
                    return true;
                }
            }

            action = null;
            return false;
        }

        /// <summary>
        /// Contents of the CharacterData list, indexed by CharacterType for convenience.
        /// </summary>
        public Dictionary<CharacterTypeEnum, CharacterClass> CharacterDataByType
        {
            get
            {
                if (_characterDataMap == null)
                {
                    _characterDataMap = new Dictionary<CharacterTypeEnum, CharacterClass>();
                    foreach (CharacterClass data in characterData)
                    {
                        if (_characterDataMap.ContainsKey(data.CharacterType))
                        {
                            throw new Exception($"Duplicate character definition detected: {data.CharacterType}");
                        }
                        _characterDataMap[data.CharacterType] = data;
                    }
                }
                return _characterDataMap;
            }
        }

        private void Awake()
        {
            if (Instance != null)
            {
                throw new Exception("Multiple GameDataSources defined!");
            }

            BuildActionIDs();

            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        private void BuildActionIDs()
        {
            var uniqueActions = new HashSet<Action>(m_ActionPrototypes)
            {
                GeneralChaseActionPrototype,
                GeneralTargetActionPrototype,
                Emote1ActionPrototype,
                Emote2ActionPrototype,
                Emote3ActionPrototype,
                Emote4ActionPrototype,
                ReviveActionPrototype,
                StunnedActionPrototype,
                DropActionPrototype,
                PickUpActionPrototype
            };

            m_AllActions = new List<Action>(uniqueActions.Count);

            int i = 0;
            foreach (Action uniqueAction in uniqueActions)
            {
                uniqueAction.ActionID = new ActionID { ID = i };
                m_AllActions.Add(uniqueAction);
                i++;
            }
        }
    }
}