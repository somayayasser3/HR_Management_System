using HrManagementSystem.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace HrManagementSystem.Services
{
    public interface IFaceRecognitionService
    {
        Task<FaceComparisonResponse> CompareFacesAsync([FromForm] CompareFacesDto dto);
    }


}
