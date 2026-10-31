using Lift.Models;

namespace Lift.State
{
    public class DoorOpenState : IElevatorState
    {
        public void HandleRequest(Elevator elevator, int targetFloor)
        {
            // Doors are open – new request will be handled after doors close
        }

        public string GetStateName() => "DoorOpen";
    }
}
