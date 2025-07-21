-- Create a stored procedure to generate salary reports for all employees
CREATE or alter PROCEDURE GenerateMonthlySalaryReports
AS
BEGIN
    SET NOCOUNT ON;
    
 -- Variables for current month/year and employee processing
    DECLARE @CurrentMonth INT = MONTH(GETDATE());
    DECLARE @CurrentYear INT = YEAR(GETDATE());
    DECLARE @PreviousMonth INT;
    DECLARE @PreviousYear INT;
    
    -- Calculate previous month and year
    IF @CurrentMonth = 1
    BEGIN
        SET @PreviousMonth = 12;
        SET @PreviousYear = @CurrentYear - 1;
    END
    ELSE
    BEGIN
        SET @PreviousMonth = @CurrentMonth - 1;
        SET @PreviousYear = @CurrentYear;
    END
    
    -- Variables for salary calculations
    DECLARE @WorkingDays INT;
    DECLARE @HourlyRate DECIMAL(18,2);
    DECLARE @DailyRate DECIMAL(18,2);
    DECLARE @EmployeeSalary DECIMAL(18,2);
    DECLARE @EmployeeOverTime DECIMAL(18,2);
    DECLARE @EmployeeDelayTime DECIMAL(18,2);
    DECLARE @EmployeeAttendance INT;
    DECLARE @EmployeeId INT;
	Declare @Yearr int;
    
    -- Cursor to process all employees
    DECLARE employee_cursor CURSOR FOR
    SELECT EmployeeId FROM Employees --WHERE IsActive = 1; -- Assuming there's an IsActive column
    
    -- Calculate working days for the previous month
    SELECT @WorkingDays = DAY(EOMONTH(DATEFROMPARTS(@PreviousYear, @PreviousMonth, 1))) - 
                         dbo.GetMonthAllHolidays(@PreviousYear, @PreviousMonth);
    
    OPEN employee_cursor;
    FETCH NEXT FROM employee_cursor INTO @EmployeeId;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Reset variables for each employee
        SET @EmployeeSalary = 0;
        SET @EmployeeOverTime = 0;
        SET @EmployeeDelayTime = 0;
        SET @EmployeeAttendance = 0;
        
		select @Yearr = @PreviousYear
        -- Get employee salary
        SELECT @EmployeeSalary = salary 
        FROM Employees 
        WHERE EmployeeId = @EmployeeId;
        
        -- Calculate daily and hourly rates
        SELECT @DailyRate = @EmployeeSalary / @WorkingDays;
        SELECT @HourlyRate = @DailyRate / 8;
        
        -- Get overtime hours for previous month
        SELECT @EmployeeOverTime = ISNULL(SUM(OvertimeHours), 0) 
        FROM Attendances 
        WHERE EmployeeId = @EmployeeId 
          AND MONTH(AttendanceDate) = @PreviousMonth 
          AND YEAR(AttendanceDate) = @PreviousYear;
        
        -- Get delay hours for previous month
        SELECT @EmployeeDelayTime = ISNULL(SUM(DelayHours), 0) 
        FROM Attendances 
        WHERE EmployeeId = @EmployeeId 
          AND MONTH(AttendanceDate) = @PreviousMonth 
          AND YEAR(AttendanceDate) = @PreviousYear;
        
        -- Get attendance count for previous month
        SELECT @EmployeeAttendance = COUNT(*) 
        FROM Attendances 
        WHERE EmployeeId = @EmployeeId 
          AND MONTH(AttendanceDate) = @PreviousMonth 
          AND YEAR(AttendanceDate) = @PreviousYear;
        
         -- Check if report already exists for this employee and month
        IF NOT EXISTS (
            SELECT 1 FROM SalaryReports 
            WHERE EmployeeId = @EmployeeId 
              AND Month = @PreviousMonth 
              AND Year = @Yearr
        )
        BEGIN
            -- Insert salary report
            INSERT INTO SalaryReports 
            VALUES (
                @EmployeeId,
                @PreviousMonth,
                @EmployeeSalary,
                @EmployeeOverTime * 2 * @HourlyRate,
                @EmployeeDelayTime * 2 * @HourlyRate,
                (@EmployeeAttendance * @DailyRate) + (@EmployeeOverTime * 2 * @HourlyRate) - (@EmployeeDelayTime * 2 * @HourlyRate),
                GETDATE(),
                GETDATE(),
				@Yearr
            );
        END
        FETCH NEXT FROM employee_cursor INTO @EmployeeId;
    END
    
    CLOSE employee_cursor;
    DEALLOCATE employee_cursor;
END;
GO

-- Create a SQL Server Agent Job to run this procedure on the 1st of every month
-- Note: This requires SQL Server Agent and appropriate permissions

-- Step 1: Create the job
USE HRManagementBD;
GO

-- Delete job if it exists
IF EXISTS (SELECT job_id FROM msdb.dbo.sysjobs WHERE name = 'Monthly Salary Report Generation')
BEGIN
    EXEC msdb.dbo.sp_delete_job @job_name = 'Monthly Salary Report Generation';
END;
GO

-- Create the job
EXEC msdb.dbo.sp_add_job
    @job_name = 'Monthly Salary Report Generation',
    @enabled = 1,
    @description = 'Generates salary reports for all employees on the 1st of each month';
GO

-- Add job step
EXEC msdb.dbo.sp_add_jobstep
    @job_name = 'Monthly Salary Report Generation',
    @step_name = 'Generate Reports',
    @subsystem = 'TSQL',
    @command = 'EXEC GenerateMonthlySalaryReports;',
    @database_name = 'HRManagementBD'; -- Replace with your actual database name
GO

-- Create schedule to run on 1st of every month at 6:00 AM
EXEC msdb.dbo.sp_add_schedule
    @schedule_name = 'Monthly on 1st',
    @freq_type = 4,        -- Monthly
    @freq_interval = 1,    -- 1st day of month
    @freq_recurrence_factor = 1,
    @active_start_time = 060000; -- 6:00 AM
GO

-- Attach schedule to job
EXEC msdb.dbo.sp_attach_schedule
    @job_name = 'Monthly Salary Report Generation',
    @schedule_name = 'Monthly on 1st';
GO

-- Add job to target server
EXEC msdb.dbo.sp_add_jobserver
    @job_name = 'Monthly Salary Report Generation',
    @server_name = @@SERVERNAME;
GO