namespace RentZ.DTO.Feedback;

public class GettingFeedback
{
    public int Id { get; set; }
    public string Content { get; set; }
    public string OwnerId { get; set; }
    public string OwnerName { get; set; }
    public string OwnerEmail { get; set; }
    public string OwnerPhoneNumber { get; set; }
}