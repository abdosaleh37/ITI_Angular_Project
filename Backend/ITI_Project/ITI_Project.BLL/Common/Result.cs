namespace ITI_Project.BLL.Common
{
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; protected set; }
        public List<string> Errors { get; protected set; }

        protected Result(bool isSuccess, string? error)
        {
            if (isSuccess && error != null)
                throw new InvalidOperationException("A successful result cannot have an error.");
            if (!isSuccess && error == null)
                throw new InvalidOperationException("A failed result must have an error.");

            IsSuccess = isSuccess;
            Error = error;
            Errors = new List<string>();
            if (!string.IsNullOrEmpty(error))
                Errors.Add(error);
        }

        protected Result(bool isSuccess, List<string> errors)
        {
            if (isSuccess && errors.Any())
                throw new InvalidOperationException("A successful result cannot have errors.");
            if (!isSuccess && !errors.Any())
                throw new InvalidOperationException("A failed result must have at least one error.");

            IsSuccess = isSuccess;
            Errors = errors;
            Error = errors.Any() ? string.Join("; ", errors) : null;
        }

        public static Result Success() => new Result(true, (string?)null);

        public static Result Failure(string error) => new Result(false, error);

        public static Result Failure(List<string> errors) => new Result(false, errors);
    }

    public class Result<T> : Result
    {
        public T? Value { get; private set; }

        protected Result(T value, bool isSuccess, string? error) 
            : base(isSuccess, error)
        {
            Value = value;
        }

        protected Result(T value, bool isSuccess, List<string> errors) 
            : base(isSuccess, errors)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new Result<T>(value, true, (string?)null);

        public static new Result<T> Failure(string error) => new Result<T>(default, false, error);

        public static new Result<T> Failure(List<string> errors) => new Result<T>(default, false, errors);
    }
}