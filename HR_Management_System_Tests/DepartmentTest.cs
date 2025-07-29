using AutoMapper;
using HrManagementSystem.Controllers;
using HrManagementSystem.DTOs.DepartmentsDTOs;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HR_Management_System_Tests;

[TestClass]
public class DepartmentTest
{
    [TestClass]
    public sealed class DepartmentControllerTests
    {
        private HRContext _context;
        private UnitOfWork _unitOfWork;
        private Mock<IMapper> _mockMapper;
        private DepartmentController _controller;

        [TestInitialize]
        public void DepartmentInit()
        {
            var options = new DbContextOptionsBuilder<HRContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new HRContext(options);
            _unitOfWork = new UnitOfWork(_context);
            _mockMapper = new Mock<IMapper>();
            _controller = new DepartmentController(_unitOfWork, _mockMapper.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }


        [TestMethod]
        public void GetAllDepartments_WithDepartments_ReturnsOkWithDepartmentList()
        {
            // Arrange
            var departments = new List<Department>
            {
                new Department
                {
                    DepartmentId = 29,
                    DepartmentName = "IT",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Description="IT DEPT"
                },
                new Department
                {
                    DepartmentId = 30,
                    DepartmentName = "HR",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Description="HR DEPT"
                }
            };

            var departmentDTOs = new List<GetDepartmentsDTO>
            {
                new GetDepartmentsDTO { DepartmentId = 1, DepartmentName = "IT" },
                new GetDepartmentsDTO { DepartmentId = 2, DepartmentName = "HR" }
            };

            _context.Departments.AddRange(departments);
            _context.SaveChanges();

            _mockMapper.Setup(m => m.Map<List<GetDepartmentsDTO>>(It.IsAny<List<Department>>()))
                .Returns(departmentDTOs);

            // Act
            var result = _controller.GetAllDepartments();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsInstanceOfType(okResult.Value, typeof(List<GetDepartmentsDTO>));
            var returnedDepartments = okResult.Value as List<GetDepartmentsDTO>;
            Assert.AreEqual(2, returnedDepartments.Count);
        }

        [TestMethod]

        public void GetAllDepartments_NoDepartments_ReturnsOkWithEmptyList()
        {
            // Arrange
            _mockMapper.Setup(m => m.Map<List<GetDepartmentsDTO>>(It.IsAny<List<Department>>()))
                .Returns(new List<GetDepartmentsDTO>());

            // Act
            var result = _controller.GetAllDepartments();

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var returnedDepartments = okResult.Value as List<GetDepartmentsDTO>;
            Assert.AreEqual(0, returnedDepartments.Count);
        }



        [TestMethod]
        public void AddNewDepartment_ValidDepartment_ReturnsOkWithDepartment()
        {
            // Arrange
            var newDeptDTO = new AddNewDepartmentDTO
            {
                DepartmentName = "Finance",
                Description = "Finance DEPT"
            };

            var newDepartment = new Department
            {
                DepartmentId = 1,
                DepartmentName = "Finance",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Description = "Finance DEPT"
            };

            _mockMapper.Setup(m => m.Map<Department>(newDeptDTO))
                .Returns(new Department
                {
                    DepartmentId = 1,
                    DepartmentName = "Finance",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Description = "Finance DEPT"
                });

            // Act
            var result = _controller.AddNewDepartment(newDeptDTO);
            Console.WriteLine(result);
            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var returnedDepartment = okResult.Value as Department;
            Assert.IsNotNull(returnedDepartment);
            Assert.AreEqual("Finance", returnedDepartment.DepartmentName);
            Assert.IsTrue(returnedDepartment.CreatedAt != default(DateTime));
            Assert.IsTrue(returnedDepartment.UpdatedAt != default(DateTime));
        }

        [TestMethod]
        public void AddNewDepartment_NotFullDataReturns_ReturnsBadRequest()
        {
            // Arrange
            var newDeptDTO = new AddNewDepartmentDTO
            {
                DepartmentName = "Finance",
                Description = "Finance DEPT",
            };

            _mockMapper.Setup(m => m.Map<Department>(newDeptDTO))
                .Returns(new Department
                {
                    DepartmentId = 1,
                    DepartmentName = "Finance"
                });

            // Act
            var result = _controller.AddNewDepartment(newDeptDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
            Assert.AreEqual("Try again", message);
        }
        [TestMethod]
        public void AddNewDepartment_DuplicateDepartmentName_ReturnsBadRequest()
        {
            // Arrange
            var existingDepartment = new Department
            {
                DepartmentId = 1,
                DepartmentName = "IT",
                Description = "IT DEPT",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Departments.Add(existingDepartment);
            _context.SaveChanges();

            var newDeptDTO = new AddNewDepartmentDTO
            {
                DepartmentName = "IT" // Duplicate name
            };

            _mockMapper.Setup(m => m.Map<Department>(newDeptDTO))
                .Returns(new Department { DepartmentName = "IT" });

            // Act
            var result = _controller.AddNewDepartment(newDeptDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }



        [TestMethod]
        public void UpdateDepartment_ValidDepartment_ReturnsOkWithUpdatedDepartment()
        {
            // Arrange
            var existingDepartment = new Department
            {
                DepartmentId = 1,
                DepartmentName = "IT",
                CreatedAt = DateTime.Now.AddDays(-1),
                UpdatedAt = DateTime.Now.AddDays(-1),
                Description = "IT DEPT"
            };

            _context.Departments.Add(existingDepartment);
            _context.SaveChanges();

            var updateDTO = new UpdateExistingDepartmentDTO
            {
                DepartmentId = 1,
                DepartmentName = "Information Technology"
            };

            _mockMapper.Setup(m => m.Map(updateDTO, It.IsAny<Department>()))
                .Callback<UpdateExistingDepartmentDTO, Department>((src, dest) =>
                {
                    dest.DepartmentName = src.DepartmentName;
                });

            // Act
            var result = _controller.UpdateDepartment(updateDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var updatedDepartment = okResult.Value as Department;
            Assert.IsNotNull(updatedDepartment);
            Assert.AreEqual("Information Technology", updatedDepartment.DepartmentName);
            Assert.IsTrue(updatedDepartment.UpdatedAt > updatedDepartment.CreatedAt);
        }
        [TestMethod]
        public void UpdateDepartment_DepartmentNotFound_ReturnsBadRequest()
        {
            // Arrange
            var updateDTO = new UpdateExistingDepartmentDTO
            {
                DepartmentId = 999, // Non-existent ID
                DepartmentName = "Information Technology",
                Description = "IT DEPT"
            };

            // Act
            var result = _controller.UpdateDepartment(updateDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
            Assert.AreEqual("No such department", message);
        }

        [TestMethod]
        public void DeleteDepartment_ValidIdNoEmployees_ReturnsOk()
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

            _context.Departments.Add(department);
            _context.SaveChanges();

            // Act
            var result = _controller.DeleteDepartment(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));

            // Verify department was deleted
            var deletedDept = _context.Departments.Find(1);
            Assert.IsNull(deletedDept);
        }
        [TestMethod]
        public void DeleteDepartment_DepartmentNotFound_ReturnsBadRequest()
        {
            // Arrange & Act
            var result = _controller.DeleteDepartment(999);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
            Assert.AreEqual("No such department", message);
        }
        [TestMethod]
        public void DeleteDepartment_DepartmentHasEmployees_ReturnsBadRequest()
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

            // Act
            var result = _controller.DeleteDepartment(1);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            var message = badRequestResult.Value.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
            Assert.AreEqual("Department has employees", message);
        }
        [TestMethod]
        public void DeleteDepartment_ExceptionThrown_ReturnsBadRequest()
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

            _context.Departments.Add(department);
            _context.SaveChanges();

            // Force an exception by disposing the context before the delete operation
            var originalContext = _unitOfWork.GetType().GetField("_context",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // This test might need adjustment based on your actual UnitOfWork implementation
            // For now, we'll test the general exception case conceptually

            // Act & Assert would depend on your specific implementation
            // This is a placeholder for exception testing
        }
        [TestMethod]
        public void DeleteDepartment_ValidIdWithMultipleEmployeesInOtherDepts_ReturnsOk()
        {
            // Arrange
            var dept1 = new Department
            {
                DepartmentId = 1,
                DepartmentName = "IT",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Description = "Finance DEPT"
            };

            var dept2 = new Department
            {
                DepartmentId = 2,
                DepartmentName = "HR",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Description = "HR DEPT"
            };

            var employee = new Employee
            {
                EmployeeId = 1,
                DepartmentId = 2, // Employee in different department
                UserId = 101,
                FullName = "John Doe",
                PhoneNumber = "1234567890",
                NationalId = "12345678901234",
                Address = "Cairo",
                Gender = "Male",
                ImagePath = "C:\\john.jpg"
            };

            _context.Departments.AddRange(dept1, dept2);
            _context.Employees.Add(employee);
            _context.SaveChanges();

            // Act
            var result = _controller.DeleteDepartment(1); // Delete IT dept (no employees)

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkResult));

            // Verify correct department was deleted
            var deletedDept = _context.Departments.Find(1);
            Assert.IsNull(deletedDept);

            // Verify other department still exists
            var remainingDept = _context.Departments.Find(2);
            Assert.IsNotNull(remainingDept);
        }

        [TestMethod]
        public void UpdateDepartment_SameName_ReturnsOk()
        {
            // Arrange
            var existingDepartment = new Department
            {
                DepartmentId = 1,
                DepartmentName = "IT",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Description = "IT DEPT"
            };

            _context.Departments.Add(existingDepartment);
            _context.SaveChanges();

            var updateDTO = new UpdateExistingDepartmentDTO
            {
                DepartmentId = 1,
                DepartmentName = "IT" // Same name
            };

            _mockMapper.Setup(m => m.Map(updateDTO, It.IsAny<Department>()))
                .Callback<UpdateExistingDepartmentDTO, Department>((src, dest) =>
                {
                    dest.DepartmentName = src.DepartmentName;
                });

            // Act
            var result = _controller.UpdateDepartment(updateDTO);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

    }
}
