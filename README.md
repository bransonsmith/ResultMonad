# SimpleResultMonad

A lightweight, simple, extensible, and type-safe Result monad for C#/.NET, supporting functional error handling, monadic chaining, and pattern matching.
Use this small package, that you can easily understand yourself, to help enable Functional practices in your C# project.

## Features

- **Type-safe success and failure results**
- **Extensible error codes** (define your own)
- **Monadic chaining** with `.Then(...)`
- **Pattern matching** made easy

---

## Installation

```
dotnet add package SimpleResultMonad
```

---

## API Overview

(That's it, 4 Types and 1 Function!)

- `ResultSuccess<T>(T data, string? message = null)`
- `ResultFailure<T>(string? message, ResultErrorCode errorCode, Exception? exception = null)`
- `Result<T>` (base type)
- `ResultErrorCode` (extensible error code type)
- `.Then(...)` extension method for chaining


## Example: Full Flow


```cs
public static class MyErrorCodes
{
  public static readonly ResultErrorCode EmployeeAlreadyExists = new("EmployeeAlreadyExists");
  public static readonly ResultErrorCode InvalidEmployeeData = new("InvalidEmployeeData"); }

  var employee = new Employee { Name = "Alice", Email = "alice@example.com" };

  var result = ValidateEmployee(employee)
                .Then(emp => CheckEmployeeDoesNotExist(emp))
                .Then(emp => SaveEmployee(emp))
                .Then(emp => SendWelcomeEmail(emp));
  switch (result)
  {
    case ResultSuccess<Employee> s: Console.WriteLine($"Employee onboarded: {s.Data.Name}"); break;
    case ResultFailure<Employee> f when f.ErrorCode == MyErrorCodes.EmployeeAlreadyExists: Console.WriteLine("Employee already exists."); break;
    case ResultFailure<Employee> f when f.ErrorCode == MyErrorCodes.InvalidEmployeeData: Console.WriteLine("Invalid employee data."); break;
    case ResultFailure<Employee> f: Console.WriteLine($"Other error: {f.ErrorCode.Code} - {f.Message}"); break;
  }
```

---
## Async Chaining

You can chain asynchronous operations using the async overloads of `.Then`.
To transition a sync chain into async, use `.ToAsync()`. 
Once, you've had 1 async call, all subsequent steps can be either synchronous or asynchronous.

**Example: Chaining async functions**

```cs
// Starting sync, chaining into later async calls
    var result = await ValidateEmployee(employee) // sync
    .ToAsync() // now async or sync call follow
    .Then(async emp => await CheckEmployeeDoesNotExistAsync(emp)) 
    .Then(emp => NormalizeEmployeeData(emp))
    .Then(async emp => await SaveEmployeeToDbAsync(emp))
    .Then(emp => new ResultSuccess<string>($"Employee {emp.Name} onboarded successfully!"));
```

---

### 1. Define Your Own Error Codes

You can define custom error codes in your application for domain-specific errors:

```cs
public static class MyErrorCodes
{
  public static readonly ResultErrorCode EmployeeAlreadyExists = new("EmployeeAlreadyExists");
  public static readonly ResultErrorCode InvalidEmployeeData = new("InvalidEmployeeData");
}
```

---

### 2. Chain Operations with `.Then`

Chain together multiple operations, each returning a `Result<T>`. If any step fails, the chain short-circuits and returns the failure.
All behavior contained to 2 types, no exception side-effects.

```cs
var employee = new Employee { Name = "Alice", Email = "alice@example.com" };
var result = ValidateEmployee(employee)
  .Then(emp => CheckEmployeeDoesNotExist(emp))
  .Then(emp => SaveEmployee(emp))
  .Then(emp => SendWelcomeEmail(emp));
```

---

### 3. Pattern Match on the Result

Handle each outcome explicitly using pattern matching on the ResultFailure.ErrorCode:
Define what to do on success, and then how to handle various ErrorCode scenarios (if needed).

```cs
switch (result)
{
  case ResultSuccess<Employee> s: Console.WriteLine($"Employee onboarded: {s.Data.Name}"); break;
  case ResultFailure<Employee> f when f.ErrorCode == MyErrorCodes.EmployeeAlreadyExists: Console.WriteLine("Employee already exists."); break;
  case ResultFailure<Employee> f when f.ErrorCode == MyErrorCodes.InvalidEmployeeData: Console.WriteLine("Invalid employee data."); break;
  case ResultFailure<Employee> f: Console.WriteLine($"Other error: {f.ErrorCode.Code} - {f.Message}"); break;
}
```


## License

MIT

---

**ResultMonad** makes functional error handling in C# easy, robust, and extensible.  
Define your own error codes, chain operations, and handle results with confidence!

