using HrManagementSystem.Models;
using HrManagementSystem.Repositories;

namespace HrManagementSystem.UnitOfWorks
{
    public class UnitOfWork
    {
        HRContext con;
        AttendanceRepository attendanceRepo;
        WorkingTaskRepository workingTaskRepo;
        DepartmentRepository departmentRepo;
        EmployeeRepository employeeRepo;
        GroupPermissionRepository groupPermissionRepo;
        OfficialHolidayRepository officialHolidayRepo;
        PermissionRepositroy permissionRepo;
        SalaryReportRepositroy salaryReportRepo;
        SystemSettingRepository systemSettingRepo;
        UserGroupRepository userGroupRepo;
        LeaveRepository leaveRepo;
        //EmployeeLeaveBalanceRepository employeeLeaveBalanceRepo;
        ChatRepository chatRepo;
        LeaveTypeRepository leaveTypeRepo;
        public UnitOfWork(HRContext context)
        {
            con = context;
        }
        public ChatRepository ChatRepo
        {
            get
            {
                if (chatRepo == null)
                    chatRepo = new ChatRepository(con);
                return chatRepo;
            }
        }
        public LeaveTypeRepository LeaveTypeRepo 
        { 
        get
            {
                if(leaveTypeRepo==null) 
                    leaveTypeRepo = new LeaveTypeRepository(con);
                return leaveTypeRepo;
            }
        }
        public virtual AttendanceRepository AttendanceRepo
        {
            get
            {
                if (attendanceRepo == null)

                    attendanceRepo = new AttendanceRepository(con);
                return attendanceRepo;
            }
        }

        public virtual DepartmentRepository DepartmentRepo
        {
            get
            {
                if (departmentRepo == null)
                    departmentRepo = new DepartmentRepository(con);
                return departmentRepo;
            }
        }

        public virtual LeaveRepository LeaveRepo
        {
            get
            {
                if (leaveRepo == null)
                    leaveRepo = new LeaveRepository(con);
                return leaveRepo;
            }
        }
        //public virtual EmployeeLeaveBalanceRepository EmployeeLeaveBalanceRepo
        //{
        //    get
        //    {
        //        if (employeeLeaveBalanceRepo == null)
        //            employeeLeaveBalanceRepo = new EmployeeLeaveBalanceRepository(con);
        //        return employeeLeaveBalanceRepo;
        //    }
        //}

        public virtual EmployeeRepository EmployeeRepo
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

        public virtual OfficialHolidayRepository OfficialHolidayRepo
        {
            get
            {
                if (officialHolidayRepo == null)
                    officialHolidayRepo = new OfficialHolidayRepository(con);
                return officialHolidayRepo;
            }
        }
        public virtual PermissionRepositroy PermissionRepo
        {
            get
            {
                if(permissionRepo == null)
                    permissionRepo = new PermissionRepositroy(con);
                return permissionRepo;
            }        
        }

        public virtual SalaryReportRepositroy SalaryReportRepo
        {
            get
            {
                if(salaryReportRepo==null)
                    salaryReportRepo = new SalaryReportRepositroy(con);
                return salaryReportRepo;
            }
        }

        public virtual SystemSettingRepository SystemSettingRepo
        {
            get
            {
                if (systemSettingRepo == null)
                    systemSettingRepo = new SystemSettingRepository(con);
                return systemSettingRepo;
            }
        }
        public virtual UserGroupRepository UserGroupRepo
        {
            get
            {
                if (userGroupRepo == null)
                    userGroupRepo = new UserGroupRepository(con);
                return userGroupRepo;
            }
        }
        public virtual WorkingTaskRepository WorkingTaskRepo
        {
            get
            {
                if(workingTaskRepo == null)
                    workingTaskRepo = new WorkingTaskRepository(con);
                return workingTaskRepo;
            }
        }

        public void Save()
        {
            con.SaveChanges();
        }
    }
}
