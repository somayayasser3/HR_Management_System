using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.AttendaceDTOs
{
    public class DetailedAttendanceDTO
    {
       public  List<GetAttendaceDTO> attendances { get; set; }

        public decimal OvertimeAmount { get; set; }
        public decimal DelayAmount { get; set; }
        public decimal OverTimeSummation { get; set; }
        public decimal DelaySummation { get; set; }



    }
}
