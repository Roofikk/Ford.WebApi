using System.Net;

namespace Ford.WebApi.Dtos.Response;

public class BadResponse(string uri, string header, HttpStatusCode status, ICollection<Error> errors)
{
    public string Uri { get; set; } = uri;
    public string Header { get; set; } = header;
    public HttpStatusCode Status { get; set; } = status;
    public ICollection<Error> Errors { get; set; } = errors;
}

public class Error(string title, string description)
{
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
}