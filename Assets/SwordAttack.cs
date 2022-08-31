using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
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
}
