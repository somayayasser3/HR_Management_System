using HrManagementSystem.Models;
using HrManagementSystem.UnitOfWorks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace HrManagementSystem.Controllers
{
    public class ChatController : Controller
    {
        UnitOfWork unit;
        IMapper map;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _http;

        public ChatController(UnitOfWork unit, IMapper mapper, UserManager<User> user, IHttpContextAccessor http)
        {
            this.unit = unit;
            this.map = mapper;
            this._userManager = user;
            this._http = http;
        }

        [HttpPost("message")]
        public async Task<IActionResult> HandleUserQueryAsync(string message)
        {
            var user = await _userManager.GetUserAsync(_http.HttpContext.User);
            var isHr = await _userManager.IsInRoleAsync(user, "HR");
            var employee = unit.EmployeeRepo.GetEmployeeWithUserID(user.Id);

            if (string.IsNullOrWhiteSpace(message))
                return BadRequest("Message cannot be empty.");

            await unit.ChatRepo.AddAsync(new ChatMessage { Message = message, Sender = "user", Timestamp = DateTime.Now });
            unit.Save();

            var analysisJson = await AnalyzeMessageIntent(message, isHr);
            var analysis = JsonSerializer.Deserialize<AnalysisResult>(analysisJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (analysis == null || string.IsNullOrEmpty(analysis.Intent))
            {
                // fallback: treat as general HR topic
                var generalReply = await GetFinalReplyFromOpenAI($"User asked: \"{message}\"\n\nReply as an expert HR assistant in English only.");
                return Ok(new ChatMessage { Sender = "bot", Message = generalReply, Timestamp = DateTime.Now });
            }

            var dataPayload = await FetchDataAsNeeded(analysis, employee.EmployeeId, isHr);

            var responsePrompt = $"User asked: \"{message}\"\n\nData from system:\n{dataPayload}\n\nReply clearly in English as an HR assistant.";
            var finalAnswer = await GetFinalReplyFromOpenAI(responsePrompt);

            var botReply = new ChatMessage { Sender = "bot", Message = finalAnswer, Timestamp = DateTime.Now };
            await unit.ChatRepo.AddAsync(botReply);
            unit.Save();

            return Ok(botReply);
        }

        private async Task<string> AnalyzeMessageIntent(string userMessage, bool isHr)
        {
            var prompt = $"You are an HR assistant. Analyze the following user message and return the result as raw JSON in this format:\n" +
                         "{ \"intent\": string, \"required_data\": [string], \"any_transformation\": string|null }\n" +
                         $"User Type: {(isHr ? "HR" : "Employee")}\n" +
                         $"Message: \"{userMessage}\"";

            return await CallOpenAI(prompt);
        }

        private async Task<string> FetchDataAsNeeded(AnalysisResult analysis, int employeeId, bool isHr)
        {
            var data = new Dictionary<string, object>();

            if (analysis?.RequiredData == null) return "";

            if (isHr && analysis.RequiredData.Contains("employee_info"))
            {
                var employees = unit.EmployeeRepo.getAll()
                    .Where(e => _userManager.GetRolesAsync(e.User).Result.Contains("Employee"))
                    .Select(e => new
                    {
                        e.FullName,
                        e.User.Email,
                        e.Salary,
                        Department = e.Department?.DepartmentName
                    }).ToList();

                data["all_employees"] = employees;
            }
            else
            {
                var emp = unit.EmployeeRepo.GetEmployeeWithDeptBYID(employeeId);

                foreach (var field in analysis.RequiredData)
                {
                    switch (field.ToLower())
                    {
                        case "salary":
                            var salary = unit.EmployeeRepo.getByID(employeeId)?.Salary ?? 0;
                            data["salary_egp"] = salary;
                            break;
                        case "department":
                            data["department"] = emp.Department?.DepartmentName;
                            break;
                        case "leave_balance":
                            data["annual"] = emp.LeaveBalance?.AnnualLeaveBalance;
                            data["sick"] = emp.LeaveBalance?.SickLeaveBalance;
                            data["unpaid"] = emp.LeaveBalance?.UnpaidLeaveBalance;
                            break;
                        case "attendances":
                            var attendances = unit.AttendanceRepo.GetAttendanceForEmployee(employeeId);
                            data["attendances"] = attendances.Select(a => new { a.AttendanceDate, a.CheckInTime, a.CheckOutTime });
                            break;
                        case "leave_requests":
                            var leaves = unit.LeaveRepo.GetAllRequests()
                                .Where(r => r.EmployeeId == employeeId)
                                .Select(r => new { r.LeaveType.Name, r.Status, r.StartDate, r.EndDate });
                            data["leave_requests"] = leaves;
                            break;
                        case "salary_reports":
                            var reports = unit.SalaryReportRepo.GetAllReportsForEmp(employeeId);
                            data["salary_reports"] = reports.Select(r => new { r.Month, r.Year, r.NetSalary });
                            break;
                    }
                }
            }

            return string.Join("\n", data.Select(d => $"{d.Key}: {JsonSerializer.Serialize(d.Value)}"));
        }

        private async Task<string> GetFinalReplyFromOpenAI(string prompt)
        {
            return await CallOpenAI(prompt);
        }

        private async Task<string> CallOpenAI(string prompt)
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[] {
                    new { role = "system", content = "You are an expert HR assistant. Always reply in English." },
                    new { role = "user", content = prompt }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString()
                      .Trim();
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
}
