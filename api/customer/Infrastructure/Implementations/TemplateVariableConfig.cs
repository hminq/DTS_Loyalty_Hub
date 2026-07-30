using System.Collections.Generic;

namespace Infrastructure.Implementations;

public class TemplateVariableConfig
{
    public string FieldName { get; set; } = string.Empty;
    public string FormatType { get; set; } = string.Empty; // "FUNCTION" or "PROPERTY"
    public Dictionary<string, string> FormatParams { get; set; } = new();
}
