using AutoMapper;
using HrManagementSystem.DTOs.Employee;
using HrManagementSystem.DTOs.OfficialHoliday;
using HrManagementSystem.DTOs.SystemSettings;
using HrManagementSystem.DTOs.AttendaceDTOs;
using HrManagementSystem.DTOs.DepartmentsDTOs;
using HrManagementSystem.DTOs.SalaryReportsDTOs;
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
            CreateMap<SystemSetting, DisplaySystemSettingsDTO>().ReverseMap();
            CreateMap<OfficialHoliday, OfficialHolidayDisplayDTO>().ReverseMap();
            CreateMap<OfficialHoliday, EditOfficalHolidayDTO>().ReverseMap();

            CreateMap<Department, GetDepartmentsDTO>().ReverseMap();
            CreateMap<AddNewDepartmentDTO,Department>();
            CreateMap<UpdateExistingDepartmentDTO,Department>();
            CreateMap<SalaryReport, GetSalaryReportDTO>().AfterMap((src, dest) =>
            {
                dest.EmployeeName = src?.Employee?.FullName;
                dest.Year = src.GeneratedAt.Year;
            });
            CreateMap<Attendance, GetAttendaceDTO>().AfterMap((src, dest) =>
            {
                dest.EmployeeName = src.Employee.FullName;
                dest.DepartmentName = src.Employee.Department.DepartmentName;
            });
            CreateMap<Attendance, AddEmpAttendance>().ReverseMap();
            CreateMap<Attendance, UpdateEmployeeAttendance>().ReverseMap();
            //CreateMap<GetAttendaceDTO, AddEmpAttendance>().ReverseMap();
            //CreateMap<GetAttendaceDTO, UpdateEmployeeAttendance>().ReverseMap();
        }
    }
    
}
