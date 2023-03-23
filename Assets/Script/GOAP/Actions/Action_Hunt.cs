using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Action_Hunt : Action_Base
{
    List<System.Type> SupportedGoals = new List<System.Type>(new System.Type[] { typeof(Goal_Hunt) });

    Goal_Hunt HuntGoal;

    NavMeshAgent agent;
    GameObject[] players;




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
        HuntGoal = (Goal_Hunt)LinkedGoal;

        //Actions Here
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        players = GameObject.FindGameObjectsWithTag("Player");

    }

    public override void OnDeactivated()
    {
        base.OnDeactivated();

        HuntGoal = null;

        agent = null;

        Debug.Log("STOP HUNTING");
    }

    public override void OnTick()
    {
        //Actions Here
        agent.SetDestination(players[0].transform.position);
        //Debug.Log("HUNTING");
    }
}
