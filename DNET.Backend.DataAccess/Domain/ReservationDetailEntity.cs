namespace DNET.Backend.DataAccess.Domain;

public class ReservationDetailEntity
{
    public enum ReservationTypeEnum
    {
        Meeting,
        Birthday,
        SpecialEvent
    }
    
    public int Id { get; set; }  // Primary Key
    public int ReservationId { get; set; }  // Foreign Key
    public ReservationTypeEnum ReservationType { get; set; } = ReservationTypeEnum.Meeting;
    public string SpecialRequests { get; set; } = string.Empty;

    // Navigation property
    public ReservationEntity Reservation { get; set; } = null!;
}