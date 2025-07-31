using ResultMonad;

namespace ResultMonad;

/// <summary>
/// Represents an error code for a result. Use this record to define and share error codes across your application.
/// These types are returned as a prop value in ResultFailure.ResultErrorCode.
/// Use these static types to pattern match on in result handling logic.
/// </summary>
/// <example>
/// <code>
/// // Define own error codes by creating a new instance of ResultErrorCode.
/// public static readonly ResultErrorCode MyCustomError = new("MyCustomError");
/// 
/// // consume them in result handling logic.
/// var result = ValidateInput("bob", "password123")
///             .Then(user => CheckUserDoesNotExist(user))
///             .Then(user => HashPassword(user))
///             .Then(user => SaveUser(user))
///             .Then(user => SendWelcomeEmail(user))
///             .Then(user => new ResultSuccess<string>($"User {user.Username} registered successfully!", "done"));
///
/// // pattern match on the error code in result handling logic.
/// switch (result)
/// {
///     case ResultFailure<string> failure when failure.ResultErrorCode == ResultErrorCode.ValidationFailed:
///         // Handle validation failure
///         break;
///     case ResultFailure<string> failure when failure.ResultErrorCode == ResultErrorCode.NotFound:
///         // Handle not found
///         break;
///     case ResultFailure<string> failure:
///         // Handle other failures
///         break;
///     case ResultSuccess<string> success:
///         // Handle success
///         break;
/// }
/// 
/// </code>
/// </example>
public sealed record ResultErrorCode(string Code)
{
    public static readonly ResultErrorCode NotFound = new("NotFound");
    public static readonly ResultErrorCode Unauthorized = new("Unauthorized");
    public static readonly ResultErrorCode Unauthenticated = new("Unauthenticated");
    public static readonly ResultErrorCode ValidationFailed = new("ValidationFailed");
    public static readonly ResultErrorCode UnexpectedResultFailure = new("UnexpectedResultFailure");
    public static readonly ResultErrorCode UncaughtExceptionInThenFunc = new("UncaughtExceptionInThenFunc");
}