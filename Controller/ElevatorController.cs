using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lift.Models;
using Lift.State;
using Lift.Database;
using Lift.Observer;
using Lift.UI;

namespace Lift.Controller
{
    public class ElevatorController
    {
        private readonly List<Elevator> _elevators = new();
        private readonly Dictionary<int, System.Windows.Forms.Timer> _moveTimers = new();
        private readonly Dictionary<int, System.Windows.Forms.Timer> _doorTimers = new();
        private readonly Dictionary<int, int> _targetFloors = new();

        public event Action<Elevator>? ElevatorUpdated;
        public event Action<string>? LogMessage;

        public IReadOnlyList<Elevator> Elevators => _elevators;

        public ElevatorController(int elevatorCount = 2)
        {
            for (int i = 1; i <= elevatorCount; i++)
            {
                var elev = new Elevator(i, 1);
                _elevators.Add(elev);

                // Timer for movement steps
                var timer = new System.Windows.Forms.Timer { Interval = 2500 };
                int elevId = i;
                timer.Tick += (s, e) => OnMoveTick(elevId);
                _moveTimers[i] = timer;
            }
        }

        public Elevator GetElevator(int id) => _elevators.First(e => e.Id == id);

        public static string GetFloorName(int floor) => floor == 2 ? "1" : "G";

        /// <summary>
        /// External request (floor call buttons). Chooses the best available elevator.
        /// </summary>
        public void HandleExternalRequest(int floor)
        {
            // Prefer idle / closest elevator
            var candidate = _elevators
                .Where(e => !e.IsMoving || e.CurrentFloor == floor)
                .OrderBy(e => e.EstimatedResponseTime(floor))
                .FirstOrDefault();

            if (candidate == null)
            {
                candidate = _elevators.OrderBy(e => e.EstimatedResponseTime(floor)).First();
            }

            Log($"External call for Floor {GetFloorName(floor)} → assigned to Elevator {candidate.Id}");
            DatabaseHelper.LogAction(candidate.Id, $"External call for floor {GetFloorName(floor)}", floor);

            SendToFloor(candidate, floor, isExternal: true);
        }

        /// <summary>
        /// External request targeted directly to a specific elevator (from its dedicated call station).
        /// </summary>
        public void HandleElevatorCall(int elevatorId, int floor)
        {
            var elev = GetElevator(elevatorId);
            Log($"Call Elevator {elevatorId} to Floor {GetFloorName(floor)}");
            DatabaseHelper.LogAction(elevatorId, $"Dedicated call to floor {GetFloorName(floor)}", floor);
            SendToFloor(elev, floor, isExternal: true);
        }

        /// <summary>
        /// Internal request from an elevator's own control panel.
        /// </summary>
        public void HandleInternalRequest(int elevatorId, int floor)
        {
            var elev = GetElevator(elevatorId);
            Log($"Internal request: Elevator {elevatorId} → Floor {GetFloorName(floor)}");
            DatabaseHelper.LogAction(elevatorId, $"Internal button pressed: Floor {GetFloorName(floor)}", floor);
            SendToFloor(elev, floor, isExternal: false);
        }

        /// <summary>
        /// Manual Door Open button pressed on elevator control panel.
        /// </summary>
        public void OpenDoorManual(int elevatorId)
        {
            var elev = GetElevator(elevatorId);
            if (!elev.IsMoving)
            {
                Log($"Elevator {elevatorId} manual door open requested");
                DatabaseHelper.LogAction(elevatorId, "Manual door open pressed", elev.CurrentFloor);
                OpenDoors(elev);
            }
        }

        /// <summary>
        /// Door sensor / light curtain: cursor (passenger) in the doorway while closing → reopen.
        /// </summary>
        public void DoorSensorReopen(int elevatorId)
        {
            var elev = GetElevator(elevatorId);
            if (elev.IsMoving) return;

            Log($"Elevator {elevatorId} door sensor tripped — reopening");
            DatabaseHelper.LogAction(elevatorId, "Door sensor obstruction — doors reopened", elev.CurrentFloor);
            OpenDoors(elev);
        }

        /// <summary>
        /// Keep doors open while the doorway is still blocked (restart auto-close timer).
        /// </summary>
        public void HoldDoorsOpen(int elevatorId)
        {
            var elev = GetElevator(elevatorId);
            if (elev.IsMoving || !elev.IsDoorOpen) return;
            if (!_doorTimers.TryGetValue(elevatorId, out var timer)) return;

            // Restart the 5s hold so doors stay open while the obstruction remains
            timer.Stop();
            timer.Start();
        }

        /// <summary>
        /// Manual Door Close button pressed on elevator control panel.
        /// </summary>
        public void CloseDoorManual(int elevatorId)
        {
            var elev = GetElevator(elevatorId);
            if (!elev.IsMoving && elev.IsDoorOpen)
            {
                if (_doorTimers.TryGetValue(elevatorId, out var timer))
                {
                    timer.Stop();
                    timer.Dispose();
                    _doorTimers.Remove(elevatorId);
                }

                elev.SetDoorOpen(false);
                elev.SetState(new IdleState());
                DatabaseHelper.LogAction(elevatorId, "Manual door close pressed", elev.CurrentFloor);
                ElevatorUpdated?.Invoke(elev);
                Log($"Elevator {elevatorId} doors closed manually at Floor {GetFloorName(elev.CurrentFloor)}");
            }
        }

        private void SendToFloor(Elevator elev, int targetFloor, bool isExternal)
        {
            if (elev.IsMoving && elev.CurrentFloor != targetFloor)
            {
                Log($"Elevator {elev.Id} is busy – request queued");
            }

            _targetFloors[elev.Id] = targetFloor;

            if (elev.CurrentFloor == targetFloor)
            {
                // Already at target floor – open doors
                OpenDoors(elev);
                return;
            }

            // Set moving direction
            elev.SetDirection(targetFloor > elev.CurrentFloor ? Direction.Up : Direction.Down);
            elev.SetMoving(true);
            elev.SetState(new MovingState());
            elev.SetDoorOpen(false);

            // Start continuous audible travel motor sound
            SoundHelper.StartTravelSound();

            DatabaseHelper.LogAction(elev.Id, $"Started moving toward floor {GetFloorName(targetFloor)}", elev.CurrentFloor);
            ElevatorUpdated?.Invoke(elev);

            _moveTimers[elev.Id].Start();
        }

        private void OnMoveTick(int elevatorId)
        {
            var elev = GetElevator(elevatorId);
            if (!_targetFloors.TryGetValue(elevatorId, out int target))
            {
                _moveTimers[elevatorId].Stop();
                SoundHelper.StopTravelSound();
                return;
            }

            if (elev.CurrentFloor == target)
            {
                _moveTimers[elevatorId].Stop();
                elev.SetMoving(false);
                elev.SetDirection(Direction.Idle);

                // Stop travel sound immediately when arriving at destination
                SoundHelper.StopTravelSound();

                DatabaseHelper.LogAction(elev.Id, $"Arrived at floor {GetFloorName(target)}", target);
                Log($"Elevator {elev.Id} arrived at Floor {GetFloorName(target)}");

                OpenDoors(elev);
                return;
            }

            // Transit step sound
            SoundHelper.PlayTransitTone();

            // Move one floor
            int next = elev.CurrentFloor + (elev.CurrentDirection == Direction.Up ? 1 : -1);
            elev.SetFloor(next);
            DatabaseHelper.LogAction(elev.Id, $"Passing floor {GetFloorName(next)}", next);
            ElevatorUpdated?.Invoke(elev);
        }

        private void OpenDoors(Elevator elev)
        {
            // Cancel any pending close timer for this elevator
            if (_doorTimers.TryGetValue(elev.Id, out var existingTimer))
            {
                existingTimer.Stop();
                existingTimer.Dispose();
                _doorTimers.Remove(elev.Id);
            }

            // Play bright arrival chime when doors open
            SoundHelper.PlayChime();

            elev.SetDoorOpen(true);
            elev.SetState(new DoorOpenState());
            DatabaseHelper.LogAction(elev.Id, "Doors opened", elev.CurrentFloor);
            ElevatorUpdated?.Invoke(elev);

            // Auto-close after 5 seconds (realistic door hold time)
            var closeTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            int elevId = elev.Id;
            closeTimer.Tick += (s, e) =>
            {
                closeTimer.Stop();
                closeTimer.Dispose();
                _doorTimers.Remove(elevId);

                var eObj = GetElevator(elevId);
                eObj.SetDoorOpen(false);
                eObj.SetState(new IdleState());
                DatabaseHelper.LogAction(elevId, "Doors closed", eObj.CurrentFloor);
                ElevatorUpdated?.Invoke(eObj);
                Log($"Elevator {elevId} doors closed at Floor {GetFloorName(eObj.CurrentFloor)}");
            };

            _doorTimers[elev.Id] = closeTimer;
            closeTimer.Start();
        }

        private void Log(string msg)
        {
            LogMessage?.Invoke($"[{DateTime.Now:HH:mm:ss}] {msg}");
        }
    }
}
