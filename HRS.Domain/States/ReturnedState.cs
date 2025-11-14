using HRS.Domain.Entities;
using HRS.Domain.Enums;

namespace HRS.Domain.States;

/// <summary>
/// Concrete State: Returned
/// Represents an order where items have been returned and are being inspected
/// Valid transitions: → Completed (when inspection is done and order is closed)
/// </summary>
public class ReturnedState : BaseRentalOrderState
{
    public override void Close(RentalOrder order, int closedById)
    {
        order.Status = RentalStatus.Completed;
        order.ClosedById = closedById;
        order.ClosedAt = DateTime.UtcNow;
        UpdateOrderMetadata(order, closedById);
        order.State = new CompletedState();
    }

    public override string GetStateName() => "Returned";

    public override bool CanTransitionTo(string targetState)
    {
        return targetState switch
        {
            "Completed" => true,
            _ => false
        };
    }
}
