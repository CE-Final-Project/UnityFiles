using Script;
using UnityEngine;

public class Goal_Idle : Goal_Base
{
    [SerializeField] int CurrentPriority = 100;
    GameObject[] players;
    // Start is called before the first frame update

    public void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
    }
    public override int CalculatePriority()
    {
        return players[0].GetComponent<PlayerController>().MaxPlayerHealth;
    }

    public override bool CanRun()
    {
        //Pre condition
        return true;
    }

    public override void OnGoalActivated(Action_Base _linkedAction)
    {
        LinkedAction = _linkedAction;
        //Debug.Log("GOAL ACTIVATED");
    }
}
