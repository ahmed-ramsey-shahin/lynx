using System.Text.Json.Serialization;
using Lynx.IdentityService.Domain.Common.Results.Abstractions;

namespace Lynx.IdentityService.Domain.Common.Results
{
    public readonly record struct Success;
    public readonly record struct Created;
    public readonly record struct Updated;
    public readonly record struct Deleted;

    public static class Result
    {
        public static Success Success => default;
        public static Created Created => default;
        public static Updated Updated => default;
        public static Deleted Deleted => default;
    }

    public sealed class Result<TValue> : IResult<TValue>
    {
        private readonly TValue? _value;
        private readonly List<Error>? _errors;

        public TValue Value => IsSuccess ? _value! : default!;

        public List<Error>? Errors => IsSuccess ? [] : _errors;

        public bool IsSuccess { get; }

        public bool IsError => !IsSuccess;

#pragma warning disable IDE0051
        [JsonConstructor]
        private Result(TValue? value, List<Error>? errors, bool isSuccess)
        {
            IsSuccess = isSuccess;

            if (isSuccess)
            {
                ArgumentNullException.ThrowIfNull(value);
                _value = value;
                _errors = [];
            }
            else
            {
                ArgumentNullException.ThrowIfNull(errors);

                if (errors.Count == 0)
                {
                    throw new ArgumentException("Provide at least one error.", nameof(errors));
                }

                _errors = errors;
                _value = default;
            }
        }
#pragma warning restore IDE0051

        private Result(Error error)
        {
            _errors = [error];
            IsSuccess = false;
        }

        private Result(List<Error> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);

            if (errors.Count == 0)
            {
                throw new ArgumentException("Provide at least one error.", nameof(errors));
            }

            _errors = errors;
            IsSuccess = false;
        }

        private Result(TValue value)
        {
            ArgumentNullException.ThrowIfNull(value);
            IsSuccess = true;
            _value = value;
        }

        public static implicit operator Result<TValue>(TValue value) => new(value);
        public static implicit operator Result<TValue>(Error error) => new(error);
        public static implicit operator Result<TValue>(List<Error> errors) => new(errors);
        public TNextValue Match<TNextValue>(Func<TValue, TNextValue> onValue, Func<List<Error>, TNextValue> onError) => IsSuccess ? onValue(Value!) : onError(Errors!);
    }
}
