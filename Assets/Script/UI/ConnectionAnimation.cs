using UnityEngine;
using UnityEngine.UI;

namespace Script.UI
{
    /// <summary>
    /// A Temporary animation script that rotates the image on the game
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ConnectionAnimation : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed;

        private void Update()
        {
            transform.Rotate(new Vector3(0, 0, rotationSpeed * Mathf.PI * Time.deltaTime));
        }
    }
}