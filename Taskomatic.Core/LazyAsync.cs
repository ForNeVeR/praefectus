using System;
using System.Threading.Tasks;
using ReactiveUI;

namespace Taskomatic.Core
{
    public class LazyAsync<T> : ReactiveObject
    {
        private readonly Func<Task<T>> _factory;
        private readonly T _defaultValue;
        private T _value;

        public LazyAsync(Func<Task<T>> factory, T defaultValue)
        {
            _factory = factory;
            _value = _defaultValue = defaultValue;
        }

        public T Value
        {
            get
            {
                if (Equals(_value, _defaultValue))
                {
                    TriggerAsyncLoad();
                }

                return _value;
            }
            set
            {
                _value = value;
                this.RaisePropertyChanged();
            }
        }

        private async void TriggerAsyncLoad()
        {
            var value = await _factory();
            Value = value;
        }
    }
}
