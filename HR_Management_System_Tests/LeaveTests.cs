using AutoMapper;
using HrManagementSystem.Controllers;
using HrManagementSystem.DTOs.LeaveDTOs;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HR_Management_System_Tests;

[TestClass]
public class LeaveTests
{
    private HRContext _context;
    private UnitOfWork _unitOfWork;
    private Mock<IMapper> _mapperMock;
    private LeaveController _controller;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<HRContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new HRContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _mapperMock = new Mock<IMapper>();
        _controller = new LeaveController(_unitOfWork, _mapperMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context?.Dispose();
    }

    [TestMethod]
    public void GetAllLeaveTypes_ShouldReturnOkWithTypes()
    {
        _context.LeaveTypes.AddRange(
            new LeaveType { Id = 1, Name = "Annual" },
            new LeaveType { Id = 2, Name = "Sick" }
        );
        _context.SaveChanges();

        var result = _controller.GetAllLeaveTypes();

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var types = okResult.Value as List<LeaveType>;
        Assert.IsNotNull(types);
        Assert.AreEqual(2, types.Count);
    }

    [TestMethod]
    public void GetAllRequests_ShouldReturnMappedRequests()
    {
        var request = new LeaveRequest
        {
            Id = 1,
            Status = "Pending",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Reason = "Vacation"
        };

        _context.LeaveRequests.Add(request);
        _context.SaveChanges();

        var dtoList = new List<DisplayResultforLeaveRequest>
        {
            new DisplayResultforLeaveRequest { Id = 1, Status = "Pending" }
        };

        _mapperMock.Setup(m => m.Map<List<DisplayResultforLeaveRequest>>(It.IsAny<List<LeaveRequest>>()))
                   .Returns(dtoList);

        var result = _controller.GetAllRequests();

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var list = okResult.Value as List<DisplayResultforLeaveRequest>;
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);
    }

    [TestMethod]
    public void HandleRequest_ApproveAnnualLeave_ShouldUpdateBalanceAndReturnOk()
    {
        var emp = new Employee
        {
            EmployeeId = 1,
            FullName = "John Doe",
            Address = "Cairo",
            PhoneNumber = "0123456789",
            Gender = "Male",
            NationalId = "12345678901234",
            ImagePath = "image.jpg",
        };
        var leaveType = new LeaveType { Id = 1, Name = "Annual" };
        var request = new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            Status = "Pending",
            LeaveType = leaveType,
            Reason = "Annual trip"
        };

        _context.Employees.Add(emp);
        _context.LeaveTypes.Add(leaveType);
        _context.LeaveRequests.Add(request);
        _context.SaveChanges();

        var result = _controller.HandleRequest(1, "Approved");

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual("Approved", _context.LeaveRequests.Find(1)?.Status);
    }

    [TestMethod]
    public void HandleRequest_AlreadyHandled_ReturnsBadRequest()
    {
        var request = new LeaveRequest
        {
            Id = 5,
            EmployeeId = 2,
            Status = "Approved",
            Reason = "Already approved request"
        };

        _context.LeaveRequests.Add(request);
        _context.SaveChanges();

        var result = _controller.HandleRequest(5, "Rejected");

        var badResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badResult);
        var message = badResult.Value.GetType().GetProperty("message")?.GetValue(badResult.Value);
        Assert.AreEqual("Request has already been handled", message);
    }
}
