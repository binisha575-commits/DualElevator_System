using Lift.Models;

namespace Lift.Observer
{
    public interface IObserver
    {
        void Update(Elevator elevator);
    }
}
