using RentZ.DTO.Enums;

namespace RentZ.DTO.Property;

public class PropertyChangeStatus
{
    public int PropId { get; set; }
    public PropertyStatus Status { get; set; }
}