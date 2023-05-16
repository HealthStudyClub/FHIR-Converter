using Microsoft.Health.Fhir.Liquid.Converter;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;
using System.Text.Json;

namespace UME.Fhir.Converter {

    internal static class ConverterLogicHandler
    {

        private static readonly Dictionary<string, DataType> SupportedInputFormats = new Dictionary<string, DataType> 
        { 
            { "hl7v2", DataType.Hl7v2 },
            { "ccda", DataType.Ccda },
            { "json", DataType.Json },
            { "fhir", DataType.Fhir }
        };

        private const string MetadataFileName = "metadata.json";
        private static readonly ProcessorSettings DefaultProcessorSettings = new ProcessorSettings();

        private static readonly string TemplateRootDir = Environment.GetEnvironmentVariable("TEMPLATE_ROOT_DIR") ?? "templates";

        private static readonly Dictionary<DataType, string> TemplateDirs = Directory.GetDirectories(TemplateRootDir).ToDictionary(
            x => GetDataType(x),
            x => x
        );
        
        private static readonly Dictionary<DataType, ITemplateProvider> TemplateProviders = SupportedInputFormats.ToDictionary(
            x => x.Value,
            x => (ITemplateProvider)new TemplateProvider(TemplateDirs[x.Value], x.Value)
        );

        // prepare and cache processors
        // seems to be thread-safe, see:
        // https://github.com/microsoft/FHIR-Converter/blob/main/src/Microsoft.Health.Fhir.Liquid.Converter/Processors/BaseProcessor.cs#L92
        private static readonly Dictionary<DataType, IFhirConverter> Processors = new Dictionary<DataType, IFhirConverter> {
            { DataType.Hl7v2, new Hl7v2Processor(DefaultProcessorSettings) },
            { DataType.Ccda, new CcdaProcessor(DefaultProcessorSettings) },
            { DataType.Json, new JsonProcessor(DefaultProcessorSettings) },
            { DataType.Fhir, new FhirProcessor(DefaultProcessorSettings) }
        };
        
        internal static ConverterResult Convert(string inputContent, string inputDataType, string rootTemplate, bool isTraceInfo)
        {
            if (SupportedInputFormats.ContainsKey(inputDataType)) 
            {
                var dataType = SupportedInputFormats[inputDataType];
                var templateProvider = TemplateProviders[dataType];
                var traceInfo = CreateTraceInfo(dataType, isTraceInfo);
                var resultString = Processors[dataType].Convert(inputContent, rootTemplate, templateProvider, traceInfo);
                return new ConverterResult(ProcessStatus.OK, resultString, traceInfo);
            }
            else 
            {
                throw new NotImplementedException($"The conversion from data type '{inputDataType}' to FHIR is not supported");
            }
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