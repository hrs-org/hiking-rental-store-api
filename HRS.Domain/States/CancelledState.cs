namespace HRS.Domain.States;

/// <summary>
/// Concrete State: Cancelled
/// Represents a cancelled rental order - terminal state
/// No valid transitions from this state
/// </summary>
public class CancelledState : BaseRentalOrderState
{
    public override string GetStateName() => "Cancelled";

    public override bool CanTransitionTo(string targetState)
    {
        // Terminal state - no transitions allowed
        return false;
    }
}
