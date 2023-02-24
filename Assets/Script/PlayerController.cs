using System.Collections.Generic;
using Cinemachine;
using Script.Networks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Script
{
    public class PlayerController : NetworkBehaviour
    {
    
        public pauseMenu pausemenu;
    
        [Header("Movement Settings")]
        [SerializeField]
        public float moveSpeed = 1f;
    
        public HealthBar healthBar;
    
        public float collisionOffset = 0.05f;
    
        public Animator animator;

    

        public int MaxPlayerHealth = 10;
        [Header("Player Settings")]
        [SerializeField]
        private NetworkVariable<int> CurrentPlayerHealth = new NetworkVariable<int>(10);

        private NetworkVariable<int> CurrentPlayerPower = new NetworkVariable<int>(0);
    
        private NetworkVariable<bool> IsSpliteFlipped = new NetworkVariable<bool>(false, default, NetworkVariableWritePermission.Owner);

        public ContactFilter2D movementFilter;
    
        public SwordAttack swordAttack;
    
        public Rigidbody2D rb;
    
        public SpriteRenderer spriteRenderer;

        [Header("Camera Settings")]
        [SerializeField] public GameObject virtualCameraPrefab;
    
        private GameObject virtualCamera;

        Vector2 movementInput;
        
        List<RaycastHit2D> castCollosions = new List<RaycastHit2D>();
        bool canMove = true;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                Destroy(GetComponent<PlayerInput>());
            }
            IsSpliteFlipped.OnValueChanged += OnIsSpliteFlippedChanged;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            SceneTransitionHandler.Instance.OnClientLoadedScene += OnClientLoadedScene;
            
            // randomize position player
            transform.position = new Vector3(Random.Range(-2f, 0f), Random.Range(0.1f, 1f), 0);
        }

        private void OnClientLoadedScene(ulong clientid)
        {
            if (!IsHost) return;
            virtualCamera = Instantiate(virtualCameraPrefab);
            virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = transform;
        }

        private void OnClientConnected(ulong obj)
        {
            if (IsOwner)
            {
                IsSpliteFlipped.Value = spriteRenderer.flipX;
                if (IsHost) return;
                virtualCamera = Instantiate(virtualCameraPrefab);
                virtualCamera.GetComponent<CinemachineVirtualCamera>().Follow = transform;
            }
            else
            {
                spriteRenderer.flipX = IsSpliteFlipped.Value;
                Destroy(virtualCamera);
            }
        }
    
        public override void OnNetworkDespawn()
        {
            IsSpliteFlipped.OnValueChanged -= OnIsSpliteFlippedChanged;
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            SceneTransitionHandler.Instance.OnClientLoadedScene -= OnClientLoadedScene;
            Destroy(virtualCamera);
        }

        // Start is called before the first frame update
        void Start()
        {
            CurrentPlayerHealth.Value = MaxPlayerHealth;

            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            // healthBar.SetMaxHealth(MaxPlayerHealth);
            if (IsOwner)
            {
                IsSpliteFlipped.Value = spriteRenderer.flipX;
            } else {
                spriteRenderer.flipX = IsSpliteFlipped.Value;
            }
        }
    
        private void OnIsSpliteFlippedChanged(bool previousValue, bool newValue)
        {
            spriteRenderer.flipX = newValue;
        }

        private void FixedUpdate()
        {
            //comment this when play on editor
            //also character didn't flip when move left, right
            if (!IsOwner) return;

            if (canMove)
            {
                // IF movement input is not 0, try to move
                if (movementInput != Vector2.zero)
                {
                    bool success = TryMove(movementInput);

                    if (!success)
                    {
                        success = TryMove(new Vector2(movementInput.x, 0));
                    }
                    if (!success)
                    {
                        success = TryMove(new Vector2(0, movementInput.y));
                    }

                    animator.SetBool("isMoving", success);
                }
                else
                {
                    animator.SetBool("isMoving", false);
                }
            
                // Set sprite direction
                if (movementInput.x < 0)
                {
                    IsSpliteFlipped.Value = true;
                }
                else if (movementInput.x > 0)
                {
                    IsSpliteFlipped.Value = false;
                }


                // Set player health
                if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    CurrentPlayerHealth.Value -= 1;
                    healthBar.SetHealth(CurrentPlayerHealth.Value);
                }
                if (Input.GetKeyDown(KeyCode.R))
                {
                    CurrentPlayerHealth.Value += 1;
                    healthBar.SetHealth(CurrentPlayerHealth.Value);
                }
                if (CurrentPlayerHealth.Value <= 0)
                {
                    PlayerDead();
                    Respawn();
                }
            }
        }

        private bool TryMove(Vector2 direction)
        {
            if (direction != Vector2.zero)
            {
                int count = rb.Cast(
                    direction,
                    movementFilter,
                    castCollosions,
                    moveSpeed * Time.fixedDeltaTime + collisionOffset);

                if (count == 0)
                {
                    rb.MovePosition(rb.position + direction * (moveSpeed * Time.fixedDeltaTime));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        void OnMove(InputValue movementValue)
        {
            movementInput = movementValue.Get<Vector2>();
        }
        void OnFire()
        {
            animator.SetTrigger("swordAttack");
            print("Attack at \nX = " + transform.position.x + ", Y = " + transform.position.y);
        }
        public void SwordAttack()
        {
            LockMovement();
            if (spriteRenderer.flipX == true)
            {
                swordAttack.AttackLeft();
            }
            else
            {
                swordAttack.AttackRight();
            }
        }
        public void EndSwordAttack()
        {
            UnlockMovement();
            swordAttack.StopAttack();
        }
        public void LockMovement()
        {
            canMove = false;
            print("Movement Locked");
        }
        public void UnlockMovement()
        {
            canMove = true;
            print("Movement Unlocked");
        }
        private void PlayerDead()
        {
            print("Player Dead");
            animator.SetTrigger("dead");
        }
        private void Respawn()
        {
            animator.SetTrigger("respawn");
            addHealth();
            healthBar.SetHealth(CurrentPlayerHealth.Value);
            print("Respawn : " + CurrentPlayerHealth);
        }
        private void addHealth()
        {
            CurrentPlayerHealth.Value += 1;
        }
    }
}
