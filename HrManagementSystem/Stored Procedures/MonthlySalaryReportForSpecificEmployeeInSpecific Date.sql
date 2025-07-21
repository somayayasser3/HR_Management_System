USE [HRManagementBD]
GO
/****** Object:  StoredProcedure [dbo].[GenerateMonthlySalaryReportForEmployee]    Script Date: 7/19/2025 7:43:45 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create a stored procedure to generate salary report for a specific employee
ALTER  PROCEDURE [dbo].[GenerateMonthlySalaryReportForEmployee]
    @CurrentMonth INT,
    @CurrentYear INT, 
    @EmployeeId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Variables for salary calculations
    DECLARE @WorkingDays INT;
    DECLARE @HourlyRate DECIMAL(18,2);
    DECLARE @DailyRate DECIMAL(18,2);
    DECLARE @EmployeeSalary DECIMAL(18,2);
    DECLARE @EmployeeOverTime DECIMAL(18,2);
    DECLARE @EmployeeDelayTime DECIMAL(18,2);
    DECLARE @EmployeeAttendance INT;
    Declare @Yearr int;
	select @Yearr = @CurrentYear
    -- Validate input parameters
    IF @CurrentMonth < 1 OR @CurrentMonth > 12
    BEGIN
        RAISERROR('Invalid month. Month must be between 1 and 12.', 16, 1);
        RETURN;
    END
    
    IF @CurrentYear < 1900 OR @CurrentYear > YEAR(GETDATE())
    BEGIN
        RAISERROR('Invalid year.', 16, 1);
        RETURN;
    END
    
    -- Check if employee exists
    IF NOT EXISTS (SELECT 1 FROM Employees WHERE EmployeeId = @EmployeeId)
    BEGIN
        RAISERROR('Employee with ID %d does not exist.', 16, 1, @EmployeeId);
        RETURN;
    END
    
    BEGIN TRY
        -- Calculate working days for the specified month
        SELECT @WorkingDays = DAY(EOMONTH(DATEFROMPARTS(@CurrentYear, @CurrentMonth, 1))) - 
               ISNULL(dbo.GetMonthAllHolidays(@CurrentYear, @CurrentMonth), 0);
        
        -- Reset variables for employee
        SET @EmployeeSalary = 0;
        SET @EmployeeOverTime = 0;
        SET @EmployeeDelayTime = 0;
        SET @EmployeeAttendance = 0;
        
        -- Get employee salary
        SELECT @EmployeeSalary = ISNULL(salary, 0)
        FROM Employees 
        WHERE EmployeeId = @EmployeeId;
        
        -- Validate salary
        IF @EmployeeSalary <= 0
        BEGIN
            RAISERROR('Employee salary is not set or invalid.', 16, 1);
            RETURN;
        END
        
        -- Calculate daily and hourly rates
        SET @DailyRate = @EmployeeSalary / NULLIF(@WorkingDays, 0);
        SET @HourlyRate = @DailyRate / 8;
        
        -- Get overtime hours for specified month
        SELECT @EmployeeOverTime = ISNULL(SUM(OvertimeHours), 0) 
        FROM Attendances 
        WHERE EmployeeId = @EmployeeId 
          AND MONTH(AttendanceDate) = @CurrentMonth 
          AND YEAR(AttendanceDate) = @CurrentYear;
        
        -- Get delay hours for specified month
        SELECT @EmployeeDelayTime = ISNULL(SUM(DelayHours), 0) 
        FROM Attendances 
        WHERE EmployeeId = @EmployeeId 
          AND MONTH(AttendanceDate) = @CurrentMonth 
          AND YEAR(AttendanceDate) = @CurrentYear;
        
        -- Get attendance count for specified month
        SELECT @EmployeeAttendance = COUNT(*) 
        FROM Attendances 
        WHERE EmployeeId = @EmployeeId 
          AND MONTH(AttendanceDate) = @CurrentMonth 
          AND YEAR(AttendanceDate) = @CurrentYear;
        
        -- Check if report already exists for this employee and month
        IF EXISTS (
            SELECT 1 FROM SalaryReports 
            WHERE EmployeeId = @EmployeeId 
              AND Month = @CurrentMonth 
              AND YEAR = @Yearr
        )
        BEGIN
            -- Update existing record
            UPDATE SalaryReports 
            SET 
                BasicSalary = @EmployeeSalary,
                OvertimeAmount = @EmployeeOverTime * 2 * @HourlyRate,
                DeductionAmount = @EmployeeDelayTime * 2 * @HourlyRate,
                NetSalary = (@EmployeeAttendance * @DailyRate) + (@EmployeeOverTime * 2 * @HourlyRate) - (@EmployeeDelayTime * 2 * @HourlyRate),
                GeneratedAt = GETDATE(),  -- Only update the "last modified" timestamp,
				Year = @Yearr
            WHERE EmployeeId = @EmployeeId 
              AND Month = @CurrentMonth 
              AND YEAR(GeneratedAt) = @CurrentYear;
              
            PRINT 'Salary report updated successfully for Employee ID: ' + CAST(@EmployeeId AS VARCHAR(10));
        END
        ELSE
        BEGIN
            -- Insert new salary report
            INSERT INTO SalaryReports (
                EmployeeId,
                Month,
                BasicSalary,
                OvertimeAmount,
                DeductionAmount,
                NetSalary,
                GeneratedAt,
                CreatedAt,
				Year
            )
            VALUES (
                @EmployeeId,
                @CurrentMonth, -- Fixed: was using @PreviousMonth which wasn't declared
                @EmployeeSalary,
                @EmployeeOverTime * 2 * @HourlyRate,
                @EmployeeDelayTime * 2 * @HourlyRate,
                (@EmployeeAttendance * @DailyRate) + (@EmployeeOverTime * 2 * @HourlyRate) - (@EmployeeDelayTime * 2 * @HourlyRate),
                GETDATE(),
                GETDATE(),
				@Yearr
            );
            
            PRINT 'Salary report generated successfully for Employee ID: ' + CAST(@EmployeeId AS VARCHAR(10));
        END
        
        -- Return the generated report
        --SELECT 
        --    sr.EmployeeId,
        --    e.FirstName + ' ' + e.LastName AS EmployeeName,
        --    sr.Month,
        --    @CurrentYear AS Year,
        --    sr.BasicSalary,
        --    sr.OvertimeAmount,
        --    sr.DeductionAmount,
        --    sr.NetSalary,
        --    @EmployeeAttendance AS AttendanceDays,
        --    @WorkingDays AS WorkingDays,
        --    @EmployeeOverTime AS OvertimeHours,
        --    @EmployeeDelayTime AS DelayHours,
        --    sr.GeneratedAt
        --FROM SalaryReports sr
        --INNER JOIN Employees e ON sr.EmployeeId = e.EmployeeId
        --WHERE sr.EmployeeId = @EmployeeId 
        --  AND sr.Month = @CurrentMonth 
        --  AND YEAR(sr.GeneratedAt) = @CurrentYear;
          
    END TRY
    BEGIN CATCH
        -- Error handling
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;