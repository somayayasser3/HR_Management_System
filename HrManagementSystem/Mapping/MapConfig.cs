using AutoMapper;
using HrManagementSystem.DTOs.AttendaceDTOs;
using HrManagementSystem.DTOs.DepartmentsDTOs;
using HrManagementSystem.DTOs.Employee;
using HrManagementSystem.DTOs.EmployeeDTOs;
using HrManagementSystem.DTOs.LeaveDTOs;
using HrManagementSystem.DTOs.LeaveType;
using HrManagementSystem.DTOs.OfficialHoliday;
using HrManagementSystem.DTOs.SalaryReportsDTOs;
using HrManagementSystem.DTOs.SystemSettings;
using HrManagementSystem.DTOs.WorkingTaskDTOs;
using HrManagementSystem.Models;

namespace HrManagementSystem.Mapping
{
    public class MapConfig : Profile
    {
        public MapConfig()
        {
            // Add your mapping configurations here
            CreateMap<Employee, DisplayEmployeeData>().AfterMap((src,des) =>
            {
                des.DepartmentName = src.Department.DepartmentName;
                des.Email = src.User.Email;
            });
            

            CreateMap<AddEmployee, Employee>().ReverseMap();

            CreateMap<AddEmployee, User>()
               .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
               .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
               .ForMember(dest => dest.IsActive, opt => opt.Ignore())
               .ForMember(dest => dest.Employee, opt => opt.Ignore());
            CreateMap<SystemSetting, UpdateSystemSettingsDTO>().ReverseMap();
            CreateMap<OfficialHoliday, OfficialHolidayDisplayDTO>().ReverseMap();
            CreateMap<OfficialHoliday, EditOfficalHolidayDTO>().ReverseMap();
            CreateMap<LeaveRequest, DisplayResultforLeaveRequest>().AfterMap(
                (src, des)=>
                {
                    des.EmployeeName = src.Employee.FullName;
                });
            CreateMap<LeaveRequest, AddLeaveRequest>().ReverseMap();
            CreateMap<Department, GetDepartmentsDTO>().ReverseMap();
            CreateMap<AddNewDepartmentDTO,Department>();
            CreateMap<UpdateExistingDepartmentDTO,Department>();
            CreateMap<SalaryReport, GetSalaryReportDTO>().AfterMap((src, dest) =>
            {
                dest.EmployeeName = src?.Employee?.FullName;
                dest.Year = src.GeneratedAt.Year;
                dest.DepartmentName = src.Employee.Department.DepartmentName;
            });
            CreateMap<Attendance, GetAttendaceDTO>().AfterMap((src, dest) =>
            {
                dest.EmployeeName = src.Employee.FullName;
                dest.DepartmentName = src.Employee.Department.DepartmentName;
            });
            CreateMap<Attendance, AddEmpAttendance>().ReverseMap();
            CreateMap<Attendance, UpdateEmployeeAttendance>().ReverseMap();
            CreateMap<UpdateEmployeeDTO, Employee>().ReverseMap();
            CreateMap<WorkingTask, AddWorkingTaskDTO>().ReverseMap();
            CreateMap<WorkingTask, DisplayWorkingTaskDTO>().AfterMap((src, dest) =>
            {
                dest.TaskId = src.Id;
                dest.EmployeeName = src.Employee?.FullName;
                dest.EmployeeId = src.Employee.EmployeeId;
                dest.DepartmentName = src.Employee.Department.DepartmentName;

            }).ReverseMap();
            CreateMap<AdminUpdatesTaskDTO, WorkingTask>().ReverseMap();
            CreateMap<UpdateEmployeeDTO, Employee>().ReverseMap();
            CreateMap<UpdateEmployeeDTO, User>().ReverseMap();
            CreateMap<AdminUpdatesAttendanceDTO, Attendance>().ReverseMap();
            CreateMap<LeaveType, AddLeaveTypeDTO>().ReverseMap();
            CreateMap<LeaveType, GetLeaveTypeDTO>().ReverseMap();
            //CreateMap<GetAttendaceDTO, AddEmpAttendance>().ReverseMap();
            //CreateMap<GetAttendaceDTO, UpdateEmployeeAttendance>().ReverseMap();
        }
    }
    
}
