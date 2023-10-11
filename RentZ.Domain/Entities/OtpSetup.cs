#nullable disable
using System.ComponentModel.DataAnnotations;

namespace RentZ.Domain.Entities;

public class OtpSetup
{
    [Key]
    public Guid Id { get; set; }
    public string Code { get; set; }
    public DateTime ExpiryDate { get; set; }

    public virtual User User { get; set; }
}

