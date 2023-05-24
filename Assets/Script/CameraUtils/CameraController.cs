using Cinemachine;
using UnityEngine;

namespace Script.CameraUtils
{
    public class CameraController : MonoBehaviour
    {
        private CinemachineVirtualCamera _mainCamera;

        private void Start()
        {
            AttachCamera();
        }
        
        private void AttachCamera()
        {
            _mainCamera = FindObjectOfType<CinemachineVirtualCamera>();
            if (_mainCamera)
            {
                // camera body / aim
                Transform transform1 = transform;
                _mainCamera.Follow = transform1;
                _mainCamera.LookAt = transform1;
            }
        }
    }
}