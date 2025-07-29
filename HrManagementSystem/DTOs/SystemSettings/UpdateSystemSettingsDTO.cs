using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.SystemSettings
{
    public class UpdateSystemSettingsDTO
    {
        public string Weekend1 { get; set; }
        public string? Weekend2 { get; set; }
        public string HoursRate { get; set; }
        public int BonusValue { get; set; }
        public int DeductionValue { get; set; }
    }
}
