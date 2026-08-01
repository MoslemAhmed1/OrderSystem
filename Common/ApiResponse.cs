namespace OrderSystem.Common
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> Success(T data, string message = "Success", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                StatusCode = statusCode,
                Message = message,
                Data = data,
                Errors = null
            };
        }

        public static ApiResponse<T> Fail(string message, int statusCode, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                StatusCode = statusCode,
                Message = message,
                Data = default,
                Errors = errors
            };
        }
    }

    public class ApiResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string>? Errors { get; set; }

        public static ApiResponse Success(string message = "Success", int statusCode = 200)
        {
            return new ApiResponse
            {
                StatusCode = statusCode,
                Message = message,
                Errors = null
            };
        }

        public static ApiResponse Fail(string message, int statusCode, List<string>? errors = null)
        {
            return new ApiResponse
            {
                StatusCode = statusCode,
                Message = message,
                Errors = errors
            };
        }
    }
}