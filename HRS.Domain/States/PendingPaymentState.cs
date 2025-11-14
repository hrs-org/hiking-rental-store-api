using HRS.Domain.Entities;
using HRS.Domain.Enums;

namespace HRS.Domain.States;

/// <summary>
/// Concrete State: PendingPayment
/// Represents an order waiting for payment confirmation
/// Valid transitions: → Pending/Booked/Rented (on payment approval), → Cancelled
/// </summary>
public class PendingPaymentState : BaseRentalOrderState
{
    public override void ApprovePayment(RentalOrder order, int approvedById, string channel)
    {
        // Determine next status based on channel
        order.Status = channel switch
        {
            "POS" => RentalStatus.Rented,
            "Manual" => RentalStatus.Booked,
            _ => RentalStatus.Pending
        };

        if (order.Status is RentalStatus.Rented or RentalStatus.Booked)
        {
            order.ApprovedById = approvedById;
            order.ApprovedAt = DateTime.UtcNow;
        }

        UpdateOrderMetadata(order, approvedById);

        // Set the new state
        order.State = order.Status switch
        {
            RentalStatus.Rented => new RentedState(),
            RentalStatus.Booked => new BookedState(),
            RentalStatus.Pending => new PendingState(),
            _ => this
        };
    }

    public override void Cancel(RentalOrder order, int cancelledById)
    {
        order.Status = RentalStatus.Cancelled;
        order.ApprovedById = cancelledById;
        order.ApprovedAt = DateTime.UtcNow;
        UpdateOrderMetadata(order, cancelledById);
        order.State = new CancelledState();
    }

    public override string GetStateName() => "PendingPayment";

    public override bool CanTransitionTo(string targetState)
    {
        return targetState switch
        {
            "Pending" => true,
            "Booked" => true,
            "Rented" => true,
            "Cancelled" => true,
            _ => false
        };
    }
}
