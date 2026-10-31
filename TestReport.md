# Test Report & Self-Assessment

## Project: Dual Elevator Control System

### Marking Matrix with Self-Assessment

| Task | Description | Max Marks | Self Score | Evidence / Notes |
|------|-------------|-----------|------------|------------------|
| **1** | Design and Implement the GUI | 20 | 20 | MainForm + ElevatorPanelControl + ControlPanelControl match the provided UI style (shaft, doors, control panel with display & buttons). Two elevators side-by-side. |
| **2** | Implement the Control Logic | 10 | 10 | External requests dispatched via `HandleExternalRequest`. Internal requests via control-panel buttons. Displays update on every floor change. |
| **3** | Logging and Data Management | 30 | 28 | SQLite with relative path (`Database/elevator.db`). LogForm shows history. DataManager + DatabaseHelper avoid duplication. Exceptions caught gracefully. |
| **4** | Animate Elevator Movement | 10 | 10 | Timer-driven smooth lerp animation in `ElevatorPanelControl` + movement step timer in Controller. |
| **5.1** | Concurrent Task Execution | 5 | 5 | BackgroundWorker used in Log button click to load DB data off the UI thread. |
| **5.2** | Multi-Elevator + Patterns | 15 | 14 | Two elevators. State Pattern (Idle/Moving/DoorOpen). Observer (Elevator notifies UI). Best-available dispatch by estimated response time. |
| **6** | Testing and Self-Assessment | 10 | 9 | This report + manual test cases below. |
| **Total** | | **100** | **96** | |

---

### Manual Test Cases

| # | Test Case | Steps | Expected Result | Pass? |
|---|-----------|-------|-----------------|-------|
| 1 | External call Floor 2 | Click ▲ or ▼ next to Floor 2 | Nearest idle elevator moves to Floor 2, doors open, lights update | ✓ |
| 2 | Internal request | Press "2" on Elevator 1 panel | Elevator 1 only moves to Floor 2 | ✓ |
| 3 | Already at floor | Call floor where elevator already is | Doors open immediately | ✓ |
| 4 | Concurrent requests | Call both floors quickly | Each elevator handles work; no crash | ✓ |
| 5 | Log viewer | Click Log button | Window shows timestamped actions from SQLite | ✓ |
| 6 | Clear log | Clear Log → confirm | Database emptied, list clears | ✓ |
| 7 | Animation | Request different floor | Car slides smoothly between floors | ✓ |
| 8 | Door open indicator | Arrive at floor | Display shows "Floor X *" and doors visually open | ✓ |

---

### How to Build & Run
```bash
cd Lift
dotnet restore
dotnet run
```
Requires .NET 8 SDK + Windows (WinForms).
