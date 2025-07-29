using AutoMapper;
using HrManagementSystem.Controllers;
using HrManagementSystem.DTOs.AttendaceDTOs;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace HR_Management_System_Tests
{
    [TestClass]
    public sealed class AttendanceTest
    {
        private HRContext _context;
        private UnitOfWork _unitOfWork;
        private Mock<IMapper> _mockMapper;
        private AttendanceController _controller;
        [TestInitialize]
        public void AttendanceInit()
        {
            var options = new DbContextOptionsBuilder<HRContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new HRContext(options);
            _unitOfWork = new UnitOfWork(_context); 
            _mockMapper = new Mock<IMapper>();
            _controller = new AttendanceController(_unitOfWork, _mockMapper.Object);
        }
        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }
        [TestMethod]
        public void GetMyAttendance_ValidEmployee_ReturnsOkWithAttendanceData()
        {
            // Arrange
            var userId = 123;
            var employeeId = 456;

            // Add test data to in-memory database
            var employee = new Employee
            {
                EmployeeId = employeeId,
                UserId = userId,
                FullName = "John Doe",
                PhoneNumber = "1234567890",
                NationalId = "12345678901234",
                Address = "Cairo",
                Gender = "Male",
                ImagePath = "C:\\abc.jpg"
            };

            var attendanceList = new List<Attendance>
            {
                new Attendance { AttendanceId = 1, EmployeeId = employeeId, AttendanceDate = DateTime.Now },
                new Attendance { AttendanceId = 2, EmployeeId = employeeId, AttendanceDate = DateTime.Now.AddDays(-1) }
            };

            var attendanceDTOList = new List<GetAttendaceDTO>
            {
                new GetAttendaceDTO { AttendanceId = 1, EmployeeId = employeeId },
                new GetAttendaceDTO { AttendanceId = 2, EmployeeId = employeeId }
            };

            _context.Employees.Add(employee);
            _context.Attendances.AddRange(attendanceList);
            _context.SaveChanges();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _mockMapper.Setup(m => m.Map<List<GetAttendaceDTO>>(It.IsAny<List<Attendance>>()))
                .Returns(attendanceDTOList);

            // Act
            var result = _controller.GetMyAttendance();
            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsInstanceOfType(okResult.Value, typeof(List<GetAttendaceDTO>));
            var returnedAttendance = okResult.Value as List<GetAttendaceDTO>;
            Assert.AreEqual(2, returnedAttendance.Count);
        }

        [TestMethod]
        public void GetMyAttendance_EmployeeNotFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 999; // Non-existent user

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = _controller.GetMyAttendance();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult.Value);
        }

        [TestMethod]
        public void GetMyAttendance_ValidEmployeeNoAttendance_ReturnsEmptyList()
        {
            // Arrange
            var userId = 123;
            var employeeId = 456;

            var employee = new Employee
            {
                EmployeeId = employeeId,
                UserId = userId,
                FullName = "John Doe",
                PhoneNumber = "1234567890",
                NationalId = "12345678901234",
                Address = "Cairo",
                Gender = "Male",
                ImagePath = "C:\\abc.jpg"
            };

            _context.Employees.Add(employee);
            _context.SaveChanges();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _mockMapper.Setup(m => m.Map<List<GetAttendaceDTO>>(It.IsAny<List<Attendance>>()))
                .Returns(new List<GetAttendaceDTO>());

            // Act
            var result = _controller.GetMyAttendance();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var returnedAttendance = okResult.Value as List<GetAttendaceDTO>;
            Assert.AreEqual(0, returnedAttendance.Count);
        }

        [TestMethod]
        public void GetMyAttendance_InvalidUserId_ThrowsException()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "invalid_user_id")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act & Assert
            Assert.ThrowsException<FormatException>(() => _controller.GetMyAttendance());
        }
        [TestMethod]
        public void GetAllAttendance_WithAttendanceData_ReturnsOkWithAllAttendance()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee
                {
                    EmployeeId = 1,
                    UserId = 101,
                    FullName = "John Doe",
                    PhoneNumber = "1234567890",
                    NationalId = "12345678901234",
                    Address = "Cairo",
                    Gender = "Male",
                    ImagePath = "C:\\john.jpg"
                },
                new Employee
                {
                    EmployeeId = 2,
                    UserId = 102,
                    FullName = "Jane Smith",
                    PhoneNumber = "0987654321",
                    NationalId = "43210987654321",
                    Address = "Alexandria",
                    Gender = "Female",
                    ImagePath = "C:\\jane.jpg"
                }
            };

            var attendanceList = new List<Attendance>
            {
                new Attendance
                {
                    AttendanceId = 1,
                    EmployeeId = 1,
                    AttendanceDate = DateTime.Now,
                    Employee = employees[0]
                },
                new Attendance
                {
                    AttendanceId = 2,
                    EmployeeId = 2,
                    AttendanceDate = DateTime.Now.AddDays(-1),
                    Employee = employees[1]
                },
                new Attendance
                {
                    AttendanceId = 3,
                    EmployeeId = 1,
                    AttendanceDate = DateTime.Now.AddDays(-2),
                    Employee = employees[0]
                }
            };

            var attendanceDTOList = new List<GetAttendaceDTO>
            {
                new GetAttendaceDTO { AttendanceId = 1, EmployeeId = 1, EmployeeName = "John Doe" },
                new GetAttendaceDTO { AttendanceId = 2, EmployeeId = 2, EmployeeName = "Jane Smith" },
                new GetAttendaceDTO { AttendanceId = 3, EmployeeId = 1, EmployeeName = "John Doe" }
            };

            _context.Employees.AddRange(employees);
            _context.Attendances.AddRange(attendanceList);
            _context.SaveChanges();

            _mockMapper.Setup(m => m.Map<List<GetAttendaceDTO>>(It.IsAny<List<Attendance>>()))
                .Returns(attendanceDTOList);

            // Act
            var result = _controller.GetAllAttendance();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsInstanceOfType(okResult.Value, typeof(List<GetAttendaceDTO>));
            var returnedAttendance = okResult.Value as List<GetAttendaceDTO>;
            Assert.AreEqual(3, returnedAttendance.Count);
        }

        [TestMethod]
        public void GetAllAttendance_NoAttendanceData_ReturnsNotFound()
        {
            // Arrange
            _mockMapper.Setup(m => m.Map<List<GetAttendaceDTO>>(It.IsAny<List<Attendance>>()))
                .Returns(new List<GetAttendaceDTO>());

            // Act
            var result = _controller.GetAllAttendance();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult.Value);

            // Check the message
            var message = notFoundResult.Value.GetType().GetProperty("message")?.GetValue(notFoundResult.Value);
            Assert.AreEqual("No Attendance Founded.", message);
        }

        [TestMethod]
        public void GetAttendanceForEmployee_ValidEmployeeWithAttendance_ReturnsOkWithAttendance()
        {
            // Arrange
            var employeeId = 1;
            var employee = new Employee
            {
                EmployeeId = employeeId,
                UserId = 101,
                FullName = "John Doe",
                PhoneNumber = "1234567890",
                NationalId = "12345678901234",
                Address = "Cairo",
                Gender = "Male",
                ImagePath = "C:\\john.jpg"
            };

            var attendanceList = new List<Attendance>
            {
                new Attendance
                {
                    AttendanceId = 1,
                    EmployeeId = employeeId,
                    AttendanceDate = DateTime.Now,
                    Employee = employee
                },
                new Attendance
                {
                    AttendanceId = 2,
                    EmployeeId = employeeId,
                    AttendanceDate = DateTime.Now.AddDays(-1),
                    Employee = employee
                }
            };

            var attendanceDTOList = new List<GetAttendaceDTO>
            {
                new GetAttendaceDTO { AttendanceId = 1, EmployeeId = employeeId, EmployeeName = "John Doe" },
                new GetAttendaceDTO { AttendanceId = 2, EmployeeId = employeeId, EmployeeName = "John Doe" }
            };

            _context.Employees.Add(employee);
            _context.Attendances.AddRange(attendanceList);
            _context.SaveChanges();

            _mockMapper.Setup(m => m.Map<List<GetAttendaceDTO>>(It.IsAny<List<Attendance>>()))
                .Returns(attendanceDTOList);

            // Act
            var result = _controller.GetAttendanceForEmployee(employeeId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsInstanceOfType(okResult.Value, typeof(List<GetAttendaceDTO>));
            var returnedAttendance = okResult.Value as List<GetAttendaceDTO>;
            Assert.AreEqual(2, returnedAttendance.Count);
            Assert.IsTrue(returnedAttendance.All(a => a.EmployeeId == employeeId));
        }

        [TestMethod]
        public void GetAttendanceForEmployee_EmployeeNotFound_ReturnsNotFound()
        {
            // Arrange
            var nonExistentEmployeeId = 999;

            _mockMapper.Setup(m => m.Map<List<GetAttendaceDTO>>(It.IsAny<List<Attendance>>()))
                .Returns(new List<GetAttendaceDTO>());

            // Act
            var result = _controller.GetAttendanceForEmployee(nonExistentEmployeeId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult.Value);

            // Check the message
            var message = notFoundResult.Value.GetType().GetProperty("message")?.GetValue(notFoundResult.Value);
            Assert.AreEqual("No such employee", message);
        }

        [TestMethod]
        public void GetAttendanceForEmployee_ValidEmployeeNoAttendance_ReturnsNotFound()
        {
            // Arrange
            var employeeId = 1;
            var employee = new Employee
            {
                EmployeeId = employeeId,
                UserId = 101,
                FullName = "John Doe",
                PhoneNumber = "1234567890",
                NationalId = "12345678901234",
                Address = "Cairo",
                Gender = "Male",
                ImagePath = "C:\\john.jpg"
            };

            _context.Employees.Add(employee);
            _context.SaveChanges();

            _mockMapper.Setup(m => m.Map<List<GetAttendaceDTO>>(It.IsAny<List<Attendance>>()))
                .Returns(new List<GetAttendaceDTO>());

            // Act
            var result = _controller.GetAttendanceForEmployee(employeeId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult.Value);

            // Check the message
            var message = notFoundResult.Value.GetType().GetProperty("message")?.GetValue(notFoundResult.Value);
            Assert.AreEqual("Employee doesn't have any attendance", message);
        }

        [TestMethod]
        public void GetAttendanceForEmployee_ValidEmployeeWithOtherEmployeeAttendance_ReturnsNotFoundForAttendance()
        {
            // Arrange
            var targetEmployeeId = 1;
            var otherEmployeeId = 2;

            var employees = new List<Employee>
            {
                new Employee
                {
                    EmployeeId = targetEmployeeId,
                    UserId = 101,
                    FullName = "John Doe",
                    PhoneNumber = "1234567890",
                    NationalId = "12345678901234",
                    Address = "Cairo",
                    Gender = "Male",
                    ImagePath = "C:\\john.jpg"
                },
                new Employee
                {
                    EmployeeId = otherEmployeeId,
                    UserId = 102,
                    FullName = "Jane Smith",
                    PhoneNumber = "0987654321",
                    NationalId = "43210987654321",
                    Address = "Alexandria",
                    Gender = "Female",
                    ImagePath = "C:\\jane.jpg"
                }
            };

            // Add attendance for other employee only
            var attendanceList = new List<Attendance>
            {
                new Attendance
                {
                    AttendanceId = 1,
                    EmployeeId = otherEmployeeId,
                    AttendanceDate = DateTime.Now,
                    Employee = employees[1]
                }
            };

            _context.Employees.AddRange(employees);
            _context.Attendances.AddRange(attendanceList);
            _context.SaveChanges();

            _mockMapper.Setup(m => m.Map<List<GetAttendaceDTO>>(It.IsAny<List<Attendance>>()))
                .Returns(new List<GetAttendaceDTO>()); // Empty list for target employee

            // Act
            var result = _controller.GetAttendanceForEmployee(targetEmployeeId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult.Value);

            // Check the message
            var message = notFoundResult.Value.GetType().GetProperty("message")?.GetValue(notFoundResult.Value);
            Assert.AreEqual("Employee doesn't have any attendance", message);
        }

      
    }

}

