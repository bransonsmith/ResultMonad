# ResultNomad

A simple, extensible, and type-safe Result monad for C#/.NET, supporting functional error handling, monadic chaining, and pattern matching.

## Features

- **Type-safe success and failure results**
- **Extensible error codes** (define your own)
- **Monadic chaining** with `.Then(...)`
- **Pattern matching** made easy

---

## Installation

```
dotnet add package ResultNomad
```

---

## Quick Start

### 1. Define Your Own Error Codes

You can define custom error codes in your application for domain-specific errors:

```cs
public static class MyErrorCodes { public static readonly ResultErrorCode EmployeeAlreadyExists = new("EmployeeAlreadyExists"); public static readonly ResultErrorCode InvalidEmployeeData = new("InvalidEmployeeData"); }
```

---

### 2. Chain Operations with `.Then`

Chain together multiple operations, each returning a `Result<T>`. If any step fails, the chain short-circuits and returns the failure.

```cs
var employee = new Employee { Name = "Alice", Email = "alice@example.com" };
var result = ValidateEmployee(employee) .Then(emp => CheckEmployeeDoesNotExist(emp)) .Then(emp => SaveEmployee(emp)) .Then(emp => SendWelcomeEmail(emp));
```

---

### 3. Pattern Match on the Result

Handle each outcome explicitly using pattern matching:

```cs
switch (result) { case ResultSuccess<Employee> s: Console.WriteLine($"Employee onboarded: {s.Data.Name}"); break; case ResultFailure<Employee> f when f.ErrorCode == MyErrorCodes.EmployeeAlreadyExists: Console.WriteLine("Employee already exists."); break; case ResultFailure<Employee> f when f.ErrorCode == MyErrorCodes.InvalidEmployeeData: Console.WriteLine("Invalid employee data."); break; case ResultFailure<Employee> f: Console.WriteLine($"Other error: {f.ErrorCode.Code} - {f.Message}"); break; }
```


---

## Example: Full Flow


```cs
public static class MyErrorCodes { public static readonly ResultErrorCode EmployeeAlreadyExists = new("EmployeeAlreadyExists"); public static readonly ResultErrorCode InvalidEmployeeData = new("InvalidEmployeeData"); }
var employee = new Employee { Name = "Alice", Email = "alice@example.com" };
var result = ValidateEmployee(employee) .Then(emp => CheckEmployeeDoesNotExist(emp)) .Then(emp => SaveEmployee(emp)) .Then(emp => SendWelcomeEmail(emp));
switch (result) { case ResultSuccess<Employee> s: Console.WriteLine($"Employee onboarded: {s.Data.Name}"); break; case ResultFailure<Employee> f when f.ErrorCode == MyErrorCodes.EmployeeAlreadyExists: Console.WriteLine("Employee already exists."); break; case ResultFailure<Employee> f when f.ErrorCode == MyErrorCodes.InvalidEmployeeData: Console.WriteLine("Invalid employee data."); break; case ResultFailure<Employee> f: Console.WriteLine($"Other error: {f.ErrorCode.Code} - {f.Message}"); break; }
```

---

## API Overview

- `ResultSuccess<T>(T data, string? message = null)`
- `ResultFailure<T>(string? message, ResultErrorCode errorCode, Exception? exception = null)`
- `Result<T>` (base type)
- `ResultErrorCode` (extensible error code type)
- `.Then(...)` extension method for chaining

---

## License

MIT

---

**ResultNomad** makes functional error handling in C# easy, robust, and extensible.  
Define your own error codes, chain operations, and handle results with confidence!

