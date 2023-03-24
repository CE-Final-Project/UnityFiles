using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : NetworkBehaviour
{
    [SerializeField] public Transform target;
    NavMeshAgent agent;
    GameObject[] players;

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
        agent = GetComponent<NavMeshAgent>();
		agent.updateRotation = false;
		agent.updateUpAxis = false;
        players = GameObject.FindGameObjectsWithTag("Player");
        
    }

    private void Update()
    {
        /* Make enemy move toward player แบบโคตร Basic 
        if (player != null)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }*/
        //agent.SetDestination(players[0].transform.position);
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
