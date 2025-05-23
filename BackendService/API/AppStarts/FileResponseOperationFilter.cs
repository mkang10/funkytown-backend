using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

public class FileResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Tìm action nào có Produces("application/vnd.openxmlformats-officedocument.wordprocessingml.document")
        var hasDocx = context.MethodInfo
            .GetCustomAttributes(true)
            .OfType<Microsoft.AspNetCore.Mvc.ProducesAttribute>()
            .Any(a => a.ContentTypes.Contains("application/vnd.openxmlformats-officedocument.wordprocessingml.document"));

        if (!hasDocx) return;

        // Xóa response default (nếu có) và thay bằng binary response
        operation.Responses.Remove("200");
        operation.Responses["200"] = new OpenApiResponse
        {
            Description = "Word document",
            Content =
            {
                ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type   = "string",
                        Format = "binary"
                    }
                }
            }
        };
    }
}
