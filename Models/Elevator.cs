using System;
using System.Collections.Generic;
using System.Linq;
using Lift.State;
using Lift.Observer;
using Lift.Subject;

namespace Lift.Models
{
    public enum Direction
    {
        Up,
        Down,
        Idle
    }

    public class Elevator : ISubject
    {
        public int Id { get; }
        public int CurrentFloor { get; private set; }
        public Direction CurrentDirection { get; private set; }
        public bool IsDoorOpen { get; private set; }
        public bool IsMoving { get; private set; }
        public IElevatorState State { get; private set; }

        private readonly List<IObserver> _observers = new();

        public Elevator(int id, int startFloor = 1)
        {
            Id = id;
            CurrentFloor = startFloor;
            CurrentDirection = Direction.Idle;
            IsDoorOpen = false;
            IsMoving = false;
            State = new IdleState();
        }

        public void SetState(IElevatorState newState)
        {
            State = newState;
            NotifyObservers();
        }

        public void SetFloor(int floor)
        {
            CurrentFloor = floor;
            NotifyObservers();
        }

        public void SetDirection(Direction dir)
        {
            CurrentDirection = dir;
            NotifyObservers();
        }

        public void SetDoorOpen(bool open)
        {
            IsDoorOpen = open;
            NotifyObservers();
        }

        public void SetMoving(bool moving)
        {
            IsMoving = moving;
            NotifyObservers();
        }

        public void RequestFloor(int floor)
        {
            State.HandleRequest(this, floor);
        }

        // ISubject
        public void Attach(IObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Detach(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyObservers()
        {
            foreach (var obs in _observers.ToList())
            {
                obs.Update(this);
            }
        }

        public int EstimatedResponseTime(int requestFloor)
        {
            // Simple heuristic: distance + penalty if busy
            int distance = Math.Abs(CurrentFloor - requestFloor);
            if (IsMoving || IsDoorOpen)
                distance += 2; // penalty for ongoing task
            return distance;
        }
    }
}
