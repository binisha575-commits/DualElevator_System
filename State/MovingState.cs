using Lift.Models;

namespace Lift.State
{
    public class MovingState : IElevatorState
    {
        public void HandleRequest(Elevator elevator, int targetFloor)
        {
            // Ignore new internal requests while moving (or queue them – simplified)
            // External dispatcher handles availability
        }

        public string GetStateName() => "Moving";
    }
}
