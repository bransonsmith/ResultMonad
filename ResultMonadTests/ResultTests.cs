using ResultMonad;

namespace ResultMonadTests;

public class ResultTests
{

    [Fact]
    public void ResultSuccess_Sets_Data_And_Message()
    {
        var result = new ResultSuccess<int>(42, "Success!");
        Assert.Equal(42, result.Data);
        Assert.Equal("Success!", result.Message);
    }

    [Fact]
    public void ResultSuccess_Defaults_Message_When_Null()
    {
        var result = new ResultSuccess<int>(100, null);
        Assert.Equal(100, result.Data);
        Assert.Equal("No message provided.", result.Message);
    }

    [Fact]
    public void ResultFailure_Sets_Properties()
    {
        var ex = new InvalidOperationException("fail");
        var result = new ResultFailure<int>("Failed", ResultErrorCode.ValidationFailed, ex);
        Assert.Equal("Failed", result.Message);
        Assert.Equal(ResultErrorCode.ValidationFailed, result.ErrorCode);
        Assert.Equal(ex, result.Exception);
    }

    [Fact]
    public void ResultFailure_Defaults_Message_When_Null()
    {
        var result = new ResultFailure<int>(null, ResultErrorCode.NotFound);
        Assert.Equal("No message provided.", result.Message);
        Assert.Equal(ResultErrorCode.NotFound, result.ErrorCode);
        Assert.Null(result.Exception);
    }

    // --- SUCCESS & FAILURE PAIRS ---

    // 1. Success: Valid func
    [Fact]
    public void Then_Success_With_Valid_Func()
    {
        var initial = new ResultSuccess<int>(10, "ok");
        var result = initial.Then(i => new ResultSuccess<string>($"Value: {i}", "done"));

        var success = Assert.IsType<ResultSuccess<string>>(result);
        Assert.Equal("Value: 10", success.Data);
        Assert.Equal("done", success.Message);
    }

    // 1. Failure: Null func
    [Fact]
    public void Then_Failure_With_Null_Func()
    {
        var initial = new ResultSuccess<int>(1, "ok");
        Result<string> result = initial.Then<int, string>(null);

        var failure = Assert.IsType<ResultFailure<string>>(result);
        Assert.Equal("The function passed to Then was null.", failure.Message);
        Assert.Equal(ResultErrorCode.UnexpectedResultFailure, failure.ErrorCode);
        Assert.Null(failure.Exception);
    }

    [Fact]
    public void Then_Success_With_Func_Returning_NonNull()
    {
        var initial = new ResultSuccess<int>(2, "ok");
        Result<string> result = initial.Then(i => new ResultSuccess<string>((i * 2).ToString(), "doubled"));

        var success = Assert.IsType<ResultSuccess<string>>(result);
        Assert.Equal("4", success.Data);
        Assert.Equal("doubled", success.Message);
    }

    [Fact]
    public void Then_Failure_With_Func_Returning_Null()
    {
        var initial = new ResultSuccess<int>(1, "ok");
        Result<string> result = initial.Then(i => null as Result<string>);

        var failure = Assert.IsType<ResultFailure<string>>(result);
        Assert.Equal("The function passed to Then returned null.", failure.Message);
        Assert.Equal(ResultErrorCode.UnexpectedResultFailure, failure.ErrorCode);
        Assert.Null(failure.Exception);
    }

    [Fact]
    public void Then_Success_With_Func_NotThrowing()
    {
        var initial = new ResultSuccess<int>(3, "ok");
        Result<string> result = initial.Then(i => new ResultSuccess<string>((i + 1).ToString(), "incremented"));

        var success = Assert.IsType<ResultSuccess<string>>(result);
        Assert.Equal("4", success.Data);
        Assert.Equal("incremented", success.Message);
    }

    [Fact]
    public void Then_Failure_With_Func_Throwing()
    {
        var initial = new ResultSuccess<int>(1, "ok");
        var ex = new InvalidOperationException("fail in func");
        Result<string> result = initial.Then<int, string>(_ => throw ex);

        var failure = Assert.IsType<ResultFailure<string>>(result);
        Assert.StartsWith("Unexpected unhandled exception caught in func used in Result.Then(<func>). ", failure.Message);
        Assert.Contains("fail in func", failure.Message);
        Assert.Equal(ResultErrorCode.UncaughtExceptionInThenFunc, failure.ErrorCode);
        Assert.Same(ex, failure.Exception);
    }

    // 4. Success: Input is ResultFailure, but for completeness, show that chaining is skipped
    [Fact]
    public void Then_Success_With_Failure_Input_Skips_Func()
    {
        var failure = new ResultFailure<int>("fail", ResultErrorCode.ValidationFailed);
        bool funcCalled = false;
        Result<string> result = failure.Then(i =>
        {
            funcCalled = true;
            return new ResultSuccess<string>("should not run");
        });

        var resultFailure = Assert.IsType<ResultFailure<string>>(result);
        Assert.Equal("fail", resultFailure.Message);
        Assert.Equal(ResultErrorCode.ValidationFailed, resultFailure.ErrorCode);
        Assert.False(funcCalled);
    }

    // 4. Failure: Input is ResultFailure, propagates failure
    [Fact]
    public void Then_Failure_With_Failure_Input()
    {
        var ex = new Exception("original");
        var failure = new ResultFailure<int>("fail", ResultErrorCode.ValidationFailed, ex);
        Result<string> result = failure.Then(i => new ResultSuccess<string>("should not run"));

        var resultFailure = Assert.IsType<ResultFailure<string>>(result);
        Assert.Equal("fail", resultFailure.Message);
        Assert.Equal(ResultErrorCode.ValidationFailed, resultFailure.ErrorCode);
        Assert.Same(ex, resultFailure.Exception);
    }

    [Fact]
    public void Then_Success_With_Known_Result_Type()
    {
        var initial = new ResultSuccess<int>(7, "ok");
        var result = initial.Then(i => new ResultSuccess<string>(i.ToString(), "converted"));

        var success = Assert.IsType<ResultSuccess<string>>(result);
        Assert.Equal("7", success.Data);
        Assert.Equal("converted", success.Message);
    }

    [Fact]
    public void Then_Failure_With_Unknown_Result_Type()
    {
        var unknown = new DummyResult<int>();
        Result<string> result = unknown.Then(i => new ResultSuccess<string>("should not run"));

        var failure = Assert.IsType<ResultFailure<string>>(result);
        Assert.Equal("Unexpected failure resolving Result.", failure.Message);
        Assert.Equal(ResultErrorCode.UnexpectedResultFailure, failure.ErrorCode);
        Assert.Null(failure.Exception);
    }


    [Fact]
    public void Then_UserRegistration_LongChain_Success()
    {
        // Simulate user registration pipeline (see these simple functions below assertions)
        var result = ValidateInput("alice", "password123")
            .Then(user => CheckUserDoesNotExist(user))
            .Then(user => HashPassword(user))
            .Then(user => SaveUser(user))
            .Then(user => SendWelcomeEmail(user))
            .Then(user => new ResultSuccess<string>($"User {user.Username} registered successfully!", "done"));

        var success = Assert.IsType<ResultSuccess<string>>(result);
        Assert.Equal("User alice registered successfully!", success.Data);
        Assert.Equal("done", success.Message);

        // --- Local pipeline steps ---
        static Result<User> ValidateInput(string username, string password)
            => string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)
                ? new ResultFailure<User>("Invalid input", ResultErrorCode.ValidationFailed)
                : new ResultSuccess<User>(new User { Username = username, Password = password }, "input valid");

        static Result<User> CheckUserDoesNotExist(User user)
            => user.Username == "bob"
                ? new ResultFailure<User>("User already exists", ResultErrorCode.ValidationFailed)
                : new ResultSuccess<User>(user, "user does not exist");

        static Result<User> HashPassword(User user)
            => new ResultSuccess<User>(user with { Password = "hashed:" + user.Password }, "password hashed");

        static Result<User> SaveUser(User user)
            => new ResultSuccess<User>(user, "user saved");

        static Result<User> SendWelcomeEmail(User user)
            => new ResultSuccess<User>(user, "email sent");

    }

    [Fact]
    public void Then_UserRegistration_LongChain_Failure()
    {
        // Simulate user registration pipeline (see these simple functions below assertions)
        var result = ValidateInput("bob", "password123")
            .Then(user => CheckUserDoesNotExist(user))
            .Then(user => HashPassword(user))
            .Then(user => SaveUser(user))
            .Then(user => SendWelcomeEmail(user))
            .Then(user => new ResultSuccess<string>($"User {user.Username} registered successfully!", "done"));


        var failure = Assert.IsType<ResultFailure<string>>(result);
        Assert.Equal("User already exists", failure.Message);
        Assert.Equal(ResultErrorCode.ValidationFailed, failure.ErrorCode);


        // --- Local pipeline steps ---
        static Result<User> ValidateInput(string username, string password)
            => string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)
                ? new ResultFailure<User>("Invalid input", ResultErrorCode.ValidationFailed)
                : new ResultSuccess<User>(new User { Username = username, Password = password }, "input valid");

        static Result<User> CheckUserDoesNotExist(User user)
            => user.Username == "bob"
                ? new ResultFailure<User>("User already exists", ResultErrorCode.ValidationFailed)
                : new ResultSuccess<User>(user, "user does not exist");

        static Result<User> HashPassword(User user)
            => new ResultSuccess<User>(user with { Password = "hashed:" + user.Password }, "password hashed");

        static Result<User> SaveUser(User user)
            => new ResultSuccess<User>(user, "user saved");

        static Result<User> SendWelcomeEmail(User user)
            => new ResultSuccess<User>(user, "email sent");
    }

    [Fact]
    public async Task ThenAsync_SingleAsyncFunc_Works()
    {
        var initial = new ResultSuccess<int>(5, "ok");
        var result = await initial.ToAsync()
            .Then<int, string>(async i =>
            {
                await Task.Delay(10);
                return new ResultSuccess<string>($"AsyncValue: {i}", "async done");
            });

        var success = Assert.IsType<ResultSuccess<string>>(result);
        Assert.Equal("AsyncValue: 5", success.Data);
        Assert.Equal("async done", success.Message);
    }

    [Fact]
    public async Task ThenAsync_ChainTwoAsyncFuncs_Works()
    {
        var initial = new ResultSuccess<int>(7, "ok");
        var result = await initial.ToAsync()
            .Then<int, string>(async i =>
            {
                await Task.Delay(10);
                return new ResultSuccess<string>($"First: {i}", "first async");
            })
            .Then<string, string>(async s =>
            {
                await Task.Delay(10);
                return new ResultSuccess<string>($"{s} - Second", "second async");
            });

        var success = Assert.IsType<ResultSuccess<string>>(result);
        Assert.Equal("First: 7 - Second", success.Data);
        Assert.Equal("second async", success.Message);
    }

    [Fact]
    public async Task Then_ThenAsync_Then_ThenAsync_Chains_Correctly()
    {

        var result = await ValidateInput("bob", "password123").ToAsync()
            .Then(async user => await CheckUserDoesNotExist(user))
            .Then(user => HashPassword(user))
            .Then(user => SaveUser(user))
            .Then(user => SendWelcomeEmail(user))
            .Then(user => new ResultSuccess<string>($"User {user.Username} registered successfully!", "done"));

        var success = Assert.IsType<ResultSuccess<string>>(result);
        Assert.Equal("Value: 3!", success.Data);
        Assert.Equal("async4", success.Message);

        static Result<User> ValidateInput(string username, string password)
    => string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)
        ? new ResultFailure<User>("Invalid input", ResultErrorCode.ValidationFailed)
        : new ResultSuccess<User>(new User { Username = username, Password = password }, "input valid");

        static async Task<Result<User>> CheckUserDoesNotExist(User user)
            => user.Username == "bob"
                ? new ResultFailure<User>("User already exists", ResultErrorCode.ValidationFailed)
                : new ResultSuccess<User>(user, "user does not exist");

        static Result<User> HashPassword(User user)
            => new ResultSuccess<User>(user with { Password = "hashed:" + user.Password }, "password hashed");

        static Result<User> SaveUser(User user)
            => new ResultSuccess<User>(user, "user saved");

        static Result<User> SendWelcomeEmail(User user)
            => new ResultSuccess<User>(user, "email sent");
    }

    private record User
    {
        public string Username { get; init; }
        public string Password { get; init; }
    }

    // Helper for unknown result type
    private sealed record DummyResult<T>() : Result<T>("dummy") { }
}