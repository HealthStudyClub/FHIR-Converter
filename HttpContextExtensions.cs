using System.Text.Json;

namespace UME.Fhir.Converter
{
    public static class HttpContextExtensions
    {
        public static async Task WriteSuccess(this HttpContext context, string jsonString)
        {
            context.Response.ContentType = "application/fhir+json";
            await context.Response.WriteAsync(jsonString);
        }

        public static async Task WriteError(this HttpContext context, int status, Exception ex)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // pretty print for demo purposes
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // FHIR uses camelCase
            };
            
            context.Response.ContentType = "application/fhir+json";
            context.Response.StatusCode = status;
            await context.Response.WriteAsync(JsonSerializer.Serialize(OperationOutcome.For(status, ex.Message), options));
        }
    }
}

