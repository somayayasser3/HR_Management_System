using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HrManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FaceRecognitionController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FaceRecognitionController> _logger;
        private readonly string _flaskApiUrl;

        public FaceRecognitionController(HttpClient httpClient, ILogger<FaceRecognitionController> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _flaskApiUrl = configuration["FlaskApi:BaseUrl"] ?? "http://localhost:5000";
        }

        [HttpPost("compare-faces")]
        public async Task<IActionResult> CompareFaces([FromForm] CompareFacesDto dto)
        {
            try
            {
                // Validate input files
                if (dto.Image1 == null || dto.Image2== null)
                {
                    return BadRequest(new { success = false, error = "Both image1 and image2 are required" });
                }

                if (dto.Image1.Length == 0 || dto.Image2.Length == 0)
                {
                    return BadRequest(new { success = false, error = "Both images must contain data" });
                }

                // Prepare multipart form content
                using var formContent = new MultipartFormDataContent();

                // Add first image
                var image1Content = new StreamContent(dto.Image1.OpenReadStream());
                image1Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.Image1.ContentType ?? "image/jpeg");
                formContent.Add(image1Content, "image1", dto.Image1.FileName ?? "image1.jpg");

                // Add second image
                var image2Content = new StreamContent(dto.Image2.OpenReadStream());
                image2Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(dto.Image2.ContentType ?? "image/jpeg");
                formContent.Add(image2Content, "image2", dto.Image2.FileName ?? "image2.jpg");

                // Send request to Flask API
                var response = await _httpClient.PostAsync($"{_flaskApiUrl}/compare-faces", formContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<FaceComparisonResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return Ok(result);
                }
                else
                {
                    _logger.LogError("Flask API returned error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(responseContent));
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error calling Flask API");
                return StatusCode(503, new { success = false, error = "Face recognition service unavailable" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing face comparison request");
                return StatusCode(500, new { success = false, error = "Internal server error" });
            }
        }
        

        [HttpPost("compare-faces-from-urls")]
        public async Task<IActionResult> CompareFacesFromUrls([FromBody] ImageUrlRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Image1Url) || string.IsNullOrEmpty(request.Image2Url))
                {
                    return BadRequest(new { success = false, error = "Both image URLs are required" });
                }

                // Download images from URLs
                var image1Task = _httpClient.GetAsync(request.Image1Url);
                var image2Task = _httpClient.GetAsync(request.Image2Url);

                await Task.WhenAll(image1Task, image2Task);

                var image1Response = await image1Task;
                var image2Response = await image2Task;

                if (!image1Response.IsSuccessStatusCode || !image2Response.IsSuccessStatusCode)
                {
                    return BadRequest(new { success = false, error = "Failed to download one or both images" });
                }

                // Prepare multipart form content
                using var formContent = new MultipartFormDataContent();

                var image1Content = new StreamContent(await image1Response.Content.ReadAsStreamAsync());
                image1Content.Headers.ContentType = image1Response.Content.Headers.ContentType;
                formContent.Add(image1Content, "image1", "image1.jpg");

                var image2Content = new StreamContent(await image2Response.Content.ReadAsStreamAsync());
                image2Content.Headers.ContentType = image2Response.Content.Headers.ContentType;
                formContent.Add(image2Content, "image2", "image2.jpg");

                // Send request to Flask API
                var response = await _httpClient.PostAsync($"{_flaskApiUrl}/compare-faces", formContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<FaceComparisonResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return Ok(result);
                }
                else
                {
                    return StatusCode((int)response.StatusCode, JsonSerializer.Deserialize<object>(responseContent));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing face comparison from URLs");
                return StatusCode(500, new { success = false, error = "Internal server error" });
            }
        }

        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_flaskApiUrl}/health");
                var content = await response.Content.ReadAsStringAsync();

                return Ok(new
                {
                    aspNetStatus = "healthy",
                    flaskApiStatus = response.IsSuccessStatusCode ? "healthy" : "unhealthy",
                    flaskResponse = JsonSerializer.Deserialize<object>(content)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed");
                return Ok(new
                {
                    aspNetStatus = "healthy",
                    flaskApiStatus = "unreachable",
                    error = ex.Message
                });
            }
        }
    }

    // Data models for requests and responses
    public class FaceComparisonResponse
    {
        public bool Success { get; set; }
        public bool IsSamePerson { get; set; }
        public double Confidence { get; set; }
        public double Distance { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Error { get; set; }
    }

    public class ImageUrlRequest
    {
        public string Image1Url { get; set; } = string.Empty;
        public string Image2Url { get; set; } = string.Empty;
    }

    public class CompareFacesDto
    {
        //[Required]
        [JsonPropertyName("image1")]
        public IFormFile Image1 { get; set; }
        //[Required]
        [JsonPropertyName("image2")]
        public IFormFile Image2 { get; set; }
    }

}