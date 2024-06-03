using Microsoft.EntityFrameworkCore;
using RentZ.Application.Services.Notification;
using RentZ.DTO.Enums;
using RentZ.DTO.Feedback;
using RentZ.DTO.Notification;
using RentZ.DTO.Property;
using RentZ.DTO.Response;
using RentZ.DTO.User.Security;
using RentZ.Infrastructure.Context;

namespace RentZ.Application.Services.Admin;

public class AdminServices : IAdminServices
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public AdminServices(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<BaseResponse<bool>> PropertyStatus(PropertyChangeStatus request, string adminId)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(x=>x.Id == request.PropId);
        if (property == null) return new BaseResponse<bool>() { Code = ErrorCode.BadRequest, Message = "Fail to find the property", Data = false };

        property.ApprovedBy = Guid.Parse(adminId);
        property.Status = request.Status;
        
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();

        if ((NotificationTypes)request.Status != 0)
        {
            await _notificationService.AddNotification(new AddNotification
            {
                Type = (NotificationTypes)request.Status,
                Title = request.Status.ToString(),
                Content = null,
                LinkId = request.PropId,
                ReceiverId = property.OwnerId.ToString(),
                SenderId = adminId
            });
        }
        

        return new BaseResponse<bool>() { Code = ErrorCode.Success, Message = "Property status changes successfully", Data = true };
    }

    public async Task<BaseResponse<PagedResult<GettingFeedback?>>> FeedBacks(Pagination pagination)
    {
        var feedback = await _context.FeedBack.Include(x => x.Client)
            .Skip((pagination.PageIndex - 1) * pagination.PageSize)
            .Take(pagination.PageSize).OrderByDescending(x => x.CreationDate).ToListAsync();

        var totalCount = await _context.FeedBack.CountAsync();

        var response = feedback.Select(x => new GettingFeedback
        {
            Id = x.Id,
            Content = x.Feedback,
            OwnerId = x.Client.Id.ToString(),
            OwnerName = x.Client.User.DisplayName,
            OwnerEmail = x.Client.User.Email,
            OwnerPhoneNumber = x.Client.User.PhoneNumber,
        }).ToList();
        return new BaseResponse<PagedResult<GettingFeedback?>>() { Code = ErrorCode.Success, Message = "Getting list of feedback", Data = new PagedResult<GettingFeedback?>(){ Items = response, TotalCount = totalCount } };
    }

    public async Task<BaseResponse<PagedResult<AdminUserData>>> GetUsers(RequestUsers usersRequest)
    {
        var users = _context.Clients.Where(x=>x.User.IsActive == usersRequest.IsActive).AsQueryable();

        if (!string.IsNullOrEmpty(usersRequest.UserId))
        {
            users = users.Where(x => x.Id.ToString() == usersRequest.UserId);
        }
        
        if (!string.IsNullOrEmpty(usersRequest.SearchKey))
        {
            users = users.Where(x => x.User.Email.Contains(usersRequest.SearchKey) ||
                                     x.User.DisplayName.Contains(usersRequest.SearchKey) ||
                                     x.User.PhoneNumber.Contains(usersRequest.SearchKey));
        }
        var count = users.Count();
        var results = await users.Skip((usersRequest.Pagination.PageIndex - 1) * usersRequest.Pagination.PageSize)
            .Take(usersRequest.Pagination.PageSize).OrderBy(x => x.User.DisplayName)
            .Select(x=> new AdminUserData(x.Id.ToString(),x.User.DisplayName,x.User.Email,x.User.PhoneNumber,x.BirthDate,x.Gender.ToString(),x.User.IsActive)).ToListAsync();

        return new BaseResponse<PagedResult<AdminUserData>> { Code = ErrorCode.Success, Message = "Get all users details done successfully", Data = new PagedResult<AdminUserData>() { Items = results, TotalCount = count } };
    }
}