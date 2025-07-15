using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.Models
{
    public class OfficialHoliday
    {
        [Key]
        public int HolidayId { get; set; }

        [Required]
        [MaxLength(100)]
        public string HolidayName { get; set; }

        public DateTime HolidayDate { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
