using System.Text.Json;
using UME.Fhir.Converter;

var appName = "UME.Fhir.Converter";

// nice rececipes https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-7.0
var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);

var app = builder.Build();

// Get a logger
var logger = app.Services.GetRequiredService<ILogger<Program>>();

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
    return $"{appName}"; 
});

app.MapGet("/probe", (HttpContext context) => {
    logger.LogInformation($"Liveness probe was called from {context.Connection.RemoteIpAddress}");
    return $"alive at {DateTimeOffset.Now.ToString()}"; 
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
                inputDataType: parameters.GetParameter("inputDataType").ToLower(),
                rootTemplate: parameters.GetParameter("rootTemplate")
            );

            // convert to JSON FHIR
            var converterResult = ConverterLogicHandler.Convert(
                inputContent: parsedParameters.inputData, 
                inputDataType: parsedParameters.inputDataType, 
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
