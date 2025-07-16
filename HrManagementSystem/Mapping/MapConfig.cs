using AutoMapper;
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
            CreateMap<Department, GetDepartmentsDTO>().ReverseMap();
            CreateMap<AddNewDepartmentDTO,Department>();
            CreateMap<UpdateExistingDepartmentDTO,Department>();
            CreateMap<SalaryReport, GetSalaryReportDTO>().AfterMap((src, dest) =>
            {
                dest.EmployeeName = src?.Employee?.FullName;
            });
            CreateMap<Attendance, GetAttendaceDTO>().AfterMap((src, dest) =>
            {
                dest.EmployeeName = src.Employee.FullName;
            });
            CreateMap<Attendance, AddEmpAttendance>().ReverseMap();
        }
    }
    
}
