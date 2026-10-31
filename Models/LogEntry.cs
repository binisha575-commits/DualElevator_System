using System;

namespace Lift.Models
{
    public class LogEntry
    {
        public int Id { get; set; }
        public int ElevatorId { get; set; }
        public string Action { get; set; } = string.Empty;
        public int Floor { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss}] Elevator {ElevatorId}: {Action} (Floor {Floor})";
        }
    }
}
