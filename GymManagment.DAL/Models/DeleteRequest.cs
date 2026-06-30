using GymManagment.DAL.Models.Enum;

namespace GymManagment.DAL.Models
{
    public class DeleteRequest : BaseEntity
    {
        public DeleteTargetType TargetType { get; set; }
        public int TargetId { get; set; }
        public string TargetName { get; set; } = default!;
        public string? Reason { get; set; }
        public string RequestedByUserId { get; set; } = default!;
        public string RequestedByName { get; set; } = default!;
        public DeleteRequestStatus Status { get; set; } = DeleteRequestStatus.Pending;
        public string? ReviewedByUserId { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? AdminNote { get; set; }
    }
}
