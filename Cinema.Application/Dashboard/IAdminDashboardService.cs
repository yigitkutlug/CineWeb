namespace Cinema.Application.Dashboard;

public interface IAdminDashboardService
{
    Task<AdminDashboardSummaryDto> GetSummaryAsync();
}
