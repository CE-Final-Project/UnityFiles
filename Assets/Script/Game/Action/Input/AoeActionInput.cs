using Script.Game.GameplayObject.RuntimeDataContainers;
using UnityEngine;
using UnityEngine.AI;

namespace Script.Game.Action.Input
{
    public class AoeActionInput : BaseActionInput
    {
        [SerializeField] private GameObject inRangeVisualization;
        [SerializeField] private GameObject outOfRangeVisualization;

        private Camera _camera;
        
        private bool _receivedMouseDownEvent;

        private NavMeshHit _navMeshHit;
        
        private static readonly Plane Plane = new Plane(Vector3.up, 0);
        
        private void Start()
        {
            float redius = GameDataSource.Instance.GetActionPrototypeByID(ActionPrototypeID).Config.Radius;
            
            transform.localScale = new Vector3(redius * 2, redius * 2, redius * 2);
            _camera = Camera.main;
        }
    }
}