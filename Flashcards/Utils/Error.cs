namespace Flashcards.Utils;
public record Error
{
    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    public static readonly Error None = new Error(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new Error("General.Null", "Null value was provided", ErrorType.Failure);
    public static readonly Error General = new Error("General.Error", "An unexpected error occurred", ErrorType.Failure);

    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error Problem(string code, string description) => new(code, description, ErrorType.Problem);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
}
