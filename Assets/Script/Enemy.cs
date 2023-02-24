using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Animator animator;
    public float Health {
        set {
            health = value;
            if(health <= 0) {
                Defeated();
            }
        }
        get {
            return health;
        }
    }
    
    public float health = 1;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    public void Defeated() {
        print("Defeated Enemy");
        animator.SetTrigger("Defeated");
    }

    public void RemoveEnemy() {
        Destroy(gameObject);
    }
}
