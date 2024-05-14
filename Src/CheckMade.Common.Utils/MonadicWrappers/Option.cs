namespace CheckMade.Common.Utils.MonadicWrappers
{
    public record Option<T>
    {
        private readonly T? _value;
        private readonly bool _hasValue;

        private Option(T value)
        {
            _value = value;
            _hasValue = true;
        }

        private Option()
        {
            _value = default;
            _hasValue = false;
        }

        public static Option<T> Some(T value) => new Option<T>(value);

        public static Option<T> None() => new Option<T>();

        public bool IsSome => _hasValue;
        public bool IsNone => !_hasValue;

        public void Deconstruct(out bool hasValue, out T? value)
        {
            hasValue = _hasValue;
            value = _value;
        }

        public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone)
        {
            return _hasValue ? onSome(_value!) : onNone();
        }

        public T GetValueOrDefault(T defaultValue = default!)
        {
            return _hasValue ? _value! : defaultValue;
        }

        public T GetValueOrThrow()
        {
            if (_hasValue)
            {
                return _value!;
            }
            throw new InvalidOperationException("No value present");
        }

        // Implicit conversion from T to Option<T>
        public static implicit operator Option<T>(T value) => Some(value);
    }
}