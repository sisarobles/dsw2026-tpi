
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentModel.Response> CreateAppointment(
            string userId,
            AppointmentModel.Request request);

        Task<IEnumerable<AppointmentSummaryModel.Response>> GetAppointmentByDni(
            string userId);

        Task DeleteAppointment(Guid idAppointment, string userId);

        Task<Pagination<AppointmentSearchModel.Response>> GetAppointmentBySearch(
            AppointmentSearchModel.Request request,
            int pageSize,
            int pageIndex);
    }
}
