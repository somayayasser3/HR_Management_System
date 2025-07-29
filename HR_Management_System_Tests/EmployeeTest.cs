using AutoMapper;
using HrManagementSystem.Controllers;
using HrManagementSystem.DTOs.Employee;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text;

namespace HrManagementSystem;

[TestClass]
public class EmployeeTest
{
    private HRContext _context;
    private UnitOfWork _unitOfWork;
    private Mock<IMapper> _mapperMock;
    private Mock<UserManager<User>> _userManagerMock;
    private EmployeeController _controller;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<HRContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new HRContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _mapperMock = new Mock<IMapper>();
        _userManagerMock = MockUserManager();

        _controller = new EmployeeController(_mapperMock.Object, _unitOfWork, _userManagerMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
    }

    private static Mock<UserManager<User>> MockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
    }

    private IFormFile GetMockFormFile(string fileName = "test.jpg")
    {
        var content = "FakeImageContent";
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        return new FormFile(fileStream, 0, fileStream.Length, "Image", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    [TestMethod]
    public async Task AddEmployeeAsync_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        var dto = new AddEmployee
        {
            FullName = "Ahmed",
            NationalId = "12345678901234",
            PhoneNumber = "0123456789",
            Password = "Pass@123",
            Address = "Cairo",
            Gender = "Male",
            Salary = 10000,
            DepartmentId = 1,
            Image = GetMockFormFile()
        };

        var user = new User { Id = 1 };
        var employee = new Employee
        {
            EmployeeId = 1,
            FullName = "Ahmed",
            PhoneNumber = "0123456789",
            NationalId = "12345678901234",
            Address = "Cairo",
            Gender = "Male",
            DepartmentId = 1,
            ImagePath = "test.jpg"
        };

        _mapperMock.Setup(m => m.Map<User>(dto)).Returns(user);
        _mapperMock.Setup(m => m.Map<Employee>(dto)).Returns(employee);
        _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _controller.AddEmployeeAsync(dto);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var message = okResult.Value.GetType().GetProperty("message")?.GetValue(okResult.Value);
        Assert.AreEqual("Employee Added Successfully", message);
    }

    [TestMethod]
    public void GetAllEmployees_ShouldReturnOk()
    {
        var employees = new List<Employee>
        {
            new Employee { EmployeeId = 1, FullName = "A", NationalId = "123", PhoneNumber = "1", Gender = "Male", Address = "Cairo", ImagePath = "path" }
        };
        var displayDtos = new List<DisplayEmployeeData>
        {
            new DisplayEmployeeData { EmployeeId = 1, FullName = "A" }
        };

        _context.Employees.AddRange(employees);
        _context.SaveChanges();

        _mapperMock.Setup(m => m.Map<List<DisplayEmployeeData>>(It.IsAny<List<Employee>>()))
                   .Returns(displayDtos);

        var result = _controller.GetAllEmployees();

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.IsInstanceOfType(okResult.Value, typeof(List<DisplayEmployeeData>));
    }

    [TestMethod]
    public void GetEmployeeByID_ShouldReturnEmployee_WhenExists()
    {
        var emp = new Employee
        {
            EmployeeId = 1,
            FullName = "A",
            PhoneNumber = "123",
            Gender = "Male",
            NationalId = "11112222333344",
            Address = "Cairo",
            ImagePath = "test.jpg"
        };

        _context.Employees.Add(emp);
        _context.SaveChanges();

        _mapperMock.Setup(m => m.Map<DisplayEmployeeData>(It.IsAny<Employee>()))
                   .Returns(new DisplayEmployeeData { EmployeeId = emp.EmployeeId });

        var result = _controller.GetEmployeeByID(emp.EmployeeId);

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var returnedEmp = okResult.Value as DisplayEmployeeData;
        Assert.IsNotNull(returnedEmp);
        Assert.AreEqual(emp.EmployeeId, returnedEmp.EmployeeId);
    }

    [TestMethod]
    public void DeleteEmployee_ShouldReturnNotFound_WhenMissing()
    {
        var result = _controller.DeleteEmployee(999);
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task DeleteEmployee_ShouldReturnOk_WhenExists()
    {
        var user = new User
        {
            Id = 1,
            FullName = "John Doe",
            PhoneNumber = "0123456789",
            Address = "Cairo",
            UserName = "john",
            Email = "john@example.com"
        };

        var emp = new Employee
        {
            EmployeeId = 1,
            UserId = 1,
            FullName = "John Doe",
            NationalId = "12345678901234",
            PhoneNumber = "0123456789",
            Gender = "Male",
            Address = "Cairo",
            ImagePath = "path"
        };

        _context.Users.Add(user);
        _context.Employees.Add(emp);
        _context.SaveChanges();

        _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = _controller.DeleteEmployee(emp.EmployeeId);

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var returnedEmp = okResult.Value as Employee;
        Assert.IsNotNull(returnedEmp);
        Assert.AreEqual(emp.EmployeeId, returnedEmp.EmployeeId);
    }
}