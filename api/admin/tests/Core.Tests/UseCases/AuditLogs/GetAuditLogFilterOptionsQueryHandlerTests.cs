using Core.Entities.Constants;
using Core.UseCases.AuditLogs.Handlers;
using Core.UseCases.AuditLogs.Queries;
using FluentAssertions;

namespace Core.Tests.UseCases.AuditLogs;

public sealed class GetAuditLogFilterOptionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsSortedApplicationConstants()
    {
        var handler = new GetAuditLogFilterOptionsQueryHandler();

        var result = await handler.Handle(
            new GetAuditLogFilterOptionsQuery(),
            CancellationToken.None);

        result.EntityTypes.Should().Equal(AuditEntityTypes.All.Order(StringComparer.Ordinal));
        result.Actions.Should().Equal(AuditActions.All.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void AllCollections_ContainEveryPublicConstant()
    {
        AuditEntityTypes.All.Should().BeEquivalentTo(GetConstants(typeof(AuditEntityTypes)));
        AuditActions.All.Should().BeEquivalentTo(GetConstants(typeof(AuditActions)));
    }

    private static IReadOnlyCollection<string> GetConstants(Type type)
    {
        return type.GetFields()
            .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToArray();
    }
}
