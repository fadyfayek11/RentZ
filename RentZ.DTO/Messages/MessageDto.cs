namespace RentZ.DTO.Messages;

public class MessageDto
{
    public int Id { get; set; }
    public DateTime SendAt { get; set; }
    public string Content { get; set; }

    public string SenderId { get; set; }
    public string SenderName { get; set; }
    
    public string ReceiverId { get; set; }
    public string ReceiverName { get; set; }
}