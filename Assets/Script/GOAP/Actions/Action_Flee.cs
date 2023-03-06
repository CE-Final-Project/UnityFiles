using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Action_Flee : Action_Base
{
    List<System.Type> SupportedGoals = new List<System.Type>(new System.Type[] { typeof(Goal_Flee) });

    Goal_Flee FleeGoal;

    NavMeshAgent agent;
    GameObject[] players;
    GameObject enemySpawner;




    public override List<System.Type> GetSupportedGoals()
    {
        return SupportedGoals;
    }

    public override float GetCost()
    {
        return 1.0f;
    }

    public override void OnActivated(Goal_Base _linkedGoal)
    {
        base.OnActivated(_linkedGoal);

        // cache the chase goal
        FleeGoal = (Goal_Flee)LinkedGoal;

        //Actions Here
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        players = GameObject.FindGameObjectsWithTag("Player");
        enemySpawner = GameObject.FindGameObjectWithTag("SpawnerPosition");


    }

    public override void OnDeactivated()
    {
        base.OnDeactivated();

        FleeGoal = null;
    }

    public override void OnTick()
    {
        //Actions Here
        agent.SetDestination(enemySpawner.transform.position);
        //Debug.Log("IDLING");
    }
}