using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCatalog.Shared.Util
{
    public class Result<T>
    {
        public string? Message { get; set; }
        public T? Content { get; set; }
        public bool IsSuccess { get; set; }
        public string Status { get; set; } = null!;

        public static Result<T> Ok(T? content, string message)
        {
            Result<T> result = new()
            {
                Message = message,
                Content = content,
                IsSuccess = true,
                Status = "OK"
            };

            return result;
        }

        public static Result<T> NotFound(T? content, string message)
        {
            Result<T> result = new()
            {
                Message = message,
                Content = content,
                IsSuccess = false,
                Status = "NOT_FOUND"
            };

            return result;
        }

        public static Result<T> Created(T? content, string message)
        {
            Result<T> result = new()
            {
                Message = message,
                Content = content,
                IsSuccess = true,
                Status = "CREATED"
            };

            return result;
        }

        public static Result<T> InternalError(T? content, string message)
        {
            Result<T> result = new()
            {
                Message = message,
                Content = content,
                IsSuccess = false,
                Status = "INTERNAL_ERROR"
            };

            return result;
        }

        public static Result<T> OperationalError(T? content, string message)
        {
            Result<T> result = new()
            {
                Message = message,
                Content = content,
                IsSuccess = false,
                Status = "OPERATIONAL_ERROR"
            };

            return result;
        }
    }
}
