using RentZ.DTO.Feedback;
using RentZ.DTO.Response;

namespace RentZ.Application.Services.Feedback;

public interface IFeedbackServices
{
    Task<BaseResponse<bool>> AddFeedback(AddingFeedback request, string uId);
}