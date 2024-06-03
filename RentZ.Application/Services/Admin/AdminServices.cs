using Azure.Core;
using ClosedXML.Excel;
using ExtCore.FileStorage.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RentZ.Application.Services.Notification;
using RentZ.Domain.Entities;
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
        var users = _context.Clients.AsQueryable();

        if (usersRequest.IsActive.HasValue)
        {
            users = users.Where(x => x.User.IsActive == usersRequest.IsActive);
        }

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

    public async Task<BaseResponse<bool>> LockUserAccount(string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id.ToString() == userId);
        if (user == null) return new BaseResponse<bool>() { Code = ErrorCode.BadRequest, Message = "Fail to find user", Data = false };

        
        user.IsActive = !user.IsActive;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return new BaseResponse<bool>() { Code = ErrorCode.Success, Message = user.IsActive ? "Activate user done" : "Deactivate user done", Data = user.IsActive };
    }

    public byte[] ExportUsersData(PagedResult<AdminUserData> users)
    {
        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("Admin Users");

            // Add header row
            worksheet.Cell(1, 1).Value = "User Id";
            worksheet.Cell(1, 2).Value = "Display Name";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "PhoneNumber";
            worksheet.Cell(1, 5).Value = "BirthDate";
            worksheet.Cell(1, 6).Value = "Gender";
            worksheet.Cell(1, 7).Value = "Is Active User";

            var headerRange = worksheet.Range(1, 1, 1, 7);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Add data rows
            for (int i = 0; i < users.TotalCount; i++)
            {
                var user = users.Items[i];
                worksheet.Cell(i + 2, 1).Value = user.UserId;
                worksheet.Cell(i + 2, 2).Value = user.DisplayName;
                worksheet.Cell(i + 2, 3).Value = user.Email;
                worksheet.Cell(i + 2, 4).Value = user.PhoneNumber;
                worksheet.Cell(i + 2, 5).Value = user.BirthDate.ToShortDateString();
                worksheet.Cell(i + 2, 6).Value = user.Gender;
                worksheet.Cell(i + 2, 7).Value = user.IsActive;
            }

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);

                return stream.ToArray();
            }
        }
    }

}