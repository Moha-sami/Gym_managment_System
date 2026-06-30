namespace GymMangment.BLL.ViewModels.AccountViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? CurrentRole { get; set; }
        public IEnumerable<string> AvailableRoles { get; set; } = [];
    }
}