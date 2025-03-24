namespace FDS.CRM.Infrastructure.Swagger;

/// <summary>
/// Add extra parameters for uploading files in swagger.
/// </summary>
public class FileUploadFilter : IOperationFilter
{
    /// <summary>
    /// Applies the specified operation.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="context">The context.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {

        var fileUploadMime = "multipart/form-data";
        if (operation.RequestBody == null || !operation.RequestBody.Content.Any(x => x.Key.Equals(fileUploadMime, StringComparison.InvariantCultureIgnoreCase)))
            return;

        var fileParams = context.MethodInfo.GetParameters().Where(p => p.ParameterType == typeof(IFormFile));
        operation.RequestBody.Content[fileUploadMime].Schema.Properties =
            fileParams.ToDictionary(k => k.Name, v => new OpenApiSchema()
            {
                Type = "string",
                Format = "binary"
            });
    }

    /// <summary>
    /// Indicates swashbuckle should consider the parameter as a file upload
    /// </summary>
    //[AttributeUsage(AttributeTargets.Method)]
    //public class FileContentType : Attribute
    //{

    //}
}
