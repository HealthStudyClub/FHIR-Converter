using Microsoft.Health.Fhir.Liquid.Converter;
using Microsoft.Health.Fhir.Liquid.Converter.Models;
using Microsoft.Health.Fhir.Liquid.Converter.Models.Hl7v2;
using Microsoft.Health.Fhir.Liquid.Converter.Processors;
using Microsoft.Health.Fhir.Liquid.Converter.Tool.Models;
using Newtonsoft.Json;

namespace Firemetrics.Fhir.Converter {

    internal static class ConverterLogicHandler
    {
        private const string MetadataFileName = "metadata.json";
        private static readonly ProcessorSettings DefaultProcessorSettings = new ProcessorSettings();

        internal static ConverterResult Convert(string inputContent, string templateDirectory, string rootTemplate, bool isTraceInfo)
        {
            var dataType = GetDataTypes(templateDirectory);
            var dataProcessor = CreateDataProcessor(dataType);
            var templateProvider = CreateTemplateProvider(dataType, templateDirectory);
            var traceInfo = CreateTraceInfo(dataType, isTraceInfo);

            var resultString = dataProcessor.Convert(inputContent, rootTemplate, templateProvider, traceInfo);
            return new ConverterResult(ProcessStatus.OK, resultString, traceInfo);
        }

        private static DataType GetDataTypes(string templateDirectory)
        {
            if (!Directory.Exists(templateDirectory))
            {
                throw new DirectoryNotFoundException($"Could not find template directory: {templateDirectory}");
            }

            var metadataPath = Path.Join(templateDirectory, MetadataFileName);
            if (!File.Exists(metadataPath))
            {
                throw new FileNotFoundException($"Could not find metadata.json in template directory: {templateDirectory}.");
            }

            var content = File.ReadAllText(metadataPath);
            var metadata = JsonConvert.DeserializeObject<Metadata>(content);
            if (Enum.TryParse<DataType>(metadata?.Type, ignoreCase: true, out var type))
            {
                return type;
            }

            throw new NotImplementedException($"The conversion from data type '{metadata?.Type}' to FHIR is not supported");
        }

        private static IFhirConverter CreateDataProcessor(DataType dataType)
        {
            return dataType switch
            {
                DataType.Hl7v2 => new Hl7v2Processor(DefaultProcessorSettings),
                DataType.Ccda => new CcdaProcessor(DefaultProcessorSettings),
                DataType.Json => new JsonProcessor(DefaultProcessorSettings),
                DataType.Fhir => new FhirProcessor(DefaultProcessorSettings),
                _ => throw new NotImplementedException($"The conversion from data type {dataType} to FHIR is not supported")
            };
        }

        private static ITemplateProvider CreateTemplateProvider(DataType dataType, string templateDirectory)
        {
            return new TemplateProvider(templateDirectory, dataType);
        }

        private static TraceInfo CreateTraceInfo(DataType dataType, bool isTraceInfo)
        {
            return isTraceInfo ? (dataType == DataType.Hl7v2 ? new Hl7v2TraceInfo() : new TraceInfo()) : null;
        }
    }

}