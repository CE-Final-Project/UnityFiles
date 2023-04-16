using Script.Game.Actions.Input;
using Script.Game.GameplayObject.Character;
using Script.Game.GameplayObject.RuntimeDataContainers;
using Script.Game.GameplayObject.UserInput;
using Shaders;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Script.Game.Actions.ConcreteActions
{
    public partial class TargetAction
    {
        private GameObject m_TargetReticule;
        private ulong m_CurrentTarget;
        private ulong m_NewTarget;

        private const float k_ReticuleGroundHeight = 0.2f;

        public override bool OnStartClient(ClientCharacter clientCharacter)
        {
            base.OnStartClient(clientCharacter);
            clientCharacter.ServerCharacter.TargetId.OnValueChanged += OnTargetChanged;
            clientCharacter.ServerCharacter.GetComponent<ClientInputSender>().ActionInputEvent += OnActionInput;

            return true;
        }

        private void OnTargetChanged(ulong oldTarget, ulong newTarget)
        {
            m_NewTarget = newTarget;
        }

        public override bool OnUpdateClient(ClientCharacter clientCharacter)
        {
            if (m_CurrentTarget != m_NewTarget)
            {
                m_CurrentTarget = m_NewTarget;

                if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(m_CurrentTarget, out NetworkObject targetObject))
                {
                    var targetEntity = targetObject != null ? targetObject.GetComponent<ITargetable>() : null;
                    if (targetEntity != null)
                    {
                        ValidateReticule(clientCharacter, targetObject);
                        m_TargetReticule.SetActive(true);

                        var parentTransform = targetObject.transform;
                        if (targetObject.TryGetComponent(out ServerCharacter serverCharacter) && serverCharacter.ClientCharacter)
                        {
                            //for characters, attach the reticule to the child graphics object.
                            parentTransform = serverCharacter.ClientCharacter.transform;
                        }

                        m_TargetReticule.transform.parent = parentTransform;
                        m_TargetReticule.transform.localPosition = new Vector3(0, k_ReticuleGroundHeight, 0);
                    }
                }
                else
                {
                    // null check here in case the target was destroyed along with the target reticule
                    if (m_TargetReticule != null)
                    {
                        m_TargetReticule.transform.parent = null;
                        m_TargetReticule.SetActive(false);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Ensures that the TargetReticule GameObject exists. This must be done prior to enabling it because it can be destroyed
        /// "accidentally" if its parent is destroyed while it is detached.
        /// </summary>
        void ValidateReticule(ClientCharacter parent, NetworkObject targetObject)
        {
            
            if (m_TargetReticule == null)
            {
                m_TargetReticule = Instantiate(parent.TargetReticulePrefab);
            }
            
            bool target_isnpc = targetObject.GetComponent<ITargetable>().IsNpc;
            bool myself_isnpc = parent.ServerCharacter.CharacterClass.IsNpc;
            bool hostile = target_isnpc != myself_isnpc;


            SpriteOutline so = m_TargetReticule.GetComponent<SpriteOutline>();
            so.color = hostile ? Color.red : Color.green;
            so.outlineSize = 1;
        }

        public override void CancelClient(ClientCharacter clientCharacter)
        {
            Destroy(m_TargetReticule);

            clientCharacter.ServerCharacter.TargetId.OnValueChanged -= OnTargetChanged;
            if (clientCharacter.TryGetComponent(out ClientInputSender inputSender))
            {
                inputSender.ActionInputEvent -= OnActionInput;
            }
        }

        private void OnActionInput(ActionRequestData data)
        {
            //this method runs on the owning client, and allows us to anticipate our new target for purposes of FX visualization.
            if (GameDataSource.Instance.GetActionPrototypeByID(data.ActionID).IsGeneralTargetAction)
            {
                m_NewTarget = data.TargetIDs[0];
            }
        }
    }
}
