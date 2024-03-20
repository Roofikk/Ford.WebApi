namespace Ford.WebApi.Dtos.Response;

public class ResponseResult<T> : ResponseResult
{
    public T? Result { get; set; }
}

public class ResponseResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}