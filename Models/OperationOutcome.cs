namespace UME.Fhir.Converter
{
    public class OperationOutcome
    {
        public static OperationOutcome For(int status, string message) {
            var oo = new OperationOutcome();
            oo.Issue.Add(new Issue {
                Severity = "error",
                Code = $"HTTP_{status}",
                Diagnostics = message
            });
            return oo;
        }

        public string ResourceType { get; set; } = "OperationOutcome";
        public List<Issue> Issue { get; set; } = new List<Issue>();
    }

    public class Issue
    {
        public string Severity { get; set; }
        public string Code { get; set; }
        public string Diagnostics { get; set; }
    }
}
