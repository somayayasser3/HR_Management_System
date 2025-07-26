using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HrManagementSystem.Services
{
    public class FaceComparisonResponse
    {
        public bool Success { get; set; }
        public bool IsSamePerson { get; set; }
        public double Confidence { get; set; }
        public double Distance { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Error { get; set; }
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
    public class FaceRecognitionService : IFaceRecognitionService
    {
        private readonly HttpClient http;
        private readonly string _flaskApiUrl;
        public FaceRecognitionService(HttpClient http, IConfiguration configuration)
        {
            this.http = http;
            _flaskApiUrl = configuration["FaceRecognitionApi:BaseUrl"] ;
        }
        public async Task<FaceComparisonResponse> CompareFacesAsync([FromForm] CompareFacesDto dto)
        {
            try
            {
                // Validate input files
                if (dto.Image1 == null || dto.Image2 == null|| dto.Image1.Length == 0 || dto.Image2.Length == 0)
                {
                    return new FaceComparisonResponse { Success = false, Message = "Invalid images" };
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
                var response = await http.PostAsync($"{_flaskApiUrl}/compare-faces", formContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<FaceComparisonResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return result ?? new FaceComparisonResponse { Success = false, Message = "Deserialization failed" };


                }
                else
                {
                    return new FaceComparisonResponse { Success = false, Message = "Flask API error", Error = responseContent };
                }

            }
            catch (HttpRequestException ex)
            {
                return new FaceComparisonResponse { Success = false, Message = "Service unavailable", Error = ex.Message };
            }
            catch (Exception ex)
            {
                return new FaceComparisonResponse { Success = false, Message = "Unexpected error", Error = ex.Message };
            }
        }

    }
}
