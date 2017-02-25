using System;
using System.Threading.Tasks;
using ReactiveUI;

namespace Taskomatic.Core
{
    public class LazyAsync<T> : ReactiveObject
    {
        private readonly Func<Task<T>> _factory;

        private bool _loadStarted;
        private T _value;
        private Exception _exception;

        public LazyAsync(Func<Task<T>> factory, T initialValue)
        {
            _factory = factory;
            _value = initialValue;
        }

        public T Value
        {
            get
            {
                if (!_loadStarted)
                {
                    TriggerAsyncLoad();
                }

                if (_exception != null)
                {
                    throw _exception;
                }

                return _value;
            }
            private set
            {
                _value = value;
                this.RaisePropertyChanged();
            }
        }

        private async void TriggerAsyncLoad()
        {
            _loadStarted = true;
            try
            {
                var value = await _factory();
                Value = value;
            }
            catch (Exception exception)
            {
                _exception = exception;
            }
        }
    }
}
