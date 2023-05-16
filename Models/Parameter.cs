namespace UME.Fhir.Converter
{
    public record ConvertParameter(string name, string valueString);
    public record ConvertParameters(string resourceType, List<ConvertParameter> parameter);

    public record ParsedConvertParameters(string inputData, string inputDataType, string rootTemplate);

}

