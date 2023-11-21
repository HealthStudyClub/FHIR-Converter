using Microsoft.Health.Fhir.Liquid.Converter;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Telemetry;
using System.Text.Json;

namespace UME.Fhir.Converter {

    internal static class ConverterLogicHandler
    {

        private const string MetadataFileName = "metadata.json";
        private static readonly ProcessorSettings DefaultProcessorSettings = new ProcessorSettings();

        private static readonly string TemplateRootDir = Environment.GetEnvironmentVariable("TEMPLATE_ROOT_DIR") ?? "templates";

        private static readonly Dictionary<DataType, string> TemplateDirs = Directory.GetDirectories(TemplateRootDir).ToDictionary(
            x => GetDataType(x),
            x => x
        );
       
        private static readonly Dictionary<DataType, ITemplateProvider> TemplateProviders = TemplateDirs.ToDictionary(
            x => x.Key,
            x => (ITemplateProvider)new TemplateProvider(TemplateDirs[x.Key], x.Key)
        );

        // prepare and cache processors
        // seems to be thread-safe, see:
        // https://github.com/microsoft/FHIR-Converter/blob/main/src/Microsoft.Health.Fhir.Liquid.Converter/Processors/BaseProcessor.cs#L92
        private static readonly Dictionary<DataType, IFhirConverter> Processors = new Dictionary<DataType, IFhirConverter> {
            { DataType.Hl7v2, new Hl7v2Processor(DefaultProcessorSettings, new ConsoleTelemetryLogger()) },
            { DataType.Ccda, new CcdaProcessor(DefaultProcessorSettings, new ConsoleTelemetryLogger()) },
            { DataType.Json, new JsonProcessor(DefaultProcessorSettings, new ConsoleTelemetryLogger()) },
            { DataType.Fhir, new FhirProcessor(DefaultProcessorSettings, new ConsoleTelemetryLogger()) }
        };
        
        internal static ConverterResult Convert(string inputContent, string inputDataType, string rootTemplate, bool isTraceInfo)
        {
            if (Enum.TryParse<DataType>(inputDataType, ignoreCase: true, out var dataType))
            {
                var templateProvider = TemplateProviders[dataType];
                var traceInfo = CreateTraceInfo(dataType, isTraceInfo);
                var resultString = Processors[dataType].Convert(inputContent, rootTemplate, templateProvider, traceInfo);
                return new ConverterResult(ProcessStatus.OK, resultString, traceInfo);
            }
            
            throw new NotImplementedException($"The conversion from data type '{inputDataType}' to FHIR is not supported");
        }

        private static DataType GetDataType(string templateDirectory)
        {
            if (!Directory.Exists(templateDirectory))
            {
                throw new DirectoryNotFoundException($"Could not find template directory: {templateDirectory}");
            }

            var metadataPath = Path.Combine(templateDirectory, MetadataFileName);
            if (!File.Exists(metadataPath))
            {
                throw new FileNotFoundException($"Could not find metadata.json in template directory: {templateDirectory}.");
            }

            var content = File.ReadAllText(metadataPath);
            var typeStr = JsonDocument.Parse(content).RootElement.GetProperty("type").GetString();
            if (Enum.TryParse<DataType>(typeStr, ignoreCase: true, out var type))
            {
                return type;
            }

            throw new NotImplementedException($"The conversion from data type '{typeStr}' to FHIR is not supported");
        }

        private static TraceInfo CreateTraceInfo(DataType dataType, bool isTraceInfo)
        {
            return isTraceInfo ? (dataType == DataType.Hl7v2 ? new Hl7v2TraceInfo() : new TraceInfo()) : null;
        }
    }

}