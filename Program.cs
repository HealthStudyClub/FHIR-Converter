using Firemetrics.Fhir.Converter;
using System.Text.Json;


var appName = "Firemetrics.Fhir.Converter";
var templateRootDir = Environment.GetEnvironmentVariable("TEMPLATE_ROOT_DIR");
if (templateRootDir == null)
{
    templateRootDir = "templates";
}

var templateDirs = new Dictionary<string, string>
{
    { "Hl7v2", Path.Combine(templateRootDir, "Hl7v2") },
    { "Ccda", Path.Combine(templateRootDir, "Ccda") },
    { "Json", Path.Combine(templateRootDir, "Json") },
    { "Fhir", Path.Combine(templateRootDir, "Fhir") },
};

// nice rececipes https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["Server"] = appName;
        return Task.CompletedTask;
    });
    await next.Invoke();
});

app.MapGet("/", () => {
    return appName; 
});

/**
 * Make conversion available as a custom FHIR operation.
 * Try to be compliant with https://github.com/microsoft/fhir-server/blob/main/docs/ConvertDataOperation.md
 * see also https://build.fhir.org/operations.html
 */
app.MapPost("/$convert-data", async (HttpContext context) =>
{ 
    try {

        var parameters = await context.Request.ReadFromJsonAsync<ConvertParameters>();

        var parsedParameters = new ParsedConvertParameters(
                inputData: parameters.GetParameter("inputData"),
                inputDataType: parameters.GetParameter("inputDataType"),
                rootTemplate: parameters.GetParameter("rootTemplate")
            );

            // convert to JSON FHIR
            var converterResult = ConverterLogicHandler.Convert(
                inputContent: parsedParameters.inputData, 
                templateDirectory: templateDirs[parsedParameters.inputDataType], 
                rootTemplate: parsedParameters.rootTemplate, 
                isTraceInfo: false);

            await context.WriteSuccess(converterResult.FhirResource.ToString());
    }
    catch (ParameterNotFoundException ex) {
        await context.WriteError(400, ex);
    }
    catch (JsonException ex) {
        await context.WriteError(400, ex);
    }
    catch (Exception ex) {
        Console.WriteLine(ex.ToString());
        await context.WriteError(500, ex);
    }

});

app.Run();
