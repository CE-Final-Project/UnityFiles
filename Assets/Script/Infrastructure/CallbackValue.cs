using System;

namespace Script.Infrastructure
{
    public class CallbackValue<T>
    {
        public Action<T> OnChanged;
        
        public CallbackValue()
        {
            
        }
        
        public CallbackValue(T cachedValue)
        {
            _cachedValue = cachedValue;
        }

        public T Value
        {
            get => _cachedValue;
            set
            {
                if (_cachedValue != null && _cachedValue.Equals(value))
                    return;
                
                _cachedValue = value;
                
                OnChanged?.Invoke(_cachedValue);
            }
        }

        private T _cachedValue = default;

        public void ForceSet(T value)
        {
            _cachedValue = value;
            OnChanged?.Invoke(_cachedValue);
        }
        
        public void SetNoCallback(T value)
        {
            _cachedValue = value;
        }
    }
}