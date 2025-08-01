using AutoMapper;
using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HrManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        UnitOfWork unit;
        IMapper map;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _http;
        private readonly string apiKey;

        public ChatController(UnitOfWork unit, IMapper mapper, UserManager<User> user, IHttpContextAccessor http, IOptions<OpenAISettings> options)
        {
            this.unit = unit;
            this.map = mapper;
            this._userManager = user;
            this._http = http;
            apiKey = options.Value.ApiKey;
        }

        [HttpGet("welcome")]
        [EndpointSummary("Get personalized welcome message based on user role")]
        public async Task<IActionResult> GetWelcomeMessage()
        {
            var user = await _userManager.GetUserAsync(_http.HttpContext.User);
            if (user == null)
                return Unauthorized(new { message = "User not found" });

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Employee";

            var welcomeMessage = GenerateWelcomeMessage(user.FullName, primaryRole);

            var botMessage = new ChatMessage
            {
                Sender = "bot",
                Message = welcomeMessage,
                Timestamp = DateTime.Now
            };

            return Ok(botMessage);
        }

        [HttpPost("message")]
        [EndpointSummary("Send message to HR chatbot")]
        public async Task<IActionResult> HandleUserQueryAsync([FromBody] ChatMessageRequest request)
        {
            var user = await _userManager.GetUserAsync(_http.HttpContext.User);
            if (user == null)
                return Unauthorized(new {  message = "User not found" });

            var roles = await _userManager.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Employee";

            var isAdmin = roles.Contains("Admin");
            var isHr = roles.Contains("HR");
            var isEmployee = roles.Contains("Employee");

            // Debug logging - you can remove this later
           // Console.WriteLine($"User: {user.FullName}, Roles: {string.Join(", ", roles)}, IsAdmin: {isAdmin}, IsHR: {isHr}");

            var employee = unit.EmployeeRepo.GetEmployeeWithUserID(user.Id);

            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest( new { message = "Message cannot be empty." });

            // Store user message
            await unit.ChatRepo.AddAsync(new ChatMessage
            {
                Message = request.Message,
                Sender = "user",
                Timestamp = DateTime.Now
            });
            unit.Save();

            // Check if user has access to requested information
            var accessCheckResult = await CheckRoleBasedAccess(request.Message, primaryRole, isAdmin, isHr, isEmployee);

            if (!accessCheckResult.HasAccess)
            {
                var deniedMessage = new ChatMessage
                {
                    Sender = "bot",
                    Message = accessCheckResult.DeniedMessage,
                    Timestamp = DateTime.Now
                };

                await unit.ChatRepo.AddAsync(deniedMessage);
                unit.Save();
                return Ok(deniedMessage);
            }

            // Analyze message intent
            var analysisJson = await AnalyzeMessageIntent(request.Message, isHr, isAdmin, primaryRole);
            var analysis = JsonSerializer.Deserialize<AnalysisResult>(analysisJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (analysis == null || string.IsNullOrEmpty(analysis.Intent))
            {
                var generalReply = await GetFinalReplyFromOpenAI(
                    $"User ({primaryRole}) asked: \"{request.Message}\"\n\n" +
                    $"Reply as an expert HR assistant. User role: {primaryRole}. " +
                    "Provide helpful HR-related information in English only."
                );

                var generalMessage = new ChatMessage
                {
                    Sender = "bot",
                    Message = generalReply,
                    Timestamp = DateTime.Now
                };

                await unit.ChatRepo.AddAsync(generalMessage);
                unit.Save();
                return Ok(generalMessage);
            }

            // Fetch data based on role and analysis
            var dataPayload = await FetchDataAsNeeded(analysis, employee?.EmployeeId ?? 0, isHr, isAdmin, primaryRole);

            var responsePrompt = BuildResponsePrompt(request.Message, dataPayload, primaryRole, user.FullName);
            var finalAnswer = await GetFinalReplyFromOpenAI(responsePrompt);

            var botReply = new ChatMessage
            {
                Sender = "bot",
                Message = finalAnswer,
                Timestamp = DateTime.Now
            };

            await unit.ChatRepo.AddAsync(botReply);
            unit.Save();

            return Ok(botReply);
        }

        [HttpGet("chat-history")]
        [EndpointSummary("Get chat history for current user")]
        public async Task<IActionResult> GetChatHistory()
        {
            var chatHistory = await unit.ChatRepo.GetChatHistoryAsync();
            return Ok(chatHistory.OrderBy(c => c.Timestamp));
        }

        [HttpDelete("clear-history")]
        [EndpointSummary("Clear chat history")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ClearChatHistory()
        {
            await unit.ChatRepo.ClearChatHistoryAsync();
            unit.Save();
            return Ok(new { message = "Chat history cleared successfully" });
        }

        private string GenerateWelcomeMessage(string userName, string role)
        {
            return role switch
            {
                "Admin" => $"👋 Welcome back, {userName}!\n\n" +
                          "As an Administrator, you have full access to the HR Management System. I can help you with:\n" +
                          "• 📊 All employee data and reports\n" +
                          "• 🏢 Department management\n" +
                          "• 📅 Attendance tracking for all employees\n" +
                          "• 💰 Salary reports and payroll\n" +
                          "• 🌴 Leave management and approvals\n" +
                          "• ⚙️ System settings and configurations\n" +
                          "• 🎉 Official holidays management\n" +
                          "• 👥 HR staff registration\n\n" +
                          "What would you like to know about today?",

                "HR" => $"👋 Hello {userName}!\n\n" +
                        "As an HR professional, I'm here to assist you with:\n" +
                        "• 👥 Employee management and profiles\n" +
                        "• 📊 Attendance reports and tracking\n" +
                        "• 🌴 Leave requests and approvals\n" +
                        "• 💰 Salary reports and calculations\n" +
                        "• 🏢 Department information\n" +
                        "• 🎉 Official holidays management\n" +
                        "• ⚙️ System settings\n\n" +
                        "How can I help you manage your HR tasks today?",

                "Employee" => $"👋 Hi {userName}!\n\n" +
                           "Welcome to your personal HR assistant! I can help you with:\n" +
                           "• 👤 Your profile and personal information\n" +
                           "• 📅 Your attendance records\n" +
                           "• 🌴 Your leave balance and requests\n" +
                           "• 💰 Your salary information\n" +
                           "• 🏢 Department details\n" +
                           "• 🎉 Official holidays\n\n" +
                           "What would you like to check today?",

                _ => $"👋 Welcome {userName}! How can I assist you with HR-related questions today?"
            };
        }

        // role-based access control
        private async Task<RoleAccessResult> CheckRoleBasedAccess(string message, string role, bool isAdmin, bool isHr, bool isEmployee)
        {
            var lowerMessage = message.ToLower();

            // Admin has access to everything - ALWAYS ALLOW
            if (isAdmin)
            {
                //Console.WriteLine("Admin access granted for all requests");
                return new RoleAccessResult { HasAccess = true };
            }

            // HR restrictions - only block very specific admin-only functions
            if (isHr)
            {
                var hrRestrictedKeywords = new[] {
                    "register hr", "add hr", "create hr", "delete hr",
                    "system configuration", "admin settings", "user management"
                };

                if (hrRestrictedKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                {
                    return new RoleAccessResult
                    {
                        HasAccess = false,
                        DeniedMessage = "🚫 Access denied. Only Administrators can perform user management operations."
                    };
                }

                //Console.WriteLine("HR access granted");
                return new RoleAccessResult { HasAccess = true };
            }

            // Employee restrictions - more restrictive
            if (isEmployee)
            {
                var employeeRestrictedKeywords = new[] {
                    "all employees", "employee list", "all staff", "everyone",
                    "all attendance", "everyone attendance", "attendance report",
                    "all salaries", "salary report", "payroll", "all leave requests",
                    "register", "add employee", "delete", "system setting"
                };

                if (employeeRestrictedKeywords.Any(keyword => lowerMessage.Contains(keyword)))
                {
                    return new RoleAccessResult
                    {
                        HasAccess = false,
                        DeniedMessage = "🚫 Access denied. You can only access your personal information. Try asking about 'my profile', 'my attendance', 'my salary', or 'my leave balance'."
                    };
                }

                //Console.WriteLine("Employee access granted for personal data");
                return new RoleAccessResult { HasAccess = true };
            }

            return new RoleAccessResult { HasAccess = true };
        }

        private async Task<string> AnalyzeMessageIntent(string userMessage, bool isHr, bool isAdmin, string role)
        {
            var roleCapabilities = GetRoleCapabilities(role, isAdmin, isHr);

            var prompt = $"You are an HR assistant. Analyze the following user message and return the result as raw JSON in this format:\n" +
                         "{{ \"intent\": string, \"required_data\": [string], \"any_transformation\": string|null }}\n" +
                         $"User Role: {role}\n" +
                         $"User Capabilities: {roleCapabilities}\n" +
                         $"Message: \"{userMessage}\"\n" +
                         "Available data types: employee_info, salary, department, leave_balance, attendances, leave_requests, salary_reports, holidays\n" +
                         "Only suggest data that the user role has access to.";
            var x = await CallOpenAI(prompt);
            return x;
        }

        private string GetRoleCapabilities(string role, bool isAdmin, bool isHr)
        {
            return role switch
            {
                "Admin" => "Full access to all employee data, salary reports, system settings, attendance records, leave management, departments",
                "HR" => "Access to employee management, attendance reports, leave approvals, salary reports, departments, holidays",
                "Employee" => "Access to personal profile, own attendance, own leave balance, own salary, department info, holidays",
                _ => "Limited access"
            };
        }

        // ENHANCED: Better data fetching with more debug info
        private async Task<string> FetchDataAsNeeded(AnalysisResult analysis, int employeeId, bool isHr, bool isAdmin, string role)
        {
            var data = new Dictionary<string, object>();

            if (analysis?.RequiredData == null)
            {
                Console.WriteLine("No required data in analysis");
                return "";
            }

            Console.WriteLine($"Required data: {string.Join(", ", analysis.RequiredData)}");

            foreach (var field in analysis.RequiredData)
            {
                Console.WriteLine($"Processing field: {field} for role: {role}");

                switch (field.ToLower())
                {
                    case "employee_info":
                        if (isAdmin || isHr)
                        {
                            // Check if the user is asking about their personal data
                            bool isPersonalRequest = analysis.Intent?.ToLower().Contains("my") == true||
                                analysis.Intent?.ToLower().Contains("personal") == true ||
                                analysis.Intent?.ToLower().Contains("profile") == true;

                            if (isPersonalRequest && employeeId > 0)
                            {
                                // Return personal data for HR/Admin
                                var emp = unit.EmployeeRepo.GetEmployeeWithDeptBYID(employeeId);
                                if (emp != null)
                                {
                                    data["my_profile"] = new
                                    {
                                        FullName = emp.FullName ?? "N/A",
                                        Email = emp.User?.Email ?? "No Email",
                                        Department = emp.Department?.DepartmentName ?? "N/A",
                                        PhoneNumber = emp.PhoneNumber ?? "N/A",
                                        Address = emp.Address ?? "N/A",
                                        HireDate = emp.HireDate,
                                        Salary = emp.Salary
                                    };
                                    Console.WriteLine($"Retrieved personal profile for HR/Admin employee ID: {employeeId}");
                                }
                            }
                            else
                            {
                                // Return all employees data for HR/Admin
                                try
                                {
                                    var employees = unit.EmployeeRepo.getAll()
                                        ?.Where(e => e != null)
                                        .Select(e => new
                                        {
                                            FullName = e.FullName ?? "N/A",
                                            Email = e.User?.Email ?? "No Email",
                                            Salary = e.Salary,
                                            HireDate = e.HireDate,
                                            Department = e.Department?.DepartmentName ?? "N/A",
                                            PhoneNumber = e.PhoneNumber ?? "N/A"
                                        })?.ToList();

                                    data["all_employees"] = employees;
                                    Console.WriteLine($"Retrieved {employees?.Count ?? 0} employees for HR/Admin");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error fetching employees: {ex.Message}");
                                    data["all_employees"] = new { error = "Could not retrieve employee data" };
                                }
                            }
                        }
                        else if (employeeId > 0)
                        {
                            var emp = unit.EmployeeRepo.GetEmployeeWithDeptBYID(employeeId);
                            if (emp != null)
                            {
                                data["my_profile"] = new
                                {
                                    emp.FullName,
                                    emp.User.Email,
                                    Department = emp.Department?.DepartmentName ?? "N/A",
                                    emp.PhoneNumber,
                                    emp.Address
                                };
                            }
                        }
                        break;

                    case "salary":
                        if (isAdmin || isHr)
                        {
                            try
                            {
                                var allSalaries = unit.EmployeeRepo.getAll()
                                    .Select(e => new { e.FullName, e.Salary }).ToList();
                                data["all_salaries"] = allSalaries;
                                Console.WriteLine($"Retrieved {allSalaries.Count} salary records");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error fetching salaries: {ex.Message}");
                                data["all_salaries"] = new { error = "Could not retrieve salary data" };
                            }
                        }
                        else if (employeeId > 0)
                        {
                            var salary = unit.EmployeeRepo.getByID(employeeId)?.Salary ?? 0;
                            data["my_salary_egp"] = salary;
                        }
                        break;

                    case "department":
                        if (isAdmin || isHr)
                        {
                            try
                            {
                                var allDepts = unit.DepartmentRepo.getAll()
                                    .Select(d => new { d.DepartmentName, d.Description }).ToList();
                                data["departments"] = allDepts;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error fetching departments: {ex.Message}");
                                data["departments"] = new { error = "Could not retrieve department data" };
                            }
                        }
                        else if (employeeId > 0)
                        {
                            var emp = unit.EmployeeRepo.GetEmployeeWithDeptBYID(employeeId);
                            data["my_department"] = emp?.Department?.DepartmentName ?? "N/A";
                        }
                        break;

                    //case "leave_balance":
                    //    if (employeeId > 0)
                    //    {
                    //        var emp = unit.EmployeeRepo.GetEmployeeWithDeptBYID(employeeId);
                    //        data["leave_balance"] = new
                    //        {
                    //            Annual = emp?.LeaveBalance?.AnnualLeaveBalance ?? 0,
                    //            Sick = emp?.LeaveBalance?.SickLeaveBalance ?? 0,
                    //            Unpaid = emp?.LeaveBalance?.UnpaidLeaveBalance ?? 0
                    //        };
                    //    }
                    //    break;

                    case "attendances":
                        if (isAdmin || isHr)
                        {
                            try
                            {
                                var allAttendances = unit.AttendanceRepo.GetAttendanceWithEmployees()
                                    .OrderByDescending(a => a.AttendanceDate)
                                    .Select(a => new
                                    {
                                        a.Employee.FullName,
                                        a.AttendanceDate,
                                        a.CheckInTime,
                                        a.CheckOutTime,
                                        a.OvertimeHours,
                                        a.DelayHours
                                    }).ToList();
                                data["all_attendances"] = allAttendances;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error fetching attendances: {ex.Message}");
                                data["all_attendances"] = new { error = "Could not retrieve attendance data" };
                            }
                        }
                        else if (employeeId > 0)
                        {
                            var attendances = unit.AttendanceRepo.GetAttendanceForEmployee(employeeId)
                                .Select(a => new
                                {
                                    a.AttendanceDate,
                                    a.CheckInTime,
                                    a.CheckOutTime,
                                    a.OvertimeHours,
                                    a.DelayHours
                                }).ToList();
                            data["my_attendances"] = attendances;
                        }
                        break;

                    case "leave_requests":
                        if (isAdmin || isHr)
                        {
                            try
                            {
                                var allLeaves = unit.LeaveRepo.GetAllRequests()
                                    .Select(r => new
                                    {
                                        r.Employee.FullName,
                                        r.LeaveType.Name,
                                        r.Status,
                                        r.StartDate,
                                        r.EndDate
                                    }).ToList();
                                data["all_leave_requests"] = allLeaves;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error fetching leave requests: {ex.Message}");
                                data["all_leave_requests"] = new { error = "Could not retrieve leave data" };
                            }
                        }
                        else if (employeeId > 0)
                        {
                            var leaves = unit.LeaveRepo.GetAllRequests()
                                .Where(r => r.EmployeeId == employeeId)
                                .Select(r => new
                                {
                                    r.LeaveType.Name,
                                    r.Status,
                                    r.StartDate,
                                    r.EndDate
                                }).ToList();
                            data["my_leave_requests"] = leaves;
                        }
                        break;

                    case "salary_reports":
                        if (isAdmin || isHr)
                        {
                            try
                            {
                                var allReports = unit.SalaryReportRepo.GetAllReportsWithEmps()
                                    .Take(50)
                                    .Select(r => new
                                    {
                                        r.Employee.FullName,
                                        r.Month,
                                        r.Year,
                                        r.NetSalary
                                    }).ToList();
                                data["all_salary_reports"] = allReports;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error fetching salary reports: {ex.Message}");
                                data["all_salary_reports"] = new { error = "Could not retrieve salary reports" };
                            }
                        }
                        else if (employeeId > 0)
                        {
                            var reports = unit.SalaryReportRepo.GetAllReportsForEmp(employeeId)
                                .Select(r => new { r.Month, r.Year, r.NetSalary }).ToList();
                            data["my_salary_reports"] = reports;
                        }
                        break;

                    case "holidays":
                        try
                        {
                            var holidays = unit.OfficialHolidayRepo.getAll()
                                .Select(h => new { h.HolidayName, h.HolidayDate, h.Description }).ToList();
                            data["official_holidays"] = holidays;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error fetching holidays: {ex.Message}");
                            data["official_holidays"] = new { error = "Could not retrieve holiday data" };
                        }
                        break;
                }
            }

            var result = string.Join("\n", data.Select(d => $"{d.Key}: {JsonSerializer.Serialize(d.Value)}"));
            Console.WriteLine($"Data payload length: {result.Length}");
            return result;
        }

        private string BuildResponsePrompt(string userMessage, string dataPayload, string role, string userName)
        {
            return $"User ({role} - {userName}) asked: \"{userMessage}\"\n\n" +
                   $"Available data from system:\n{dataPayload}\n\n" +
                   $"Instructions:\n" +
                   $"- Reply as a helpful HR assistant\n" +
                   $"- User role: {role}\n" +
                   $"- Use professional but friendly tone\n" +
                   $"- Include relevant emojis\n" +
                   $"- Format data clearly if presenting lists or numbers\n" +
                   $"- Reply in English only\n" +
                   $"- If no data available, provide helpful guidance\n" +
                   $"- If there are errors in data retrieval, acknowledge them and suggest alternatives";
        }

        private async Task<string> GetFinalReplyFromOpenAI(string prompt)
        {
            return await CallOpenAI(prompt);
        }

        private async Task<string> CallOpenAI(string prompt)
        {

            if (string.IsNullOrEmpty(apiKey))
                return "I apologize, but the AI service is currently unavailable. Please contact your system administrator.";

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = "gpt-4.1",
                //model = "gpt-4o-mini",
                messages = new[] {
                    new { role = "system", content = "You are an expert HR assistant. Always reply in English with a professional but friendly tone." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 1000,
                temperature = 0.7
            };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                var json = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(json);
                return doc.RootElement
                          .GetProperty("choices")[0] .GetProperty("message") .GetProperty("content").GetString() ?.Trim() ?? "I'm sorry, I couldn't process your request at the moment.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OpenAI API Error: {ex.Message}");
                return "I'm sorry, I'm experiencing technical difficulties. Please try again later.";
            }
        }
    }

    public class AnalysisResult
    {
        [JsonPropertyName("intent")]
        public string Intent { get; set; }

        [JsonPropertyName("required_data")]
        public List<string> RequiredData { get; set; }

        [JsonPropertyName("any_transformation")]
        public string Transformation { get; set; }
    }

    public class RoleAccessResult
    {
        public bool HasAccess { get; set; }
        public string DeniedMessage { get; set; }
    }

    public class ChatMessageRequest
    {
        public string Message { get; set; }
    }
}