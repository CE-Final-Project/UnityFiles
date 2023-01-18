using System;
using System.Collections.Generic;
using Survival.Game.Gameplay.Actions;
using Survival.Game.Gameplay.Configuration;
using Survival.Game.Gameplay.GameplayObjects.Character;
using UnityEngine;
using Action = Survival.Game.Gameplay.Actions.Action;

namespace Survival.Game.Gameplay.GameplayObjects
{
    public class GameDataSource : MonoBehaviour
    {
        /// <summary>
        /// static accessor for all GameData.
        /// </summary>
        public static GameDataSource Instance { get; private set; }

        [Header("Character classes")]
        [Tooltip("All CharacterClass data should be slotted in here")]
        [SerializeField]
        private CharacterClass[] m_CharacterData;

        Dictionary<CharacterTypeEnum, CharacterClass> m_CharacterDataMap;

        //Actions that are directly listed here will get automatically assigned ActionIDs and they don't need to be a part of m_ActionPrototypes array
        [Header("Common action prototypes")]
        [SerializeField]
        Actions.Action m_GeneralChaseActionPrototype;

        [SerializeField]
        Actions.Action m_GeneralTargetActionPrototype;

        [SerializeField]
        Actions.Action m_Emote1ActionPrototype;

        [SerializeField]
        Actions.Action m_Emote2ActionPrototype;

        [SerializeField]
        Actions.Action m_Emote3ActionPrototype;

        [SerializeField]
        Actions.Action m_Emote4ActionPrototype;

        [SerializeField]
        Actions.Action m_ReviveActionPrototype;

        [SerializeField]
        Actions.Action m_StunnedActionPrototype;

        [SerializeField]
        Actions.Action m_DropActionPrototype;

        [SerializeField]
        Actions.Action m_PickUpActionPrototype;

        [Tooltip("All Action prototype scriptable objects should be slotted in here")]
        [SerializeField]
        private Actions.Action[] m_ActionPrototypes;

        public Actions.Action GeneralChaseActionPrototype => m_GeneralChaseActionPrototype;

        public Actions.Action GeneralTargetActionPrototype => m_GeneralTargetActionPrototype;

        public Actions.Action Emote1ActionPrototype => m_Emote1ActionPrototype;

        public Actions.Action Emote2ActionPrototype => m_Emote2ActionPrototype;

        public Actions.Action Emote3ActionPrototype => m_Emote3ActionPrototype;

        public Actions.Action Emote4ActionPrototype => m_Emote4ActionPrototype;

        public Actions.Action ReviveActionPrototype => m_ReviveActionPrototype;

        public Actions.Action StunnedActionPrototype => m_StunnedActionPrototype;

        public Actions.Action DropActionPrototype => m_DropActionPrototype;
        public Actions.Action PickUpActionPrototype => m_PickUpActionPrototype;

        List<Actions.Action> m_AllActions;

        public Actions.Action GetActionPrototypeByID(ActionID index)
        {
            return m_AllActions[index.ID];
        }

        /// <summary>
        /// Contents of the CharacterData list, indexed by CharacterType for convenience.
        /// </summary>
        public Dictionary<CharacterTypeEnum, CharacterClass> CharacterDataByType
        {
            get
            {
                if (m_CharacterDataMap == null)
                {
                    m_CharacterDataMap = new Dictionary<CharacterTypeEnum, CharacterClass>();
                    foreach (CharacterClass data in m_CharacterData)
                    {
                        if (m_CharacterDataMap.ContainsKey(data.CharacterType))
                        {
                            throw new System.Exception($"Duplicate character definition detected: {data.CharacterType}");
                        }
                        m_CharacterDataMap[data.CharacterType] = data;
                    }
                }
                return m_CharacterDataMap;
            }
        }

        private void Awake()
        {
            if (Instance != null)
            {
                throw new System.Exception("Multiple GameDataSources defined!");
            }

            BuildActionIDs();

            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        void BuildActionIDs()
        {
            var uniqueActions = new HashSet<Actions.Action>(m_ActionPrototypes);
            uniqueActions.Add(GeneralChaseActionPrototype);
            uniqueActions.Add(GeneralTargetActionPrototype);
            uniqueActions.Add(Emote1ActionPrototype);
            uniqueActions.Add(Emote2ActionPrototype);
            uniqueActions.Add(Emote3ActionPrototype);
            uniqueActions.Add(Emote4ActionPrototype);
            uniqueActions.Add(ReviveActionPrototype);
            uniqueActions.Add(StunnedActionPrototype);
            uniqueActions.Add(DropActionPrototype);
            uniqueActions.Add(PickUpActionPrototype);

            m_AllActions = new List<Actions.Action>(uniqueActions.Count);

            int i = 0;
            foreach (var uniqueAction in uniqueActions)
            {
                uniqueAction.ActionID = new ActionID { ID = i };
                m_AllActions.Add(uniqueAction);
                i++;
            }
        }
    }
}
