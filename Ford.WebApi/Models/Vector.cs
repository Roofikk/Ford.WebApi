using System.Text.Json.Serialization;

namespace Ford.WebApi.Models;

public class Vector
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    [JsonIgnore]
    public float Magnitude
    {
        get
        {
            return MathF.Sqrt(X * X + Y * Y + Z * Z);
        }
    }
}
