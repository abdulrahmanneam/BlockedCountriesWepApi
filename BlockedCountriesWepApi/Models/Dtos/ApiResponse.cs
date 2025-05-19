namespace BlockedCountriesWepApi.Models.Dtos
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }

        public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data, StatusCode = 200 };
        public static ApiResponse<T> Error(string errorMessage, int statusCode) => new() { Success = false, ErrorMessage = errorMessage, StatusCode = statusCode };
    }
}
