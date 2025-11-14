using HRS.Domain.Entities;

namespace HRS.Domain.States;

/// <summary>
/// State interface - defines operations that can be performed on a rental order
/// Each concrete state will implement these operations based on valid transitions
/// </summary>
public interface IRentalOrderState
{
    /// <summary>
    /// Approves the rental order (transitions to Booked)
    /// </summary>
    void Approve(RentalOrder order, int approvedById);

    /// <summary>
    /// Approves payment for the rental order
    /// </summary>
    void ApprovePayment(RentalOrder order, int approvedById, string channel);

    /// <summary>
    /// Cancels the rental order
    /// </summary>
    void Cancel(RentalOrder order, int cancelledById);

    /// <summary>
    /// Marks the order as rented (items picked up)
    /// </summary>
    void MarkAsRented(RentalOrder order, int updatedById);

    /// <summary>
    /// Marks the order as returned
    /// </summary>
    void MarkAsReturned(RentalOrder order, int returnedById, string? remarks);

    /// <summary>
    /// Closes/completes the rental order
    /// </summary>
    void Close(RentalOrder order, int closedById);

    /// <summary>
    /// Gets the current status name
    /// </summary>
    string GetStateName();

    /// <summary>
    /// Checks if the transition to another state is valid
    /// </summary>
    bool CanTransitionTo(string targetState);
}
