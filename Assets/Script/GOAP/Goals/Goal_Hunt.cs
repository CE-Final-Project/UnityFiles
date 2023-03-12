using Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal_Hunt : Goal_Base
{
    [SerializeField] int CurrentPriority = 100;
    GameObject[] players;
    GameObject enemySpawner;
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
        if(Vector2.Distance(players[0].transform.position,enemySpawner.GetComponentsInChildren<Transform>()[3].position) <= 3)
        {
            return true;
        }
        return false;
    }

    public override void OnGoalActivated(Action_Base _linkedAction)
    {
        LinkedAction = _linkedAction;
        //Debug.Log("GOAL ACTIVATED");

        
    }

}
