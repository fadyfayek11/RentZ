#nullable disable
namespace RentZ.DTO.Logging;

public class BaseResponse<T>
{
	public string SuccessCode { get; set; }
	public string Message { get; set; }
	public T Data { get; set; }
	public List<string> Errors { get; set; }
}