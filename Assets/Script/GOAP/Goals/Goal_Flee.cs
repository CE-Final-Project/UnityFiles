using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_Flee : Goal_Base
{
    [SerializeField] int CurrentPriority = 100;
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

        return CurrentPriority;
    }

    public override bool CanRun()
    {
        //Pre condition
        if (gameObject.GetComponent<Enemy>().health < 3)
        {
            return true;
            Debug.Log("CAN FLEE");
            
        }

        return false;
    }

    public override void OnGoalActivated(Action_Base _linkedAction)
    {
        LinkedAction = _linkedAction;
        //Debug.Log("GOAL ACTIVATED");
        Debug.Log("Low HP");


    }

    public override void OnGoalDeactivated()
    {
        LinkedAction = null;

        nearestPlayer = null;
    }
}
