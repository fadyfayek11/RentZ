using RentZ.DTO.Feedback;
using RentZ.DTO.Property;
using RentZ.DTO.Response;
using RentZ.DTO.User.Security;

namespace RentZ.Application.Services.Admin;

public interface IAdminServices
{
    Task<BaseResponse<bool>> PropertyStatus(PropertyChangeStatus status, string adminId);
    Task<BaseResponse<PagedResult<GettingFeedback?>>> FeedBacks(Pagination pagination);
    Task<BaseResponse<PagedResult<AdminUserData>>> GetUsers(RequestUsers usersRequest);
}