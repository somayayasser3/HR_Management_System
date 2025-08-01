using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HrManagementSystem.Models
{
    public class HRContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public HRContext(DbContextOptions<HRContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<GroupPermission> GroupPermissions { get; set; }
        public DbSet<SalaryReport> SalaryReports { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveType> LeaveTypes { get; set; }
        //public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances { get; set; }
        public DbSet<WorkingTask> WorkingTasks { get; set; }

        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<OfficialHoliday> OfficialHolidays { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        public async Task GenerateMonthlySalaryReportsAsync()
        {
            await Database.ExecuteSqlRawAsync("EXEC GenerateMonthlySalaryReports");
        }
        public async Task GenerateMonthlySalaryReportForEmployee(int month , int year , int employeeId)
        {
            await Database.ExecuteSqlRawAsync("EXEC GenerateMonthlySalaryReportForEmployee @CurrentMonth = {0}, @CurrentYear = {1}, @EmployeeId = {2}",
                           month, year, employeeId);
        }
        public async Task GenerateMonthlySalaryReportForAllEmployeesInSpecificDate2(int month , int year )
        {
            await Database.ExecuteSqlRawAsync("EXEC GenerateMonthlySalaryReportsForAllEmployees @CurrentMonth = {0}, @CurrentYear = {1}",
                           month, year);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User-Employee relationship (One-to-One)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Employee-Department relationship (Many-to-One)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Employee-Attendance relationship (One-to-Many)
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure SalaryReport-Employee relationship
            modelBuilder.Entity<SalaryReport>()
                .HasOne(sr => sr.Employee)
                .WithMany()
                .HasForeignKey(sr => sr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure GroupPermission relationships
            modelBuilder.Entity<GroupPermission>()
                .HasKey(gp => new { gp.GroupId, gp.PermissionId });

            modelBuilder.Entity<GroupPermission>()
                .HasOne(gp => gp.UserGroup)
                .WithMany(g => g.GroupPermissions)
                .HasForeignKey(gp => gp.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GroupPermission>()
                .HasOne(gp => gp.Permission)
                .WithMany(p => p.GroupPermissions)
                .HasForeignKey(gp => gp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LeaveType>().HasData(
            new LeaveType { Id = 1, Name = "Annual", MaxDaysPerYear = 21 },
            new LeaveType { Id = 2, Name = "Sick", MaxDaysPerYear = 15 },
            new LeaveType { Id = 3, Name = "Unpaid", MaxDaysPerYear = 0 }
                        );
            modelBuilder.Entity<User>().HasIndex(m => m.PhoneNumber).IsUnique();
            modelBuilder.Entity<Employee>().HasIndex(m => m.PhoneNumber).IsUnique();
            modelBuilder.Entity<Employee>().HasIndex(m => m.NationalId).IsUnique();
        }
    }

}

