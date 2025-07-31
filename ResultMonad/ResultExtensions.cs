namespace ResultMonad;

public static class ResultExtensions
{
    /// <summary>
    /// Chains the current <see cref="Result{TIn}"/> to the next operation, returning a <see cref="Result{TOut}"/>.
    /// If the current result is a success, invokes the provided function with the success data.
    /// If the current result is a failure, propagates the failure to the next result type.
    /// </summary>
    /// <typeparam name="TIn">The type of the data in the input result.</typeparam>
    /// <typeparam name="TOut">The type of the data in the output result.</typeparam>
    /// <param name="result">The current result to bind from.</param>
    /// <param name="func">A function to invoke if the result is successful.</param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the outcome of the chained operation,
    /// or a propagated failure if the original result was a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var employee = new Employee { Name = "Bob", Email = "bob@example.com" };
    /// var result = ValidateEmployee(employee)
    ///     .Then(emp => CheckEmployeeDoesNotExist(emp))
    ///     .Then(emp => NormalizeEmployeeData(emp))
    ///     .Then(emp => SaveEmployeeToDb(emp))
    ///     .Then(emp => SendWelcomeEmail(emp))
    ///     .Then(emp => new ResultSuccess&lt;string&gt;($"Employee {emp.Name} onboarded successfully!"));
    /// </code>
    /// </example>
    public static Result<TOut> Then<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> func)
    {
        if (result is ResultSuccess<TIn> success)
        {
            if (func is null)
            {
                return new ResultFailure<TOut>(
                    "The function passed to Then was null.",
                    ResultErrorCode.UnexpectedResultFailure
                );
            }
            try
            {
                var next = func(success.Data);
                if (next is null)
                {
                    return new ResultFailure<TOut>(
                        "The function passed to Then returned null.",
                        ResultErrorCode.UnexpectedResultFailure
                    );
                }
                return next;
            }
            catch (Exception ex)
            {
                return new ResultFailure<TOut>(
                    "Unexpected unhandled exception caught in func used in Result.Then(<func>). " + ex.Message,
                    ResultErrorCode.UncaughtExceptionInThenFunc,
                    ex
                );
            }
        }
        else if (result is ResultFailure<TIn> failure)
        {
            return new ResultFailure<TOut>(
                failure.Message,
                failure.ErrorCode,
                failure.Exception
            );
        }
        else
        {
            return new ResultFailure<TOut>(
                "Unexpected failure resolving Result.",
                ResultErrorCode.UnexpectedResultFailure
            );
        }
    }
}