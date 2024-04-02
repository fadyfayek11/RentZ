using RentZ.DTO.Enums;

namespace RentZ.DTO.Messages;

public class MessageDto
{
    public int Id { get; set; }
    public int ConversationId { get; set; }
    public DateTime SendAt { get; set; }
    public string Content { get; set; }

    public string SenderId { get; set; }
    public string SenderName { get; set; }
    
    public string ReceiverId { get; set; }
    public string ReceiverName { get; set; }
}

public class ConversationDto
{
    public int Id { get; set; }
    public int PropId { get; set; }
    public PropertyType PropertyType { get; set; }
    public string PropertyName { get; set; }
    public DateTime SendAt { get; set; }

    public string OwnerId { get; set; }

    public string SenderId { get; set; }
    public string SenderName { get; set; }
    public string SenderImageUrl { get; set; }
    
    public string ReceiverId { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverImageUrl { get; set; }

    public bool IsReadBySender { get; set; }
    public bool IsSenderOnline { get; set; }
    public bool IsReadByReceiver { get; set; }
    public bool IsReceiverOnline { get; set; }
}