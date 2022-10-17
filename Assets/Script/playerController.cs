using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour {

    public HealthBar healthBar;
    public pauseMenu pausemenu;
    public float moveSpeed = 1f;
    public float collisionOffset = 0.05f;
    public float MaxPlayerHealth = 10;
    public float CurrentPlayerHealth;
    public ContactFilter2D movementFilter;
    public SwordAttack swordAttack;
    
    Vector2 movementInput;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;

    Animator animator;

    List<RaycastHit2D> castCollosions = new List<RaycastHit2D>();
    bool canMove = true;

    // Start is called before the first frame update
    void Start()
    {
        CurrentPlayerHealth = MaxPlayerHealth;
        healthBar.SetMaxHealth(MaxPlayerHealth);
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void FixedUpdate() {
        if(canMove) {
            // IF movement input is not 0, try to move
            if(movementInput != Vector2.zero) {
                bool success = TryMove(movementInput);

                if(!success) {
                    success = TryMove(new Vector2(movementInput.x, 0));
                }
                if(!success) {
                    success = TryMove(new Vector2(0, movementInput.y));
                }

                animator.SetBool("isMoving", success);
            } else {
                animator.SetBool("isMoving", false);
            }

            // Set sprite direction
            if(movementInput.x < 0) {
                spriteRenderer.flipX = true;
            } else if (movementInput.x > 0) {
                spriteRenderer.flipX = false;
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                CurrentPlayerHealth -= 1;
                healthBar.SetHealth(CurrentPlayerHealth);
            }
            if(Input.GetKeyDown(KeyCode.R))
            {
                CurrentPlayerHealth += 1;
                healthBar.SetHealth(CurrentPlayerHealth);
            }
        }
    }

    private bool TryMove(Vector2 direction) {
        if(direction != Vector2.zero) {
            int count = rb.Cast(
            direction, 
            movementFilter, 
            castCollosions, 
            moveSpeed * Time.fixedDeltaTime + collisionOffset);
            
            if(count == 0) {
                rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
                return true;
            }
            else {
                return false;
            }
        } else {
            return false;
        }
    }

    void OnMove(InputValue movementValue) {
        movementInput = movementValue.Get<Vector2>();
    }
    void OnFire() {
        animator.SetTrigger("swordAttack");
        print("Attack at \nX = " + transform.position.x + ", Y = " + transform.position.y);
    }
    public void SwordAttack() {
        LockMovement();
        if(spriteRenderer.flipX == true) {
            swordAttack.AttackLeft();
        } else {
            swordAttack.AttackRight();
        }
    }
    public void EndSwordAttack() {
        UnlockMovement();
        swordAttack.StopAttack();
    }
    public void LockMovement() {
        canMove = false;
    }
    public void UnlockMovement() {
        canMove = true;
    }
}
