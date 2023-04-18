using System;
using UnityEngine;

namespace Script.Game.GameplayObject.AOEs
{
    public class FXAoe : MonoBehaviour
    {
        
        private float m_Duration = 1.1f; // default duration
        
        public void Initialize(float duration)
        {
            m_Duration = duration;
        }


        private void Update()
        {
            m_Duration -= Time.deltaTime;
            if (m_Duration <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}