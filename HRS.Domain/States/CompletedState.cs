namespace HRS.Domain.States;

/// <summary>
/// Concrete State: Completed
/// Represents a finalized rental order - terminal state
/// No valid transitions from this state
/// </summary>
public class CompletedState : BaseRentalOrderState
{
  public override string GetStateName() => "Completed";

  public override bool CanTransitionTo(string targetState)
  {
    // Terminal state - no transitions allowed
    return false;
  }
}
