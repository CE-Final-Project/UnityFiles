using Script.Infrastructure;
using UnityEngine;

namespace Script.Game
{
    public class CountDown : MonoBehaviour
    {
        private CallbackValue<float> _timeLeft = new CallbackValue<float>();
        
        private const int CountDownTime = 4;

        public void OnEnable()
        {
            // _timeLeft.OnChanged += OnTimeChanged;
            _timeLeft.Value = -1;
        }
        
        public void StartCountDown()
        {
            _timeLeft.Value = CountDownTime;
        }
        
        public void CancelCountDown()
        {
            _timeLeft.Value = -1;
        }
        
        public void Update()
        {
            if (_timeLeft.Value < 0)
                return;
            _timeLeft.Value -= Time.deltaTime;
            // if (_timeLeft.Value < 0)
                // GameManager.Instance.FinishedCountDown();
        }
    }
}