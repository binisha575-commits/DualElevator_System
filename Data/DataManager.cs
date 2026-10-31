using System;
using System.Collections.Generic;
using Lift.Models;
using Lift.Database;

namespace Lift.Data
{
    /// <summary>
    /// Thin data access layer to avoid code duplication in event handlers.
    /// </summary>
    public class DataManager
    {
        public void Log(int elevatorId, string action, int floor)
        {
            DatabaseHelper.LogAction(elevatorId, action, floor);
        }

        public List<LogEntry> GetLogs()
        {
            return DatabaseHelper.GetAllLogs();
        }

        public void Clear()
        {
            DatabaseHelper.ClearLogs();
        }
    }
}
