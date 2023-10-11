#nullable disable
using RentZ.DTO.Enums;

namespace RentZ.DTO.Response;

public class BaseResponse<T>
{
	public ErrorCode Code { get; set; }
	public string Message { get; set; }
	public T Data { get; set; }
	public List<string> Errors { get; set; }
}