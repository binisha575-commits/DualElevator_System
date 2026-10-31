using Lift.Observer;

namespace Lift.Subject
{
    // Marker / convenience – Elevator implements the methods directly
    public interface ISubject
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        void NotifyObservers();
    }
}
