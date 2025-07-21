USE [HRManagementBD]
GO
/****** Object:  StoredProcedure [dbo].[GenerateMonthlySalaryReports]    Script Date: 7/19/2025 7:42:50 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Create a stored procedure to generate salary reports for all employees
CREATE OR ALTER PROCEDURE [dbo].[GenerateMonthlySalaryReports]
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
