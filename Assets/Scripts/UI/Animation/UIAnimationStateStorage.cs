using System;
using System.Collections.Generic;

namespace MRK.UI.Animation
{
    public class UIAnimationStateStorage
    {
        private readonly List<UIAnimationState> _animationStates;
        private UIAnimationState _currentState;
        private bool _isBackup;

        public UIAnimationStateStorage()
        {
            _animationStates = new List<UIAnimationState>();
        }

        public void SetStorageMode(bool backup)
        {
            _isBackup = backup;
        }

        public void Begin(string state)
        {
            //we call GetState for both anyway
            var proxyState = GetState(state);

            if (_isBackup)
            {
                if (proxyState != null)
                {
                    throw new Exception($"State {state} already exists");
                }

                _currentState = new UIAnimationState(state);
                _animationStates.Add(_currentState);
                return;
            }

            _currentState = proxyState;
            if (_currentState == null)
            {
                throw new Exception($"State {state} doesnt exist");
            }
        }

        public T Get<T>(string key)
        {
            if (_currentState == null)
            {
                throw new Exception("Current state is null");
            }

            return _currentState.Get<T>(key);
        }

        public void Set<T>(string key, T val)
        {
            _currentState.Set(key, val);
        }

        private UIAnimationState GetState(string state)
        {
            return _animationStates.Find(s => s.State == state);
        }
    }
}
