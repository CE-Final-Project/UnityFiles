using System;
using System.Collections.Generic;
using Survival.Game.Gameplay.UserInput;
using Survival.Game.Gameplay.GameplayObjects;
using Survival.Game.Gameplay.GameplayObjects.Character;
using Unity.Netcode;
using UnityEngine;
using Action = Survival.Game.Gameplay.Actions.Action;
using SkillTriggerStyle = Survival.Game.Gameplay.UserInput.ClientInputSender.SkillTriggerStyle;

namespace Survival.Game.Gameplay.UI
{
    /// <summary>
    /// Provides logic for a Hero Action Bar with attack, skill buttons and a button to open emotes panel
    /// This bar tracks button clicks on hero action buttons for later use by ClientInputSender
    /// </summary>
    public class HeroActionBar : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The button that activates the basic action (comparable to right-clicking the mouse)")]
        UIHUDButton m_BasicActionButton;

        [SerializeField]
        [Tooltip("The button that activates the hero's first special move")]
        UIHUDButton m_SpecialAction1Button;

        [SerializeField]
        [Tooltip("The button that activates the hero's second special move")]
        UIHUDButton m_SpecialAction2Button;

        [SerializeField]
        [Tooltip("The button that opens/closes the Emote bar")]
        UIHUDButton m_EmoteBarButton;

        [SerializeField]
        [Tooltip("The Emote bar that will be enabled or disabled when clicking the Emote bar button")]
        GameObject m_EmotePanel;

        /// <summary>
        /// Our input-sender. Initialized in RegisterInputSender()
        /// </summary>
        ClientInputSender m_InputSender;

        /// <summary>
        /// Cached reference to local player's net state.
        /// We find the Sprites to use by checking the Skill1, Skill2, and Skill3 members of our chosen CharacterClass
        /// </summary>
        ServerCharacter m_ServerCharacter;

        /// <summary>
        /// If we have another player selected, this is a reference to their stats; if anything else is selected, this is null
        /// </summary>
        ServerCharacter m_SelectedPlayerServerCharacter;

        /// <summary>
        /// If m_SelectedPlayerNetState is non-null, this indicates whether we think they're alive. (Updated every frame)
        /// </summary>
        bool m_WasSelectedPlayerAliveDuringLastUpdate;

        /// <summary>
        /// Identifiers for the buttons on the action bar.
        /// </summary>
        enum ActionButtonType
        {
            BasicAction,
            Special1,
            Special2,
            EmoteBar,
        }

        /// <summary>
        /// Cached UI information about one of the buttons on the action bar.
        /// Takes care of registering/unregistering click-event messages,
        /// and routing the events into HeroActionBar.
        /// </summary>
        class ActionButtonInfo
        {
            public readonly ActionButtonType Type;
            public readonly UIHUDButton Button;
            public readonly UITooltipDetector Tooltip;

            /// <summary> T
            /// The current Action that is used when this button is pressed.
            /// </summary>
            public Action CurAction;

            readonly HeroActionBar m_Owner;

            public ActionButtonInfo(ActionButtonType type, UIHUDButton button, HeroActionBar owner)
            {
                Type = type;
                Button = button;
                Tooltip = button.GetComponent<UITooltipDetector>();
                m_Owner = owner;
            }

            public void RegisterEventHandlers()
            {
                Button.OnPointerDownEvent += OnClickDown;
                Button.OnPointerUpEvent += OnClickUp;
            }

            public void UnregisterEventHandlers()
            {
                Button.OnPointerDownEvent -= OnClickDown;
                Button.OnPointerUpEvent -= OnClickUp;
            }

            void OnClickDown()
            {
                m_Owner.OnButtonClickedDown(Type);
            }

            void OnClickUp()
            {
                m_Owner.OnButtonClickedUp(Type);
            }
        }

        /// <summary>
        /// Dictionary of info about all the buttons on the action bar.
        /// </summary>
        Dictionary<ActionButtonType, ActionButtonInfo> m_ButtonInfo;

        /// <summary>
        /// Cache the input sender from a <see cref="ClientPlayerAvatar"/> and self-initialize.
        /// </summary>
        /// <param name="clientPlayerAvatar"></param>
        void RegisterInputSender(ClientPlayerAvatar clientPlayerAvatar)
        {
            if (!clientPlayerAvatar.TryGetComponent(out ClientInputSender inputSender))
            {
                Debug.LogError("ClientInputSender not found on ClientPlayerAvatar!", clientPlayerAvatar);
            }

            if (m_InputSender != null)
            {
                Debug.LogWarning($"Multiple ClientInputSenders in scene? Discarding sender belonging to {m_InputSender.gameObject.name} and adding it for {inputSender.gameObject.name} ");
            }

            m_InputSender = inputSender;
            m_ServerCharacter = m_InputSender.GetComponent<ServerCharacter>();
            m_ServerCharacter.TargetId.OnValueChanged += OnSelectionChanged;
            m_ServerCharacter.HeldNetworkObject.OnValueChanged += OnHeldNetworkObjectChanged;
            UpdateAllActionButtons();
        }

        void OnHeldNetworkObjectChanged(ulong previousValue, ulong newValue)
        {
            UpdateAllActionButtons();
        }

        void DeregisterInputSender()
        {
            m_InputSender = null;
            if (m_ServerCharacter != null)
            {
                m_ServerCharacter.TargetId.OnValueChanged -= OnSelectionChanged;
                m_ServerCharacter.HeldNetworkObject.OnValueChanged -= OnHeldNetworkObjectChanged;
            }
            m_ServerCharacter = null;
        }

        void Awake()
        {
            m_ButtonInfo = new Dictionary<ActionButtonType, ActionButtonInfo>()
            {
                [ActionButtonType.BasicAction] = new ActionButtonInfo(ActionButtonType.BasicAction, m_BasicActionButton, this),
                [ActionButtonType.Special1] = new ActionButtonInfo(ActionButtonType.Special1, m_SpecialAction1Button, this),
                [ActionButtonType.Special2] = new ActionButtonInfo(ActionButtonType.Special2, m_SpecialAction2Button, this),
                [ActionButtonType.EmoteBar] = new ActionButtonInfo(ActionButtonType.EmoteBar, m_EmoteBarButton, this),
            };

            ClientPlayerAvatar.LocalClientSpawned += RegisterInputSender;
            ClientPlayerAvatar.LocalClientDespawned += DeregisterInputSender;
        }

        void OnEnable()
        {
            foreach (ActionButtonInfo buttonInfo in m_ButtonInfo.Values)
            {
                buttonInfo.RegisterEventHandlers();
            }
        }

        void OnDisable()
        {
            foreach (ActionButtonInfo buttonInfo in m_ButtonInfo.Values)
            {
                buttonInfo.UnregisterEventHandlers();
            }
        }

        void OnDestroy()
        {
            DeregisterInputSender();

            ClientPlayerAvatar.LocalClientSpawned -= RegisterInputSender;
            ClientPlayerAvatar.LocalClientDespawned -= DeregisterInputSender;
        }

        void Update()
        {
            // If we have another player selected, see if their aliveness state has changed,
            // and if so, update the interactiveness of the basic-action button

            if (UnityEngine.Input.GetKeyUp(KeyCode.Alpha4))
            {
                m_ButtonInfo[ActionButtonType.EmoteBar].Button.OnPointerUpEvent.Invoke();
            }

            if (!m_SelectedPlayerServerCharacter) { return; }

            bool isAliveNow = m_SelectedPlayerServerCharacter.NetLifeState.LifeState.Value == LifeState.Alive;
            if (isAliveNow != m_WasSelectedPlayerAliveDuringLastUpdate)
            {
                // this will update the icons so that the basic-action button's interactiveness is correct
                UpdateAllActionButtons();
            }

            m_WasSelectedPlayerAliveDuringLastUpdate = isAliveNow;
        }

        void OnSelectionChanged(ulong oldSelectionNetworkId, ulong newSelectionNetworkId)
        {
            UpdateAllActionButtons();
        }

        void OnButtonClickedDown(ActionButtonType buttonType)
        {
            if (buttonType == ActionButtonType.EmoteBar)
            {
                return; // this is the "emote" button; we won't do anything until they let go of the button
            }

            if (m_InputSender == null)
            {
                //nothing to do past this point if we don't have an InputSender.
                return;
            }

            // send input to begin the action associated with this button
            m_InputSender.RequestAction(m_ButtonInfo[buttonType].CurAction, ClientInputSender.SkillTriggerStyle.UI);
        }

        void OnButtonClickedUp(ActionButtonType buttonType)
        {
            if (buttonType == ActionButtonType.EmoteBar)
            {
                m_EmotePanel.SetActive(!m_EmotePanel.activeSelf);
                return;
            }

            if (m_InputSender == null)
            {
                //nothing to do past this point if we don't have an InputSender.
                return;
            }

            // send input to complete the action associated with this button
            m_InputSender.RequestAction(m_ButtonInfo[buttonType].CurAction, ClientInputSender.SkillTriggerStyle.UIRelease);
        }

        /// <summary>
        /// Updates all the action buttons and caches info about the currently-selected entity (when appropriate):
        /// stores info in m_SelectedPlayerNetState and m_WasSelectedPlayerAliveDuringLastUpdate
        /// </summary>
        void UpdateAllActionButtons()
        {
            UpdateActionButton(m_ButtonInfo[ActionButtonType.BasicAction], m_ServerCharacter.CharacterClass.Skill1);
            UpdateActionButton(m_ButtonInfo[ActionButtonType.Special1], m_ServerCharacter.CharacterClass.Skill2);
            UpdateActionButton(m_ButtonInfo[ActionButtonType.Special2], m_ServerCharacter.CharacterClass.Skill3);

            var isHoldingNetworkObject =
                NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(m_ServerCharacter.HeldNetworkObject.Value,
                    out var heldNetworkObject);

            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(m_ServerCharacter.TargetId.Value,
                out var selection);

            if (isHoldingNetworkObject)
            {
                // show drop!
                UpdateActionButton(m_ButtonInfo[ActionButtonType.BasicAction], GameDataSource.Instance.DropActionPrototype, true);
            }
            if ((m_ServerCharacter.TargetId.Value != 0
                    && selection != null
                    && selection.TryGetComponent(out PickUpState pickUpState))
               )
            {
                // special case: targeting a pickup-able item or holding a pickup object
                UpdateActionButton(m_ButtonInfo[ActionButtonType.BasicAction], GameDataSource.Instance.PickUpActionPrototype, true);
            }
            else if (m_ServerCharacter.TargetId.Value != 0
                && selection != null
                && selection.NetworkObjectId != m_ServerCharacter.NetworkObjectId
                && selection.TryGetComponent(out ServerCharacter charState)
                && !charState.IsNpc)
            {
                // special case: when we have a player selected, we change the meaning of the basic action
                // we have another player selected! In that case we want to reflect that our basic Action is a Revive, not an attack!
                // But we need to know if the player is alive... if so, the button should be disabled (for better player communication)

                bool isAlive = charState.NetLifeState.LifeState.Value == LifeState.Alive;
                UpdateActionButton(m_ButtonInfo[ActionButtonType.BasicAction], GameDataSource.Instance.ReviveActionPrototype, !isAlive);

                // we'll continue to monitor our selected player every frame to see if their life-state changes.
                m_SelectedPlayerServerCharacter = charState;
                m_WasSelectedPlayerAliveDuringLastUpdate = isAlive;
            }
            else
            {
                m_SelectedPlayerServerCharacter = null;
                m_WasSelectedPlayerAliveDuringLastUpdate = false;
            }
        }

        void UpdateActionButton(ActionButtonInfo buttonInfo, Action action, bool isClickable = true)
        {
            // first find the info we need (sprite and description)
            Sprite sprite = null;
            string description = "";

            if (action != null)
            {
                sprite = action.Config.Icon;
                description = action.Config.Description;
            }

            // set up UI elements appropriately
            if (sprite == null)
            {
                buttonInfo.Button.gameObject.SetActive(false);
            }
            else
            {
                buttonInfo.Button.gameObject.SetActive(true);
                buttonInfo.Button.interactable = isClickable;
                buttonInfo.Button.image.sprite = sprite;
                buttonInfo.Tooltip.SetText(description);
            }

            // store the action type so that we can retrieve it in click events
            buttonInfo.CurAction = action;
        }
    }
}