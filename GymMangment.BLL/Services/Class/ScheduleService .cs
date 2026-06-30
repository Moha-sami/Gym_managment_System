using AutoMapper;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Common;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.BookingViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GymMangment.BLL.Services.Class
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result<IEnumerable<SessionScheduleViewModel>>> GetScheduleAsync(CancellationToken ct = default)
        {
            var sessions = await _unitOfWork.Sessions.GetAllAsync(
                false, ct,
                s => s.Trainer,
                s => s.Category,
                s => s.SessionMembers);

            var model = _mapper.Map<IEnumerable<SessionScheduleViewModel>>(sessions);
            return Result<IEnumerable<SessionScheduleViewModel>>.Success(model);
        }

        public async Task<Result<IEnumerable<BookingViewModel>>> GetAllBookingsAsync(CancellationToken ct = default)
        {
            var bookings = await _unitOfWork.Bookings.GetAllAsync(
                false, ct,
                b => b.Member,
                b => b.Session,
                b => b.Session.Category,
                b => b.Session.Trainer);

            var model = _mapper.Map<IEnumerable<BookingViewModel>>(bookings);
            return Result<IEnumerable<BookingViewModel>>.Success(model);
        }

        public async Task<Result<IEnumerable<BookingViewModel>>> GetMemberBookingsAsync(int memberId, CancellationToken ct = default)
        {
            var bookings = await _unitOfWork.Bookings.GetAllAsync(
                false, ct,
                b => b.Member,
                b => b.Session,
                b => b.Session.Category,
                b => b.Session.Trainer);

            var memberBookings = bookings.Where(b => b.MemberId == memberId);
            var model = _mapper.Map<IEnumerable<BookingViewModel>>(memberBookings);
            return Result<IEnumerable<BookingViewModel>>.Success(model);
        }

        public async Task<Result<CreateBookingViewModel>> GetBookingFormDataAsync(int sessionId, CancellationToken ct = default)
        {
            var members = await _unitOfWork.Members.GetAllAsync(ct: ct);
            var memberships = await _unitOfWork.Memberships.GetAllAsync(ct: ct);

            // Only members with active membership can book
            var activeMemberIds = memberships
                .Where(m => m.IsActive)
                .Select(m => m.MemberID)
                .ToHashSet();

            var eligibleMembers = members
                .Where(m => activeMemberIds.Contains(m.Id))
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.Name
                });

            var model = new CreateBookingViewModel
            {
                SessionId = sessionId,
                Members = eligibleMembers
            };

            return Result<CreateBookingViewModel>.Success(model);
        }

        public async Task<Result> BookSessionAsync(int sessionId, int memberId, CancellationToken ct = default)
        {
            // Check member has active membership
            var memberships = await _unitOfWork.Memberships.GetAllAsync(ct: ct);
            var hasActiveMembership = memberships.Any(m => m.MemberID == memberId && m.IsActive);
            if (!hasActiveMembership)
                return Result.Failure("Member does not have an active membership.");

            // Check session exists and has available slots
            var sessions = await _unitOfWork.Sessions.GetAllAsync(
                false, ct,
                s => s.SessionMembers);
            var session = sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null)
                return Result.Failure("Session not found.");

            if (session.SessionMembers.Count >= session.Capacity)
                return Result.Failure("Session is fully booked.");

            if (session.EndDate < DateTime.Now)
                return Result.Failure("Cannot book a completed session.");

            // Check member hasn't already booked this session
            var existingBooking = await _unitOfWork.Bookings.AnyAsync(
                b => b.SessionId == sessionId && b.MemberId == memberId, ct);
            if (existingBooking)
                return Result.Failure("Member has already booked this session.");

            var booking = new Booking
            {
                SessionId = sessionId,
                MemberId = memberId,
                IsAttended = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var rows = await _unitOfWork.Bookings.AddAsync(booking, ct);
            return rows > 0
                ? Result.Success()
                : Result.Failure("Failed to create booking. Please try again.");
        }

        public async Task<Result> CancelBookingAsync(int bookingId, CancellationToken ct = default)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId, ct);
            if (booking == null)
                return Result.Failure("Booking not found.");

            await _unitOfWork.Bookings.DeleteAsync(booking, ct);
            return Result.Success();
        }
    }
}