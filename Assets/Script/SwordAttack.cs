using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public float damage = 3;
    Vector2 rightAttackOffset;
    public Collider2D swordColider;
    
    private void Start() {
        rightAttackOffset = transform.position;
    }
    
    public void AttackRight() {
        swordColider.enabled = true;
        transform.localPosition = rightAttackOffset;
    }
    
    public void AttackLeft() { 
        swordColider.enabled = true;
        transform.localPosition = new Vector2(rightAttackOffset.x * -1, rightAttackOffset.y);
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
