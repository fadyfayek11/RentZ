﻿using Microsoft.EntityFrameworkCore;
using RentZ.Application.Services.Notification;
using RentZ.DTO.Enums;
using RentZ.DTO.Feedback;
using RentZ.DTO.Notification;
using RentZ.DTO.Property;
using RentZ.DTO.Response;
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

        await _notificationService.AddNotification(new AddNotification
        {
            Type = (NotificationTypes)request.Status,
            Title = request.Status.ToString(),
            Content = null,
            LinkId = request.PropId,
            ReceiverId = property.OwnerId.ToString(),
            SenderId = adminId
        });

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
}