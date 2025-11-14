using HRS.Domain.Entities;
using HRS.Domain.Enums;

namespace HRS.Domain.States;

/// <summary>
/// Concrete State: Booked
/// Represents an approved order that is booked and awaiting pickup
/// Valid transitions: → Rented (when items are picked up)
/// </summary>
public class BookedState : BaseRentalOrderState
{
  public override void MarkAsRented(RentalOrder order, int updatedById)
  {
    order.Status = RentalStatus.Rented;
    UpdateOrderMetadata(order, updatedById);
    order.State = new RentedState();
  }

  public override string GetStateName() => "Booked";

  public override bool CanTransitionTo(string targetState)
  {
    return targetState switch
    {
      "Rented" => true,
      _ => false
    };
  }
}
