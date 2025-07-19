declare @WorkingDays int;
declare @HourlyRate decimal(18,2);
declare @DailyRate decimal(18,2);
declare @EmployeeSalary decimal(18,2);
declare @EmployeeOverTime decimal(18,2);
declare @EmployeeDelayTime decimal(18,2);
declare @EmployeeAttendance int;
select @WorkingDays = Day(EOMonth(getDATE())) - dbo.GetMonthAllHolidays(2025,7) ;
select @EmployeeSalary =  salary from Employees where EmployeeId = 12;
select @DailyRate = @EmployeeSalary/@WorkingDays;
select @HourlyRate = @DailyRate/8;
select @EmployeeOverTime = SUM(OvertimeHours) from Attendances where EmployeeId = 12 and Month(AttendanceDate) = 7 and year(Attendancedate) = 2025 
select @EmployeeDelayTime = SUM(DelayHours) from Attendances where EmployeeId = 12 and Month(AttendanceDate) = 7 and year(Attendancedate) = 2025 
select @EmployeeAttendance = count(*) from  Attendances where EmployeeId = 12 and Month(AttendanceDate) = 7 and year(Attendancedate) = 2025
--select @WorkingDays 'Working days';
--select @EmployeeSalary 'Employee salary';
--Select @DailyRate 'Employee Daily Rate';
--select @HourlyRate 'Employee Hourly Rate';
--select @EmployeeOverTime 'Over Time';
--select @EmployeeDelayTime'Delay Time';
--select @EmployeeAttendance'Attendance';


insert into SalaryReports (EmployeeId,Month,BasicSalary,OvertimeAmount,DeductionAmount,NetSalary,GeneratedAt,CreatedAt)
values(12,7,@EmployeeSalary,@EmployeeOverTime *2 * @HourlyRate,@EmployeeDelayTime*2*@HourlyRate,(@EmployeeAttendance * @DailyRate)+ ( @EmployeeOverTime *2 * @HourlyRate  ) - (@EmployeeDelayTime*2*@HourlyRate),GETDATE(),GETDATE())




CREATE FUNCTION [dbo].[GetMonthAllHolidays](@Year INT, @Month INT)
RETURNS INT
AS
BEGIN
	Declare @offcialHolidays int;  
	select @offcialHolidays =  count(*)
	from  OfficialHolidays
	where Month(holidaydate) = @Month 
	AND YEAR(holidaydate)=@Year
	 AND DATENAME(WEEKDAY, holidaydate) NOT IN ('Friday', 'Saturday');

    RETURN  dbo.GetWeekendDaysInMonth(@year, @Month)  + @offcialHolidays;
END




CREATE FUNCTION [dbo].[GetWeekendDaysInMonth](@Year INT, @Month INT)
RETURNS INT
AS
BEGIN
    DECLARE @FirstOfMonth DATE = DATEFROMPARTS(@Year, @Month, 1);
    DECLARE @LastOfMonth DATE = EOMONTH(@FirstOfMonth);
    DECLARE @FirstDayOfWeek INT = DATEPART(WEEKDAY, @FirstOfMonth);
    
    -- Find first Friday and Saturday
    DECLARE @FirstFriday DATE = DATEADD(DAY, (6 - @FirstDayOfWeek + 7) % 7, @FirstOfMonth);
    DECLARE @FirstSaturday DATE = DATEADD(DAY, (7 - @FirstDayOfWeek + 7) % 7, @FirstOfMonth);
    
    -- Count occurrences mathematically
    DECLARE @FridayCount INT = DATEDIFF(DAY, @FirstFriday, @LastOfMonth) / 7 + 1;
    DECLARE @SaturdayCount INT = DATEDIFF(DAY, @FirstSaturday, @LastOfMonth) / 7 + 1;
    
    RETURN @FridayCount + @SaturdayCount;
END



---- INSERT query for attendance table
--INSERT INTO Attendances(
--    EmployeeId,
--    CheckInTime,
--    CheckOutTime,
--    OvertimeHours,
--    DelayHours,
--    CreatedAt,
--    UpdatedAt,
--    AttendanceDate
--) VALUES (                            -- AttendanceId (int)
--    11,                            -- EmployeeId (int)
--    '09:00:00',                     -- CheckInTime (time)
--    '17:30:00',                     -- CheckOutTime (time) - nullable
--    2.50,                           -- OvertimeHours (decimal(5,2))
--    0.00,                           -- DelayHours (decimal(5,2))
--    '2025-07-19 08:45:30',         -- CreatedAt (datetime2)
--    '2025-07-19 08:45:30',         -- UpdatedAt (datetime2)
--    '2025-07-19'                    -- AttendanceDate (date)
--);