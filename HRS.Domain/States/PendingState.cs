using HRS.Domain.Entities;
using HRS.Domain.Enums;

namespace HRS.Domain.States;

/// <summary>
/// Concrete State: Pending
/// Represents an order that is pending approval
/// Valid transitions: → Booked (on approval), → Cancelled
/// </summary>
public class PendingState : BaseRentalOrderState
{
    public override void Approve(RentalOrder order, int approvedById)
    {
        order.Status = RentalStatus.Booked;
        order.ApprovedById = approvedById;
        order.ApprovedAt = DateTime.UtcNow;
        UpdateOrderMetadata(order, approvedById);
        order.State = new BookedState();
    }

    public override void Cancel(RentalOrder order, int cancelledById)
    {
        order.Status = RentalStatus.Cancelled;
        order.ApprovedById = cancelledById;
        order.ApprovedAt = DateTime.UtcNow;
        UpdateOrderMetadata(order, cancelledById);
        order.State = new CancelledState();
    }

    public override string GetStateName() => "Pending";

    public override bool CanTransitionTo(string targetState)
    {
        return targetState switch
        {
            "Booked" => true,
            "Cancelled" => true,
            _ => false
        };
    }
}
