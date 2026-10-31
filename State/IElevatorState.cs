using Lift.Models;

namespace Lift.State
{
    public interface IElevatorState
    {
        void HandleRequest(Elevator elevator, int targetFloor);
        string GetStateName();
    }
}
