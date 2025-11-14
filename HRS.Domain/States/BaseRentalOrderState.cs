using HRS.Domain.Entities;

namespace HRS.Domain.States;

/// <summary>
/// Base abstract class for rental order states
/// Provides default implementations that throw exceptions for invalid transitions
/// Concrete states override only the valid transitions
/// </summary>
public abstract class BaseRentalOrderState : IRentalOrderState
{
    public virtual void Approve(RentalOrder order, int approvedById)
    {
        throw new InvalidOperationException(
            $"Cannot approve order in {GetStateName()} state. Only Pending orders can be approved.");
    }

    public virtual void ApprovePayment(RentalOrder order, int approvedById, string channel)
    {
        throw new InvalidOperationException(
            $"Cannot approve payment for order in {GetStateName()} state. Only PendingPayment orders can have payment approved.");
    }

    public virtual void Cancel(RentalOrder order, int cancelledById)
    {
        throw new InvalidOperationException(
            $"Cannot cancel order in {GetStateName()} state. Only Pending or PendingPayment orders can be cancelled.");
    }

    public virtual void MarkAsRented(RentalOrder order, int updatedById)
    {
        throw new InvalidOperationException(
            $"Cannot mark order as rented in {GetStateName()} state. Only Booked orders can be marked as rented.");
    }

    public virtual void MarkAsReturned(RentalOrder order, int returnedById, string? remarks)
    {
        throw new InvalidOperationException(
            $"Cannot mark order as returned in {GetStateName()} state. Only Rented orders can be returned.");
    }

    public virtual void Close(RentalOrder order, int closedById)
    {
        throw new InvalidOperationException(
            $"Cannot close order in {GetStateName()} state. Only Returned orders can be closed.");
    }

    public abstract string GetStateName();

    public abstract bool CanTransitionTo(string targetState);

    protected static void UpdateOrderMetadata(RentalOrder order, int userId)
    {
        order.UpdatedById = userId;
        order.UpdatedAt = DateTime.UtcNow;
    }
}
