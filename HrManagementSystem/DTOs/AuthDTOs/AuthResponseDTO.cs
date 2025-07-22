namespace HrManagementSystem.DTOs.AuthDTOs
{
    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
