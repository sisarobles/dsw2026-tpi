using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;

namespace Dsw2026Tpi.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IPersistence _persistence;

        public AppointmentService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public Task<AppointmentModel.Response> CreateAppointment(AppointmentModel.Request request)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAppointment(Guid idAppointment)
        {
            throw new NotImplementedException();     
        }

        public Task<IEnumerable<AppointmentSummaryModel.Response>> GetAppointmentByDni(long dni)
        {
            throw new NotImplementedException();
        }

        public Task<Pagination<AppointmentSearchModel.Response>> GetAppointmentBySearch(AppointmentSearchModel.Request request, int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }
    }
}
