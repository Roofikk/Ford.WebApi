namespace Ford.WebApi.Dtos.Response;

public class ServiceResult<T> : ServiceResult
{
    public T? Result { get; set; }
}

public class ServiceResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}