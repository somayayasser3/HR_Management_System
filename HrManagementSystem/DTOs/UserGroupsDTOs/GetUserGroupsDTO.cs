using System.ComponentModel.DataAnnotations;

namespace HrManagementSystem.DTOs.UserGroupsDTOs
{
    public class GetUserGroupsDTO
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
    }
}
