using System;
using System.Collections.Generic;

namespace Script.Infrastructure
{
    public class DisposableGroup : IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
        }

        public void Add(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }
    }
}
