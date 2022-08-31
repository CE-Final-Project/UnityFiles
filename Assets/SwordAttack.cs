using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public float damage = 3;
    Vector2 rightattackOffset;
    Collider2D swordColider;
    
    private void Start() {
        swordColider = GetComponent<Collider2D>();
        rightattackOffset = transform.position;
    }
    
    public void AttackRight() {
        print("ATK RIGHT");
        swordColider.enabled = true;
        transform.position = rightattackOffset;
    }
    
    public void AttackLeft() {
        print("ATK LEFT");
        swordColider.enabled = true;
        transform.position = new Vector3(rightattackOffset.x * -1, rightattackOffset.y);
    }
    
    public void StopAttack() {
        swordColider.enabled = false;
    }
    
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Enemy") {
            Enemy enemy = other.GetComponent<Enemy>();

            if(enemy != null) {
                enemy.Health -= damage;
            }
        }
    }
}
