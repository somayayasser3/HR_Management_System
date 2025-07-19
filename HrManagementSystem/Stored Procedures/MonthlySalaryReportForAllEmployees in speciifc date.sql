USE [HRManagementBD]
GO
/****** Object:  StoredProcedure [dbo].[GenerateMonthlySalaryReportsForAllEmployees]    Script Date: 7/19/2025 7:44:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Create a stored procedure to generate salary reports for all employees
ALTER   PROCEDURE [dbo].[GenerateMonthlySalaryReportsForAllEmployees]
    @CurrentMonth INT,
    @CurrentYear INT
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
    DECLARE @EmployeeId INT;
    DECLARE @ProcessedCount INT = 0;
    DECLARE @ErrorCount INT = 0;
    
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
    
    BEGIN TRY
        -- Calculate working days for the specified month (once for all employees)
        SELECT @WorkingDays = DAY(EOMONTH(DATEFROMPARTS(@CurrentYear, @CurrentMonth, 1))) - 
               ISNULL(dbo.GetMonthAllHolidays(@CurrentYear, @CurrentMonth), 0);
        
        -- Validate working days
        IF @WorkingDays <= 0
        BEGIN
            RAISERROR('Invalid working days calculated for the specified month.', 16, 1);
            RETURN;
        END
        
        -- Cursor to process all active employees
        DECLARE employee_cursor CURSOR FOR
        SELECT EmployeeId 
        FROM Employees 
       
          where ISNULL(Salary, 0) > 0;  -- Only process employees with valid salary
        
        OPEN employee_cursor;
        FETCH NEXT FROM employee_cursor INTO @EmployeeId;
        
        WHILE @@FETCH_STATUS = 0
        BEGIN
            BEGIN TRY
                -- Reset variables for each employee
                SET @EmployeeSalary = 0;
                SET @EmployeeOverTime = 0;
                SET @EmployeeDelayTime = 0;
                SET @EmployeeAttendance = 0;
                
                -- Get employee salary
                SELECT @EmployeeSalary = ISNULL(salary, 0)
                FROM Employees 
                WHERE EmployeeId = @EmployeeId;
                
                -- Skip if salary is invalid
                IF @EmployeeSalary <= 0
                BEGIN
                    PRINT 'Warning: Employee ID ' + CAST(@EmployeeId AS VARCHAR(10)) + ' has invalid salary. Skipping.';
                    GOTO NEXT_EMPLOYEE;
                END
                
                -- Calculate daily and hourly rates
                SET @DailyRate = @EmployeeSalary / @WorkingDays;
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
                      AND Year = @CurrentYear
                )
                BEGIN
                    -- Update existing record
                    UPDATE SalaryReports 
                    SET 
                        BasicSalary = @EmployeeSalary,
                        OvertimeAmount = @EmployeeOverTime * 2 * @HourlyRate,
                        DeductionAmount = @EmployeeDelayTime * 2 * @HourlyRate,
                        NetSalary = (@EmployeeAttendance * @DailyRate) + (@EmployeeOverTime * 2 * @HourlyRate) - (@EmployeeDelayTime * 2 * @HourlyRate),
                        GeneratedAt = GETDATE(),
                        Year = @CurrentYear
                    WHERE EmployeeId = @EmployeeId 
                      AND Month = @CurrentMonth 
                      AND Year = @CurrentYear;
                      
                    PRINT 'Salary report updated for Employee ID: ' + CAST(@EmployeeId AS VARCHAR(10));
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
                        @CurrentMonth,
                        @EmployeeSalary,
                        @EmployeeOverTime * 2 * @HourlyRate,
                        @EmployeeDelayTime * 2 * @HourlyRate,
                        (@EmployeeAttendance * @DailyRate) + (@EmployeeOverTime * 2 * @HourlyRate) - (@EmployeeDelayTime * 2 * @HourlyRate),
                        GETDATE(),
                        GETDATE(),
                        @CurrentYear
                    );
                    
                    PRINT 'Salary report generated for Employee ID: ' + CAST(@EmployeeId AS VARCHAR(10));
                END
                
                SET @ProcessedCount = @ProcessedCount + 1;
                
            END TRY
            BEGIN CATCH
                -- Handle individual employee errors without stopping the entire process
                SET @ErrorCount = @ErrorCount + 1;
                PRINT 'Error processing Employee ID ' + CAST(@EmployeeId AS VARCHAR(10)) + ': ' + ERROR_MESSAGE();
            END CATCH
            
            NEXT_EMPLOYEE:
            FETCH NEXT FROM employee_cursor INTO @EmployeeId;
        END
        
        CLOSE employee_cursor;
        DEALLOCATE employee_cursor;
        
       
    END TRY
    BEGIN CATCH
        -- Handle main procedure errors
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        -- Clean up cursor if it exists
        IF CURSOR_STATUS('local', 'employee_cursor') >= -1
        BEGIN
            CLOSE employee_cursor;
            DEALLOCATE employee_cursor;
        END
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;