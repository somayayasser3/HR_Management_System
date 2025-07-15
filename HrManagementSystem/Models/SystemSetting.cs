using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.Models
{
    public class SystemSetting
    {
        [Key]
        public int SettingId { get; set; }

        [Required]
        [MaxLength(100)]
        public string SettingName { get; set; }

        [Required]
        public string SettingValue { get; set; }

        [MaxLength(100)]
        public string SettingType { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
