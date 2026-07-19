using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentModel.Response> CreateAppointment(AppointmentModel.Request request);
        Task<IEnumerable<AppointmentSummaryModel.Response>> GetAppointmentByDni(long dni);  
        Task DeleteAppointment(Guid idAppointment);//id del url
        Task<Pagination<AppointmentSearchModel.Response>> GetAppointmentBySearch(AppointmentSearchModel.Request request, int pageSize, int pageIndex); 
    }
}
