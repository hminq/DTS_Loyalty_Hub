using System;
using System.Collections.Generic;
using System.Text.Json;
using Core.Abstractions;

namespace Infrastructure.Implementations;

public class TemplateEngine : ITemplateEngine
{
    public string Render(string template, string availableVariablesJson, object dataContext)
    {
        if (string.IsNullOrEmpty(template) || string.IsNullOrEmpty(availableVariablesJson))
            return template;

        var configs = JsonSerializer.Deserialize<List<TemplateVariableConfig>>(availableVariablesJson);
        if (configs == null || configs.Count == 0) return template;

        var dataDict = ConvertToDictionary(dataContext);
        var result = template;

        foreach (var config in configs)
        {
            string placeholder = "{{" + config.FieldName + "}}";
            string replacement = "";

            if (config.FormatType == "PROPERTY")
            {
                if (config.FormatParams.TryGetValue("name", out string propPath))
                {
                    replacement = EvaluateProperty(propPath, dataDict);
                }
            }
            else if (config.FormatType == "FUNCTION")
            {
                if (config.FormatParams.TryGetValue("name", out string funcName) &&
                    config.FormatParams.TryGetValue("params", out string funcParamPath))
                {
                    var paramValue = EvaluateProperty(funcParamPath, dataDict);
                    replacement = EvaluateFunction(funcName, paramValue);
                }
            }

            result = result.Replace(placeholder, replacement);
        }

        return result;
    }

    private string EvaluateProperty(string propertyPath, Dictionary<string, object> dataDict)
    {
        var parts = propertyPath.Split('.');
        if (parts.Length == 2)
        {
            var objName = parts[0];
            var propName = parts[1];

            if (dataDict.TryGetValue(objName, out var objValue) && objValue != null)
            {
                var propInfo = objValue.GetType().GetProperty(propName);
                if (propInfo != null)
                {
                    return propInfo.GetValue(objValue)?.ToString() ?? "";
                }
            }
        }
        return "";
    }

    private string EvaluateFunction(string functionName, string paramValue)
    {
        return functionName switch
        {
            "GetCustomerName" => string.IsNullOrEmpty(paramValue) ? "Quý khách" : paramValue,
            "FormatDateTime" => DateTime.TryParse(paramValue, out var dt) ? dt.ToString("dd/MM/yyyy") : paramValue,
            _ => paramValue
        };
    }

    private Dictionary<string, object> ConvertToDictionary(object obj)
    {
        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        if (obj == null) return dict;

        foreach (var prop in obj.GetType().GetProperties())
        {
            dict[prop.Name] = prop.GetValue(obj);
        }
        return dict;
    }
}
