using System.Collections.Generic;

namespace Core.Abstractions;

public interface ITemplateEngine
{
    string Render(string template, string availableVariablesJson, object dataContext);
}
