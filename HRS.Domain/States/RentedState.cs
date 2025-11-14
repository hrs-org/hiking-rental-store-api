using HRS.Domain.Entities;
using HRS.Domain.Enums;

namespace HRS.Domain.States;

/// <summary>
/// Concrete State: Rented
/// Represents an order where items are currently in use by the customer
/// Valid transitions: → Returned (when items are brought back)
/// </summary>
public class RentedState : BaseRentalOrderState
{
    public override void MarkAsReturned(RentalOrder order, int returnedById, string? remarks)
    {
        order.Status = RentalStatus.Returned;
        order.ReturnedById = returnedById;
        order.ReturnedAt = DateTime.UtcNow;
        order.ReturnRemarks = remarks;
        UpdateOrderMetadata(order, returnedById);
        order.State = new ReturnedState();
    }

    public override string GetStateName() => "Rented";

    public override bool CanTransitionTo(string targetState)
    {
        return targetState switch
        {
            "Returned" => true,
            _ => false
        };
    }
}
