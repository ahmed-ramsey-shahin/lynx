namespace Lynx.RedirectionService.Domain.Common.Results.Abstractions
{
    public interface IResult
    {
        List<Error>? Errors { get; }
        bool IsSuccess { get; }
        bool IsError { get; }
    }

    public interface IResult<TValue> : IResult
    {
        TValue Value { get; }
    }
}
