using System;
using System.Collections.Generic;
using Script.Configuration;
using Script.Game.Action;
using Script.Game.GameplayObject.Character;
using UnityEngine;

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
        
        [Tooltip("All Action prototype scriptable objects should be slotted in here")]
        [SerializeField]
        private Action.Action[] actionPrototypes;

        private List<Action.Action> _allActions;

        public Action.Action GetActionPrototypeByID(ActionID index)
        {
            return _allActions[index.ID];
        }
        
        public bool TryGetActionPrototypeByID(ActionID index, out Action.Action action)
        {
            for (int i = 0; i < _allActions.Count; i++)
            {
                if (_allActions[i].ActionID != index) continue;
                
                action = _allActions[i];
                return true;
            }
            action = null;
            return false;
        }
        
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
                        _characterDataMap.Add(data.CharacterType, data);
                    }
                }
                return _characterDataMap;
            }
        }
        
        private void Awake()
        {
            if (Instance != null)
            {
                throw new Exception("Multiple GameDataSource instances detected!");
            }

            BuildActionIDs();
            
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        
        private void BuildActionIDs()
        {
            var uniqueActions = new HashSet<Action.Action>(actionPrototypes);
            
            _allActions = new List<Action.Action>(uniqueActions.Count);
            int i = 0;
            foreach (Action.Action action in uniqueActions)
            {
                action.ActionID = new ActionID{ID = i};
                _allActions.Add(action);
                i++;
            }
        }
    }
}