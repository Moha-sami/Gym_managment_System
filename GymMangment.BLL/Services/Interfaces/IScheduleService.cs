using GymMangment.BLL.Common;
using GymMangment.BLL.ViewModels.BookingViewModels;

namespace GymMangment.BLL.Services.Interfaces
{
    public interface IScheduleService
    {
        Task<Result<IEnumerable<SessionScheduleViewModel>>> GetScheduleAsync(CancellationToken ct = default);
        Task<Result<IEnumerable<BookingViewModel>>> GetAllBookingsAsync(CancellationToken ct = default);
        Task<Result<IEnumerable<BookingViewModel>>> GetMemberBookingsAsync(int memberId, CancellationToken ct = default);
        Task<Result<CreateBookingViewModel>> GetBookingFormDataAsync(int sessionId, CancellationToken ct = default);
        Task<Result> BookSessionAsync(int sessionId, int memberId, CancellationToken ct = default);
        Task<Result> CancelBookingAsync(int memberId, int sessionId, CancellationToken ct = default);
    }
}