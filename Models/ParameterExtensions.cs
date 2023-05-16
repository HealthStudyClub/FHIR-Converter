namespace UME.Fhir.Converter
{
    public static class ConvertParametersExtensions
    {
        public static string GetParameter(this ConvertParameters parameters, string name)
        {
            try {
                return parameters.parameter.First(p => p.name == name).valueString;
            } catch (InvalidOperationException ex) {
                throw new ParameterNotFoundException($"Parameter '{name}' not found in request", ex);
            }
        }
    }
}