using UnityEngine;

public class Goal_Hunt : Goal_Base
{
    [SerializeField] int CurrentPriority = 1;
    GameObject[] players;
    GameObject enemySpawner;
    public GameObject nearestPlayer;
    // Start is called before the first frame update

    public void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        enemySpawner = GameObject.FindGameObjectWithTag("SpawnerPosition");
    }
    public override int CalculatePriority()
    {
        
        return 1;
    }

    public override bool CanRun()
    {
        //Pre condition
        nearestPlayer = GetClosestPlayer(players);
        if (Vector2.Distance(nearestPlayer.transform.position, transform.position) <= 1)  {
            return true; 
        }

        return false;
    }

    public override void OnGoalActivated(Action_Base _linkedAction)
    {
        LinkedAction = _linkedAction;
        //Debug.Log("GOAL ACTIVATED");

        
    }

    public override void OnGoalDeactivated()
    {
        LinkedAction = null;

        nearestPlayer = null;
    }

    GameObject GetClosestPlayer(GameObject[] players)
    {
        GameObject tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (GameObject t in players)
        {
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }

}
