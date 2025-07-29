using AutoMapper;
using HrManagementSystem.Controllers;
using HrManagementSystem.DTOs.WorkingTaskDTOs;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace HR_Management_System_Tests;

[TestClass]
public class WorkingTaskTests
{
    private HRContext _context;
    private UnitOfWork _unitOfWork;
    private Mock<IMapper> _mockMapper;
    private WorkingTaskController _controller;

    [TestInitialize]
    public void WorkingTaskInit()
    {
        var options = new DbContextOptionsBuilder<HRContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new HRContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _mockMapper = new Mock<IMapper>();
        _controller = new WorkingTaskController(_unitOfWork, _mockMapper.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }

    [TestMethod]
    public async Task AddNewWorkingTask_ValidTask_ReturnsOkWithTask()
    {
        // Arrange
        var department = new Department
        {
            DepartmentId = 1,
            DepartmentName = "IT",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Description = "IT DEPT"
        };

        var employee = new Employee
        {
            EmployeeId = 1,
            DepartmentId = 1,
            UserId = 101,
            FullName = "John Doe",
            PhoneNumber = "1234567890",
            NationalId = "12345678901234",
            Address = "Cairo",
            Gender = "Male",
            ImagePath = "C:\\john.jpg"
        };

        var taskDTO = new AddWorkingTaskDTO
        {
            EmployeeId = 1,
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(5)
        };

        var workingTask = new WorkingTask
        {
            Id = 1,
            EmployeeId = 1,
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(5),
            Status = "Pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Employee = employee
        };

        var displayTaskDTO = new DisplayWorkingTaskDTO
        {
            TaskId = 1,
            EmployeeId = 1,
            EmployeeName = "John Doe",
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(5),
            Status = "Pending",
            DepartmentName = "IT"
        };

        _context.Departments.Add(department);
        _context.Employees.Add(employee);
        _context.SaveChanges();

        _mockMapper.Setup(m => m.Map<WorkingTask>(taskDTO))
            .Returns(new WorkingTask
            {
                EmployeeId = 1,
                Description = "Complete project documentation",
                DueDate = DateTime.Now.AddDays(5)
            });

        _mockMapper.Setup(m => m.Map<DisplayWorkingTaskDTO>(It.IsAny<WorkingTask>()))
            .Returns(displayTaskDTO);

        // Act
        var result = await _controller.AddNewWorkingTask(taskDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        var returnedTask = okResult.Value as DisplayWorkingTaskDTO;
        Assert.IsNotNull(returnedTask);
        Assert.AreEqual(1, returnedTask.EmployeeId);
        Assert.AreEqual("Complete project documentation", returnedTask.Description);
    }

    [TestMethod]
    public async Task AddNewWorkingTask_NullTask_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.AddNewWorkingTask(null);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = result as BadRequestObjectResult;
        var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
        Assert.AreEqual("Empty task", message);
    }

    [TestMethod]
    public async Task AddNewWorkingTask_EmployeeNotFound_ReturnsBadRequest()
    {
        // Arrange
        var taskDTO = new AddWorkingTaskDTO
        {
            EmployeeId = 999,
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(5)
        };

        // Act
        var result = await _controller.AddNewWorkingTask(taskDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = result as BadRequestObjectResult;
        var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
        Assert.AreEqual("Employee Not found", message);
    }

    [TestMethod]
    public async Task AddNewWorkingTask_PastDueDate_ReturnsBadRequest()
    {
        // Arrange
        var department = new Department
        {
            DepartmentId = 1,
            DepartmentName = "IT",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Description = "IT DEPT"
        };

        var employee = new Employee
        {
            EmployeeId = 1,
            DepartmentId = 1,
            UserId = 101,
            FullName = "John Doe",
            PhoneNumber = "1234567890",
            NationalId = "12345678901234",
            Address = "Cairo",
            Gender = "Male",
            ImagePath = "C:\\john.jpg"
        };

        var taskDTO = new AddWorkingTaskDTO
        {
            EmployeeId = 1,
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(-1) // Past date
        };

        _context.Departments.Add(department);
        _context.Employees.Add(employee);
        _context.SaveChanges();

        // Act
        var result = await _controller.AddNewWorkingTask(taskDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = result as BadRequestObjectResult;
        var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
        Assert.AreEqual("Can't add past date tasks", message);
    }

    [TestMethod]
    public async Task GetWorkingTasks_WithTasks_ReturnsOkWithTasksList()
    {
        // Arrange
        var department = new Department
        {
            DepartmentId = 1,
            DepartmentName = "IT",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Description = "IT DEPT"
        };

        var employee = new Employee
        {
            EmployeeId = 1,
            DepartmentId = 1,
            UserId = 101,
            FullName = "John Doe",
            PhoneNumber = "1234567890",
            NationalId = "12345678901234",
            Address = "Cairo",
            Gender = "Male",
            ImagePath = "C:\\john.jpg"
        };

        var workingTask = new WorkingTask
        {
            Id = 1,
            EmployeeId = 1,
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(5),
            Status = "Pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Employee = employee
        };

        var displayTaskDTOs = new List<DisplayWorkingTaskDTO>
        {
            new DisplayWorkingTaskDTO
            {
                TaskId = 1,
                EmployeeId = 1,
                EmployeeName = "John Doe",
                Description = "Complete project documentation",
                DueDate = DateTime.Now.AddDays(5),
                Status = "Pending",
                DepartmentName = "IT"
            }
        };

        _context.Departments.Add(department);
        _context.Employees.Add(employee);
        _context.WorkingTasks.Add(workingTask);
        _context.SaveChanges();

        _mockMapper.Setup(m => m.Map<List<DisplayWorkingTaskDTO>>(It.IsAny<List<WorkingTask>>()))
            .Returns(displayTaskDTOs);

        // Act
        var result = await _controller.GetWorkingTasks();

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        var returnedTasks = okResult.Value as List<DisplayWorkingTaskDTO>;
        Assert.AreEqual(1, returnedTasks.Count);
    }

    [TestMethod]
    public async Task GetWorkingTasks_NoTasks_ReturnsBadRequest()
    {
        // Arrange
        _mockMapper.Setup(m => m.Map<List<DisplayWorkingTaskDTO>>(It.IsAny<List<WorkingTask>>()))
            .Returns((List<DisplayWorkingTaskDTO>)null);

        // Act
        var result = await _controller.GetWorkingTasks();

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = result as BadRequestObjectResult;
        var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
        Assert.AreEqual("No tasks", message);
    }

    [TestMethod]
    public async Task GetTask_ValidId_ReturnsOkWithTask()
    {
        // Arrange
        var department = new Department
        {
            DepartmentId = 1,
            DepartmentName = "IT",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Description = "IT DEPT"
        };

        var employee = new Employee
        {
            EmployeeId = 1,
            DepartmentId = 1,
            UserId = 101,
            FullName = "John Doe",
            PhoneNumber = "1234567890",
            NationalId = "12345678901234",
            Address = "Cairo",
            Gender = "Male",
            ImagePath = "C:\\john.jpg"
        };

        var workingTask = new WorkingTask
        {
            Id = 1,
            EmployeeId = 1,
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(5),
            Status = "Pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Employee = employee
        };

        var displayTaskDTO = new DisplayWorkingTaskDTO
        {
            TaskId = 1,
            EmployeeId = 1,
            EmployeeName = "John Doe",
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(5),
            Status = "Pending",
            DepartmentName = "IT"
        };

        _context.Departments.Add(department);
        _context.Employees.Add(employee);
        _context.WorkingTasks.Add(workingTask);
        _context.SaveChanges();

        _mockMapper.Setup(m => m.Map<DisplayWorkingTaskDTO>(It.IsAny<WorkingTask>()))
            .Returns(displayTaskDTO);

        // Act
        var result = await _controller.GetTask(1);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        var returnedTask = okResult.Value as DisplayWorkingTaskDTO;
        Assert.IsNotNull(returnedTask);
        Assert.AreEqual(1, returnedTask.TaskId);
    }

    [TestMethod]
    public async Task GetTask_TaskNotFound_ReturnsBadRequest()
    {
        // Arrange
        _mockMapper.Setup(m => m.Map<DisplayWorkingTaskDTO>(It.IsAny<WorkingTask>()))
            .Returns((DisplayWorkingTaskDTO)null);

        // Act
        var result = await _controller.GetTask(999);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = result as BadRequestObjectResult;
        var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
        Assert.AreEqual("No such task", message);
    }

    [TestMethod]
    public async Task GetAllEmployeeTasks_ValidEmployee_ReturnsOkWithTasks()
    {
        // Arrange
        var department = new Department
        {
            DepartmentId = 1,
            DepartmentName = "IT",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Description = "IT DEPT"
        };

        var employee = new Employee
        {
            EmployeeId = 1,
            DepartmentId = 1,
            UserId = 101,
            FullName = "John Doe",
            PhoneNumber = "1234567890",
            NationalId = "12345678901234",
            Address = "Cairo",
            Gender = "Male",
            ImagePath = "C:\\john.jpg"
        };

        var workingTask = new WorkingTask
        {
            Id = 1,
            EmployeeId = 1,
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(5),
            Status = "Pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Employee = employee
        };

        var displayTaskDTOs = new List<DisplayWorkingTaskDTO>
        {
            new DisplayWorkingTaskDTO
            {
                TaskId = 1,
                EmployeeId = 1,
                EmployeeName = "John Doe",
                Description = "Complete project documentation",
                DueDate = DateTime.Now.AddDays(5),
                Status = "Pending",
                DepartmentName = "IT"
            }
        };

        _context.Departments.Add(department);
        _context.Employees.Add(employee);
        _context.WorkingTasks.Add(workingTask);
        _context.SaveChanges();

        _mockMapper.Setup(m => m.Map<List<DisplayWorkingTaskDTO>>(It.IsAny<List<WorkingTask>>()))
            .Returns(displayTaskDTOs);

        // Act
        var result = await _controller.GetAllEmployeeTasks(1);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        var returnedTasks = okResult.Value as List<DisplayWorkingTaskDTO>;
        Assert.AreEqual(1, returnedTasks.Count);
    }

    [TestMethod]
    public async Task GetAllEmployeeTasks_EmployeeNotFound_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetAllEmployeeTasks(999);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = result as BadRequestObjectResult;
        var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
        Assert.AreEqual("No such employee", message);
    }

    [TestMethod]
    public async Task GetAllEmployeeTasks_NoTasks_ReturnsBadRequest()
    {
        // Arrange
        var department = new Department
        {
            DepartmentId = 1,
            DepartmentName = "IT",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Description = "IT DEPT"
        };

        var employee = new Employee
        {
            EmployeeId = 1,
            DepartmentId = 1,
            UserId = 101,
            FullName = "John Doe",
            PhoneNumber = "1234567890",
            NationalId = "12345678901234",
            Address = "Cairo",
            Gender = "Male",
            ImagePath = "C:\\john.jpg"
        };

        _context.Departments.Add(department);
        _context.Employees.Add(employee);
        _context.SaveChanges();

        _mockMapper.Setup(m => m.Map<List<DisplayWorkingTaskDTO>>(It.IsAny<List<WorkingTask>>()))
            .Returns((List<DisplayWorkingTaskDTO>)null);

        // Act
        var result = await _controller.GetAllEmployeeTasks(1);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = result as BadRequestObjectResult;
        var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
        Assert.AreEqual("Employee has no tasks", message);
    }

    [TestMethod]
    public async Task EmployeeUpdateTask_ValidUpdate_ReturnsOkWithStatus()
    {
        // Arrange
        var workingTask = new WorkingTask
        {
            Id = 1,
            EmployeeId = 1,
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(5),
            Status = "Pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var updateDTO = new EmployeeUpdatesTaskDTO
        {
            TaskId = 1,
            Status = "In Progress"
        };

        _context.WorkingTasks.Add(workingTask);
        _context.SaveChanges();

        // Act
        var result = await _controller.EmployeeUpdateTask(updateDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        var status = okResult.Value.GetType().GetProperty("status")?.GetValue(okResult.Value);
        Assert.AreEqual("In Progress", status);
    }

    [TestMethod]
    public async Task EmployeeUpdateTask_TaskNotFound_ReturnsBadRequest()
    {
        // Arrange
        var updateDTO = new EmployeeUpdatesTaskDTO
        {
            TaskId = 999,
            Status = "In Progress"
        };

        // Act
        var result = await _controller.EmployeeUpdateTask(updateDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = result as BadRequestObjectResult;
        var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
        Assert.AreEqual("No such task", message);
    }

    [TestMethod]
    public async Task EmployeeUpdateTask_LateTask_SetsStatusToLate()
    {
        // Arrange
        var workingTask = new WorkingTask
        {
            Id = 1,
            EmployeeId = 1,
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(-1), // Past due date
            Status = "Pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var updateDTO = new EmployeeUpdatesTaskDTO
        {
            TaskId = 1,
            Status = "Completed"
        };

        _context.WorkingTasks.Add(workingTask);
        _context.SaveChanges();

        // Act
        var result = await _controller.EmployeeUpdateTask(updateDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        var status = okResult.Value.GetType().GetProperty("status")?.GetValue(okResult.Value);
        Assert.AreEqual("Late", status);
    }

    [TestMethod]
    public async Task AdminUpdatesTask_ValidUpdate_ReturnsOkWithMessage()
    {
        // Arrange
        var department = new Department
        {
            DepartmentId = 1,
            DepartmentName = "IT",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Description = "IT DEPT"
        };

        var employee = new Employee
        {
            EmployeeId = 1,
            DepartmentId = 1,
            UserId = 101,
            FullName = "John Doe",
            PhoneNumber = "1234567890",
            NationalId = "12345678901234",
            Address = "Cairo",
            Gender = "Male",
            ImagePath = "C:\\john.jpg"
        };

        var workingTask = new WorkingTask
        {
            Id = 1,
            EmployeeId = 1,
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(5),
            Status = "Pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var updateDTO = new AdminUpdatesTaskDTO
        {
            Id = 1,
            EmployeeId = 1,
            Description = "Updated description",
            DueDate = DateTime.Now.AddDays(7),
            Status = "In Progress"
        };

        _context.Departments.Add(department);
        _context.Employees.Add(employee);
        _context.WorkingTasks.Add(workingTask);
        _context.SaveChanges();

        _mockMapper.Setup(m => m.Map(updateDTO, It.IsAny<WorkingTask>()))
            .Returns(workingTask)
            .Callback<AdminUpdatesTaskDTO, WorkingTask>((src, dest) =>
            {
                dest.Description = src.Description;
                dest.DueDate = src.DueDate;
            });

        // Act
        var result = await _controller.AdminUpdatesTask(updateDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        var message = okResult.Value.GetType().GetProperty("message")?.GetValue(okResult.Value);
        Assert.AreEqual("Task updated", message);
    }

    [TestMethod]
    public async Task AdminUpdatesTask_TaskNotFound_ReturnsBadRequest()
    {
        // Arrange
        var updateDTO = new AdminUpdatesTaskDTO
        {
            Id = 999,
            EmployeeId = 1,
            Description = "Updated description",
            DueDate = DateTime.Now.AddDays(7),
            Status = "In Progress"
        };

        // Act
        var result = await _controller.AdminUpdatesTask(updateDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = result as BadRequestObjectResult;
        var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
        Assert.AreEqual("No such task", message);
    }

    [TestMethod]
    public async Task AdminUpdatesTask_EmployeeNotFound_ReturnsNotFound()
    {
        // Arrange
        var workingTask = new WorkingTask
        {
            Id = 1,
            EmployeeId = 1,
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(5),
            Status = "Pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var updateDTO = new AdminUpdatesTaskDTO
        {
            Id = 1,
            EmployeeId = 999, // Non-existent employee
            Description = "Updated description",
            DueDate = DateTime.Now.AddDays(7),
            Status = "In Progress"
        };

        _context.WorkingTasks.Add(workingTask);
        _context.SaveChanges();

        // Act
        var result = await _controller.AdminUpdatesTask(updateDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        var notFoundResult = result as NotFoundObjectResult;
        var message = notFoundResult.Value.GetType().GetProperty("message")?.GetValue(notFoundResult.Value);
        Assert.AreEqual("Employee not found", message);
    }

    [TestMethod]
    public async Task AdminUpdatesTask_Exception_ReturnsBadRequest()
    {
        // Arrange
        var department = new Department
        {
            DepartmentId = 1,
            DepartmentName = "IT",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Description = "IT DEPT"
        };

        var employee = new Employee
        {
            EmployeeId = 1,
            DepartmentId = 1,
            UserId = 101,
            FullName = "John Doe",
            PhoneNumber = "1234567890",
            NationalId = "12345678901234",
            Address = "Cairo",
            Gender = "Male",
            ImagePath = "C:\\john.jpg"
        };

        var workingTask = new WorkingTask
        {
            Id = 1,
            EmployeeId = 1,
            Description = "Complete project documentation",
            DueDate = DateTime.Now.AddDays(5),
            Status = "Pending",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var updateDTO = new AdminUpdatesTaskDTO
        {
            Id = 1,
            EmployeeId = 1,
            Description = "Updated description",
            DueDate = DateTime.Now.AddDays(7),
            Status = "In Progress"
        };

        _context.Departments.Add(department);
        _context.Employees.Add(employee);
        _context.WorkingTasks.Add(workingTask);
        _context.SaveChanges();

        _mockMapper.Setup(m => m.Map(updateDTO, It.IsAny<WorkingTask>()))
            .Throws(new Exception("Mapping error"));

        // Act
        var result = await _controller.AdminUpdatesTask(updateDTO);

        // Assert
        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        var badRequestResult = result as BadRequestObjectResult;
        var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
        Assert.AreEqual("Try again", message);
    }
}