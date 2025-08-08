using System;
using System.Threading.Tasks;

namespace ResultMonad;

public static class ResultExtensions
{
    /// <summary>
    /// Chains the current <see cref="Result{TIn}"/> to the next synchronous operation, returning a <see cref="Result{TOut}"/>.
    /// If the current result is a success, invokes the provided function with the success data.
    /// If the current result is a failure, propagates the failure to the next result type.
    /// Use this for purely synchronous monadic chains.
    /// </summary>
    /// <typeparam name="TIn">The type of the data in the input result.</typeparam>
    /// <typeparam name="TOut">The type of the data in the output result.</typeparam>
    /// <param name="result">The current result to bind from.</param>
    /// <param name="func">A synchronous function to invoke if the result is successful.</param>
    /// <returns>
    /// A <see cref="Result{TOut}"/> representing the outcome of the chained operation,
    /// or a propagated failure if the original result was a failure.
    /// </returns>
    /// <example>
    /// <code>
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

    /// <summary>
    /// Chains the current <see cref="Task{Result{TIn}}"/> to the next synchronous operation, returning a <see cref="Task{Result{TOut}}"/>.
    /// Use this when your chain is already asynchronous, but the next step is synchronous.
    /// </summary>
    /// <typeparam name="TIn">The type of the data in the input result.</typeparam>
    /// <typeparam name="TOut">The type of the data in the output result.</typeparam>
    /// <param name="resultTask">The current asynchronous result to bind from.</param>
    /// <param name="func">A synchronous function to invoke if the result is successful.</param>
    /// <returns>
    /// A <see cref="Task{Result{TOut}}"/> representing the outcome of the chained operation,
    /// or a propagated failure if the original result was a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await ValidateEmployeeAsync(employee)
    ///     .Then(emp => NormalizeEmployeeData(emp))
    ///     .Then(emp => SaveEmployeeToDb(emp));
    /// </code>
    /// </example>
    public static async Task<Result<TOut>> Then<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> func)
    {
        var result = await resultTask.ConfigureAwait(false);
        return result.Then(func);
    }

    /// <summary>
    /// Chains the current <see cref="Task{Result{TIn}}"/> to the next asynchronous operation, returning a <see cref="Task{Result{TOut}}"/>.
    /// Use this for chaining asynchronous monadic operations.
    /// </summary>
    /// <typeparam name="TIn">The type of the data in the input result.</typeparam>
    /// <typeparam name="TOut">The type of the data in the output result.</typeparam>
    /// <param name="resultTask">The current asynchronous result to bind from.</param>
    /// <param name="func">An asynchronous function to invoke if the result is successful.</param>
    /// <returns>
    /// A <see cref="Task{Result{TOut}}"/> representing the outcome of the chained asynchronous operation,
    /// or a propagated failure if the original result was a failure.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await ValidateEmployeeAsync(employee)
    ///     .Then(async emp => await NormalizeEmployeeDataAsync(emp))
    ///     .Then(async emp => await SaveEmployeeToDbAsync(emp));
    /// </code>
    /// </example>
    public static async Task<Result<TOut>> Then<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> func)
    {
        var result = await resultTask.ConfigureAwait(false);

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
                var next = await func(success.Data).ConfigureAwait(false);
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
                    "Unexpected unhandled exception caught in async func used in Result.Then(<func>). " + ex.Message,
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

    /// <summary>
    /// Converts a synchronous <see cref="Result{T}"/> to a <see cref="Task{Result{T}}"/>.
    /// Use this to start an asynchronous chain from a synchronous result.
    /// </summary>
    /// <typeparam name="T">The type of the data in the result.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>A completed task containing the result.</returns>
    /// <example>
    /// <code>
    /// var result = await ValidateEmployee(employee)
    ///     .ToAsync()
    ///     .Then(async emp => await NormalizeEmployeeDataAsync(emp))
    ///     .Then(emp => SaveEmployeeToDb(emp));
    /// </code>
    /// </example>
    public static Task<Result<T>> ToAsync<T>(this Result<T> result)
        => Task.FromResult(result);
}