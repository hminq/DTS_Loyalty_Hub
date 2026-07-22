using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Core.Abstractions;

namespace Scheduler;

public class TierExpirationSchedulerWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TierExpirationSchedulerWorker> _logger;
    
    // Đặt chu kỳ chạy (khi test bạn có thể đổi thành TimeSpan.FromSeconds(30))
    private readonly TimeSpan _period = TimeSpan.FromSeconds(30);

    public TierExpirationSchedulerWorker(
        IServiceProvider serviceProvider, 
        ILogger<TierExpirationSchedulerWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Tier Expiration Scheduler Worker đã khởi chạy.");

        // Tạo PeriodicTimer chạy định kỳ
        using var timer = new PeriodicTimer(_period);

        // Chạy lần đầu tiên ngay khi start app (hoặc bỏ qua nếu muốn đợi hết 1 chu kỳ)
        await ProcessExpiredTiersAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken) && !stoppingToken.IsCancellationRequested)
        {
            await ProcessExpiredTiersAsync(stoppingToken);
        }
    }

    private async Task ProcessExpiredTiersAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Bắt đầu quét danh sách khách hàng hết hạn Tier...");

            // Vì BackgroundService là Singleton, cần tạo Scope để inject Scoped Services (DbContext/Repository)
            using var scope = _serviceProvider.CreateScope();
            var customerTierRepo = scope.ServiceProvider.GetRequiredService<ICustomerTierRepo>();

            int updatedCount = await customerTierRepo.CheckAndProcessExpiredTiersAsync(cancellationToken);

            _logger.LogInformation("Đã quét xong! Số lượng customer được xử lý (giữ hạng/hạ hạng): {Count}", updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Có lỗi xảy ra trong quá trình quét cập nhật Tier!");
        }
    }
}