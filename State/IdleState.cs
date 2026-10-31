using Lift.Models;

namespace Lift.State
{
    public class IdleState : IElevatorState
    {
        public void HandleRequest(Elevator elevator, int targetFloor)
        {
            if (elevator.CurrentFloor == targetFloor)
            {
                // Already there – open doors briefly
                elevator.SetState(new DoorOpenState());
            }
            else
            {
                elevator.SetDirection(targetFloor > elevator.CurrentFloor ? Direction.Up : Direction.Down);
                elevator.SetState(new MovingState());
            }
        }

        public string GetStateName() => "Idle";
    }
}
