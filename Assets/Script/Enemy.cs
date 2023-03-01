using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    //[SerializeField] private float moveSpeed = 5f;
    Animator animator;
    private GameObject player;
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

    private void Update()
    {
        /* Make enemy move toward player แบบโคตร Basic 
        if (player != null)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }*/
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }
    }

    public void Defeated() {
        print("Defeated Enemy");
        animator.SetTrigger("Defeated");
    }

    public void RemoveEnemy() {
        Destroy(gameObject);
    }
}
