namespace Firemetrics.Fhir.Converter
{
    public class ParameterNotFoundException : Exception
    {
        public ParameterNotFoundException()
        {
        }

        public ParameterNotFoundException(string message)
            : base(message)
        {
        }

        public ParameterNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }


}