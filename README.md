# Dual Elevator Control System (C# WinForms)

## Overview
Object-oriented elevator control application for a two-floor building with **two elevators**.

Implements:
- **Task 1** – GUI matching the provided elevator panel design
- **Task 2** – Control logic for external & internal requests
- **Task 3** – SQL Server LocalDB logging, exception handling, no code duplication
- **Task 4** – Smooth animation using Timers + delegates
- **Task 5.1** – BackgroundWorker for database operations
- **Task 5.2** – Dual elevators + State Pattern + Observer Pattern + best-available dispatch

## Folder Structure
```
Lift/
├── Controller/          # ElevatorController (dispatch & movement)
├── Data/                # DataManager (avoids duplication)
├── Database/            # SQL Server LocalDB helper
├── Models/              # Elevator, LogEntry
├── Observer/            # IObserver
├── State/               # IdleState, MovingState, DoorOpenState
├── Subject/             # ISubject
├── UI/                  # MainForm, ElevatorPanelControl, ControlPanelControl, LogForm
├── Lift.csproj
├── Program.cs
└── README.md
```

---

## New Laptop Setup (Full Instructions)

This project runs on **Windows only** (WinForms). Follow the steps below on a fresh laptop.

### What you need to install

| # | Software | Required? | Why |
|---|----------|-----------|-----|
| 1 | **Windows 10 or Windows 11** | Yes | WinForms apps do not run on macOS/Linux |
| 2 | **Visual Studio 2022** (Community is free) **or** **.NET 8 SDK** | Yes (pick one path) | Build and run the C# project |
| 3 | **SQL Server Express LocalDB** | Yes | Stores elevator operation logs (`LiftDB`) |
| 4 | **Git** (optional) | No | Only if you clone the project from a repository |

> Tip: Installing **Visual Studio 2022** with the **.NET desktop development** workload usually installs both the .NET SDK and LocalDB for you. That is the easiest path for beginners.

---

### Option A — Easiest (Recommended): Visual Studio 2022

#### Step 1: Download Visual Studio 2022
1. Open a browser and go to: https://visualstudio.microsoft.com/downloads/
2. Download **Visual Studio 2022 Community** (free).

#### Step 2: Install with the correct workload
1. Run the installer.
2. On the **Workloads** tab, check:
   - **.NET desktop development**
3. On the right side (Installation details), make sure these are included:
   - .NET desktop development tools
   - .NET 8.0 Runtime / SDK (or latest .NET SDK)
   - **SQL Server Express LocalDB** (under Optional components — enable it if not already checked)
4. Click **Install** and wait until it finishes.
5. Restart the laptop if the installer asks you to.

#### Step 3: Verify LocalDB is installed
1. Open **Command Prompt** or **PowerShell**.
2. Run:
   ```bash
   sqllocaldb info
   ```
3. You should see `MSSQLLocalDB` in the list.
4. Start it if needed:
   ```bash
   sqllocaldb start MSSQLLocalDB
   ```

#### Step 4: Open and run the project
1. Open **Visual Studio 2022**.
2. Choose **Open a project or solution**.
3. Browse to this folder and open `Lift.csproj`.
4. Press **F5** (or click the green Start button).
5. The Dual Elevator window should open.
6. On first run, the app automatically creates the `LiftDB` database and `ElevatorLog` table.

---

### Option B — Lightweight: .NET 8 SDK + LocalDB (no Visual Studio)

Use this if you prefer VS Code / Cursor / command line only.

#### Step 1: Install .NET 8 SDK
1. Go to: https://dotnet.microsoft.com/download/dotnet/8.0
2. Download the **.NET 8 SDK** for Windows (x64).
3. Install it, then open a **new** PowerShell window and verify:
   ```bash
   dotnet --version
   ```
   You should see a version starting with `8.` (for example `8.0.xxx`).

#### Step 2: Install SQL Server Express LocalDB
LocalDB is required because the app connects to `(localdb)\MSSQLLocalDB`.

**Easiest ways to get LocalDB:**
1. Install **SQL Server Express** and select LocalDB, from:  
   https://www.microsoft.com/sql-server/sql-server-downloads  
   (choose Express → download → during setup include LocalDB),  
   **or**
2. Install the **SQL Server Express LocalDB** package / Media (often installed via Visual Studio Installer → Individual components → search “LocalDB”).

After install, verify:
```bash
sqllocaldb info
sqllocaldb start MSSQLLocalDB
```

#### Step 3: Run the project from the terminal
```bash
cd "C:\path\to\DualElevatorSystem\Lift"
dotnet restore
dotnet run
```

The GUI should open. The database `LiftDB` is created automatically on first launch.

---

### Optional tools (not required)

| Tool | Purpose |
|------|---------|
| **Git** | Clone / pull the project — https://git-scm.com/download/win |
| **Visual Studio Code** or **Cursor** | Edit code (still need .NET SDK to build/run) |
| **SQL Server Management Studio (SSMS)** | Browse `LiftDB` tables if you want — https://aka.ms/ssmsfullsetup |

---

## Requirements (summary)

- **OS:** Windows 10 / 11
- **Framework:** .NET 8 (`net8.0-windows`)
- **UI:** Windows Forms
- **Database:** SQL Server Express LocalDB instance `MSSQLLocalDB`, database name `LiftDB`

## How to Run

### Visual Studio
Open `Lift.csproj` → press **F5**.

### Command line
```bash
cd Lift
dotnet restore
dotnet run
```

### Self-tests (console)
```bash
dotnet run -- --test
```

## Features
- Two elevators with independent control panels
- External call buttons (▲/▼) on each floor – system assigns the nearest/idle elevator
- Internal floor selection buttons per elevator
- Animated car movement with open/closed door visuals
- Real-time floor display on each control panel
- Persistent operation log stored in SQL Server LocalDB (`LiftDB` / `ElevatorLog`)
- Log viewer with Refresh / Clear
- State Design Pattern for elevator states
- Observer pattern for UI updates
- BackgroundWorker for non-blocking DB reads

## Design Patterns Used
| Pattern   | Location                          |
|-----------|-----------------------------------|
| State     | `State/*` + `Elevator.SetState`   |
| Observer  | `Observer/IObserver`, `Elevator`  |
| Controller| `Controller/ElevatorController`   |

## Database
The app uses **SQL Server LocalDB**:
- Instance: `(localdb)\MSSQLLocalDB`
- Database: `LiftDB` (created automatically if missing)
- Table: `dbo.ElevatorLog` (Id, ElevatorId, Action, Floor, Timestamp)

Connection uses Windows Integrated Security (no username/password to configure).

If startup fails with a database error:
1. Confirm LocalDB is installed: `sqllocaldb info`
2. Start the instance: `sqllocaldb start MSSQLLocalDB`
3. Run the app again

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `'dotnet' is not recognized` | Install .NET 8 SDK and open a **new** terminal |
| `Unable to connect to SQL Server` | Install/start LocalDB (`sqllocaldb start MSSQLLocalDB`) |
| Project won't open / wrong target | Install **.NET desktop development** workload in Visual Studio |
| App builds but no window | Confirm you are on Windows and using `net8.0-windows` |
| NuGet restore fails | Check internet connection, then run `dotnet restore` again |
