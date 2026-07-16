using Core.UseCases.AuditLogs;

namespace Core.Abstractions;

public interface IAuditLogWriter
{
    void Add(AuditLogEntry entry);
}
