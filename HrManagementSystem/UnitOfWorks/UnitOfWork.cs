using HrManagementSystem.Models;
using HrManagementSystem.Repositories;

namespace HrManagementSystem.UnitOfWorks
{
    public class UnitOfWork
    {
        HRContext con;
        AttendanceRepository attendanceRepo;
        DepartmentRepository departmentRepo;
        EmployeeRepository employeeRepo;
        GroupPermissionRepository groupPermissionRepo;
        OfficialHolidayRepository officialHolidayRepo;
        PermissionRepositroy permissionRepo;
        SalaryReportRepositroy salaryReportRepo;
        SystemSettingRepository systemSettingRepo;
        UserGroupRepository userGroupRepo;
        public UnitOfWork(HRContext context)
        {
            con = context;
        }

        public AttendanceRepository AttendanceRepo
        {
            get
            {
                if (attendanceRepo == null)

                    attendanceRepo = new AttendanceRepository(con);
                return attendanceRepo;
            }
        }

        public DepartmentRepository DepartmentRepo
        {
            get
            {
                if (departmentRepo == null)
                    departmentRepo = new DepartmentRepository(con);
                return departmentRepo;
            }
        }


        public EmployeeRepository EmployeeRepo
        {
            get
            {
                if (employeeRepo == null)
                    employeeRepo = new EmployeeRepository(con);
                return employeeRepo;
            }
        }

        public GroupPermissionRepository GroupPermissionRepo {
            get
            {
                if (groupPermissionRepo == null)
                    groupPermissionRepo = new GroupPermissionRepository(con);
                return groupPermissionRepo;
            }
        }

        public OfficialHolidayRepository OfficialHolidayRepo
        {
            get
            {
                if (officialHolidayRepo == null)
                    officialHolidayRepo = new OfficialHolidayRepository(con);
                return officialHolidayRepo;
            }
        }
        public PermissionRepositroy PermissionRepo
        {
            get
            {
                if(permissionRepo == null)
                    permissionRepo = new PermissionRepositroy(con);
                return permissionRepo;
            }        
        }

        public SalaryReportRepositroy SalaryReportRepo
        {
            get
            {
                if(salaryReportRepo==null)
                    salaryReportRepo = new SalaryReportRepositroy(con);
                return salaryReportRepo;
            }
        }

        public SystemSettingRepository SystemSettingRepo
        {
            get
            {
                if (systemSettingRepo == null)
                    systemSettingRepo = new SystemSettingRepository(con);
                return systemSettingRepo;
            }
        }
        public UserGroupRepository UserGroupRepo
        {
            get
            {
                if (userGroupRepo == null)
                    userGroupRepo = new UserGroupRepository(con);
                return userGroupRepo;
            }
        }

        public void Save()
        {
            con.SaveChanges();
        }
    }
}
