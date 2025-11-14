using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRS.Domain.Enums;
using HRS.Domain.States;

namespace HRS.Domain.Entities;

[Table("RentalOrders")]
public class RentalOrder
{
    [Key] public int Id { get; set; }
    public int? CustomerId { get; set; }
    [ForeignKey(nameof(CustomerId))] public User? Customer { get; set; }
    [MaxLength(150)] public string? GuestName { get; set; }
    [MaxLength(50)] public string? GuestPhone { get; set; }
    [MaxLength(150)] public string? GuestEmail { get; set; }
    [Required] public DateTime StartDate { get; set; }
    [Required] public DateTime EndDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalAmount { get; set; }

    [Required] public RentalStatus Status { get; set; } = RentalStatus.Pending;
    [Required] public OrderChannel Channel { get; set; } = OrderChannel.Online;
    [Required] public OrderPaymentType PaymentType { get; set; } = OrderPaymentType.Other;
    public int? ApprovedById { get; set; }
    [ForeignKey(nameof(ApprovedById))] public User? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ReturnedById { get; set; }
    [ForeignKey(nameof(ReturnedById))] public User? ReturnedBy { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public int? ClosedById { get; set; }
    [ForeignKey(nameof(ClosedById))] public User? ClosedBy { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool HasIssues { get; set; }
    public int ItemsGoodCount { get; set; }
    public int ItemsIssueCount { get; set; }
    public string? ReturnRemarks { get; set; }
    public string? StripeSessionId { get; set; }

    // State Pattern: Current state of the rental order
    [NotMapped]
    public IRentalOrderState State { get; set; } = new PendingState();

    public ICollection<RentalOrderItem> RentalOrderItems { get; set; } = [];
    public ICollection<RentalOrderPackage> RentalOrderPackages { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public int CreatedById { get; set; }
    [ForeignKey(nameof(CreatedById))] public User? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedById { get; set; }
    [ForeignKey(nameof(UpdatedById))] public User? UpdatedBy { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Initialize the state based on current status
    /// This is called when loading from database to restore the correct state object
    /// </summary>
    public void InitializeState()
    {
        State = Status switch
        {
            RentalStatus.PendingPayment => new PendingPaymentState(),
            RentalStatus.Pending => new PendingState(),
            RentalStatus.Booked => new BookedState(),
            RentalStatus.Rented => new RentedState(),
            RentalStatus.Returned => new ReturnedState(),
            RentalStatus.Completed => new CompletedState(),
            RentalStatus.Cancelled => new CancelledState(),
            _ => new PendingState()
        };
    }
}
