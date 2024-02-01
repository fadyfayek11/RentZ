using RentZ.Domain.Entities;
using RentZ.DTO.Enums;
using RentZ.DTO.Feedback;
using RentZ.DTO.Response;
using RentZ.Infrastructure.Context;

namespace RentZ.Application.Services.Feedback;

public class FeedbackServices : IFeedbackServices
{
    private readonly ApplicationDbContext _context;

    public FeedbackServices(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BaseResponse<bool>> AddFeedback(AddingFeedback request, string uId)
    {
        if (string.IsNullOrEmpty(request.FeedbackContent) || string.IsNullOrEmpty(uId)) return new BaseResponse<bool>() { Code = ErrorCode.BadRequest, Message = "Fail to create a feedback", Data = false };

        await _context.FeedBack.AddAsync(new FeedBack
        {
            Feedback = request.FeedbackContent,
            OwnerId = Guid.Parse(uId),
        });

        var isRequestSaved = await _context.SaveChangesAsync();
        return new BaseResponse<bool>() { Code = ErrorCode.Success, Message = "We get your feedback successfully", Data = true };
    }
}