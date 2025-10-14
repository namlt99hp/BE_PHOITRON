namespace BE_PHOITRON.Application.ResponsesModels
{
    public record ApiResponse<T>(bool Success, string? Message, int StatusCode, T? Data)
    {
        public static ApiResponse<T> Ok(T? data = default, string? message = null) => new(true, message, 200, data);
        public static ApiResponse<T> Created(T? data = default, string? message = null) => new(true, message, 201, data);
        public static ApiResponse<T> BadRequest(string message) => new(false, message, 400, default);
        public static ApiResponse<T> NotFound(string message = "Not Found") => new(false, message, 404, default);
        public static ApiResponse<T> Error(string message, int statusCode = 500) => new(false, message, statusCode, default);
    }
}


