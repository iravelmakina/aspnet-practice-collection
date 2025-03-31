namespace DNET.Backend.Api.Infrastructure;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class SwaggerHeaderAttribute : Attribute
{
    public SwaggerHeaderAttribute(string name, string? description = null, bool required = false, string? format = null)
    {
        Name = name;
        Description = description;
        Required = required;
        Format = format;
    }

    public string Name { get; }
    public string? Description { get; }
    public bool Required { get; }
    public string? Format { get; }
}
