using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Lift.Controller;
using Lift.Models;
using Lift.Database;
using System.ComponentModel;

namespace Lift.UI
{
    public partial class MainForm : Form
    {
        private readonly ElevatorController _controller;
        private ElevatorPanelControl _leftElevator = null!;
        private ElevatorPanelControl _rightElevator = null!;
        private ControlPanelControl _controlPanelLeft = null!;
        private ControlPanelControl _controlPanelRight = null!;
        private Button _btnFloorGUp = null!;
        private Button _btnFloor1Down = null!;
        private Button _btnFloor1Down2 = null!;
        private Button _btnFloorGUp2 = null!;
        private Label _lblTitle = null!;
        private Panel _mainPanel = null!;

        public MainForm()
        {
            _controller = new ElevatorController(2);
            InitializeComponent();
            WireEvents();
            UpdateAllDisplays();
        }

        private void InitializeComponent()
        {
            this.Text = "Dual Elevator Control System";
            this.ClientSize = new Size(940, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(232, 230, 226);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Font = new Font("Segoe UI", 9F);

            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(232, 230, 226)
            };
            this.Controls.Add(_mainPanel);

            // Title centered at top
            _lblTitle = new Label
            {
                Text = "Two-Floor Dual Elevator System",
                Font = new Font("Segoe UI Semibold", 16F),
                ForeColor = Color.FromArgb(50, 50, 60),
                Size = new Size(940, 34),
                Location = new Point(0, 14),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            _mainPanel.Controls.Add(_lblTitle);

            // ===== EXTERNAL FLOOR CALL BUTTONS (LEFT SIDE, MATCHING REFERENCE IMAGE) =====
            // Top floor: First Floor call button (▼ down to ground)
            var lblF1Left = new Label
            {
                Text = "Floor 1",
                Location = new Point(14, 115),
                AutoSize = true,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 102, 110)
            };
            _mainPanel.Controls.Add(lblF1Left);

            _btnFloor1Down = CreateSleekCallButton("▼", new Point(18, 140), "Call Elevator (Down to Ground Floor)");

            // Bottom floor: Ground Floor call button (▲ up to floor 1)
            var lblFGLeft = new Label
            {
                Text = "Floor G",
                Location = new Point(14, 335),
                AutoSize = true,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 102, 110)
            };
            _mainPanel.Controls.Add(lblFGLeft);

            _btnFloorGUp = CreateSleekCallButton("▲", new Point(18, 360), "Call Elevator (Up to First Floor)");

            // =========================================================================
            // SEQUENCE: [ ELEVATOR 1 SHAFT ] [ ELEVATOR 1 CONTROL PANEL ]
            //           [ ELEVATOR 2 SHAFT ] [ ELEVATOR 2 CONTROL PANEL ]
            // =========================================================================

            // 1. ELEVATOR 1 SHAFT
            _leftElevator = new ElevatorPanelControl(1)
            {
                Location = new Point(75, 60),
                Size = new Size(200, 480)
            };
            _mainPanel.Controls.Add(_leftElevator);

            // 2. ELEVATOR 1 CONTROL PANEL (immediately to the right of Elevator 1 shaft)
            _controlPanelLeft = new ControlPanelControl(1)
            {
                Location = new Point(292, 80),
                Size = new Size(160, 420)
            };
            _mainPanel.Controls.Add(_controlPanelLeft);

            // 3. ELEVATOR 2 SHAFT (after Elevator 1 control panel)
            _rightElevator = new ElevatorPanelControl(2)
            {
                Location = new Point(478, 60),
                Size = new Size(200, 480)
            };
            _mainPanel.Controls.Add(_rightElevator);

            // 4. ELEVATOR 2 CONTROL PANEL (immediately to the right of Elevator 2 shaft)
            _controlPanelRight = new ControlPanelControl(2)
            {
                Location = new Point(695, 80),
                Size = new Size(160, 420)
            };
            _mainPanel.Controls.Add(_controlPanelRight);

            // ===== EXTERNAL FLOOR CALL BUTTONS (RIGHT SIDE → ELEVATOR 2) =====
            var lblF1Right = new Label
            {
                Text = "Floor 1",
                Location = new Point(878, 115),
                AutoSize = true,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 102, 110)
            };
            _mainPanel.Controls.Add(lblF1Right);

            _btnFloor1Down2 = CreateSleekCallButton("▼", new Point(882, 140),
                "Call Elevator 2 to Floor 1");

            var lblFGR = new Label
            {
                Text = "Floor G",
                Location = new Point(878, 335),
                AutoSize = true,
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 102, 110)
            };
            _mainPanel.Controls.Add(lblFGR);

            _btnFloorGUp2 = CreateSleekCallButton("▲", new Point(882, 360),
                "Call Elevator 2 to Floor G");

            // ===== LOG BUTTON & INSTRUCTIONS (underneath Elevator 2 Control Panel) =====
            var btnLog = new Button
            {
                Text = "Log",
                Size = new Size(100, 32),
                Location = new Point(725, 516),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 222, 228),
                Font = new Font("Segoe UI", 9.5F),
                Cursor = Cursors.Hand
            };
            btnLog.FlatAppearance.BorderColor = Color.FromArgb(150, 153, 160);
            btnLog.Click += BtnLog_Click;
            _mainPanel.Controls.Add(btnLog);

            var lblHelp = new Label
            {
                Text = "Left ▲/▼ → Elevator 1. Right ▲/▼ → Elevator 2.\nHover in a closing doorway to reopen (door sensor).",
                Location = new Point(620, 558),
                Size = new Size(300, 45),
                ForeColor = Color.FromArgb(90, 92, 100),
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.TopCenter
            };
            _mainPanel.Controls.Add(lblHelp);
        }

        private Button CreateSleekCallButton(string arrowText, Point location, string tooltipText)
        {
            var btn = new Button
            {
                Text = arrowText,
                Size = new Size(38, 38),
                Location = location,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = false
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                bool isLit = (bool)(btn.Tag ?? false);

                using var path = RoundedRect(new Rectangle(0, 0, btn.Width - 1, btn.Height - 1), 6);

                // Dark metallic beveled look matching reference image
                Color topCol = isLit ? Color.FromArgb(255, 215, 60) : Color.FromArgb(65, 68, 76);
                Color botCol = isLit ? Color.FromArgb(235, 150, 20) : Color.FromArgb(25, 28, 34);

                using (var brush = new LinearGradientBrush(new Rectangle(0, 0, btn.Width, btn.Height), topCol, botCol, LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }

                using (var pen = new Pen(Color.FromArgb(90, 92, 100), 1.5f))
                {
                    g.DrawPath(pen, path);
                }

                // White arrow icon
                Color arrowCol = isLit ? Color.FromArgb(40, 20, 0) : Color.White;
                using var arrowBrush = new SolidBrush(arrowCol);
                int cx = btn.Width / 2;
                int cy = btn.Height / 2;
                int aw = 10;
                int ah = 8;

                if (arrowText == "▲")
                {
                    Point[] tri = { new Point(cx, cy - ah / 2), new Point(cx - aw / 2, cy + ah / 2), new Point(cx + aw / 2, cy + ah / 2) };
                    g.FillPolygon(arrowBrush, tri);
                }
                else
                {
                    Point[] tri = { new Point(cx - aw / 2, cy - ah / 2), new Point(cx + aw / 2, cy - ah / 2), new Point(cx, cy + ah / 2) };
                    g.FillPolygon(arrowBrush, tri);
                }
            };

            var tt = new ToolTip();
            tt.SetToolTip(btn, tooltipText);
            _mainPanel.Controls.Add(btn);
            return btn;
        }

        private void WireEvents()
        {
            // Left landing buttons → Elevator 1 only
            _btnFloor1Down.Click += (s, e) =>
            {
                SoundHelper.PlayButtonClick();
                CallElevator(1, 2, _btnFloor1Down); // Floor 1
            };
            _btnFloorGUp.Click += (s, e) =>
            {
                SoundHelper.PlayButtonClick();
                CallElevator(1, 1, _btnFloorGUp); // Floor G
            };

            // Right landing buttons → Elevator 2 only
            _btnFloor1Down2.Click += (s, e) =>
            {
                SoundHelper.PlayButtonClick();
                CallElevator(2, 2, _btnFloor1Down2); // Floor 1
            };
            _btnFloorGUp2.Click += (s, e) =>
            {
                SoundHelper.PlayButtonClick();
                CallElevator(2, 1, _btnFloorGUp2); // Floor G
            };

            // Elevator 1 internal panel
            _controlPanelLeft.FloorSelected += (floor) =>
            {
                _controller.HandleInternalRequest(1, floor);
                _controlPanelLeft.LightFloorButton(floor, true);
            };
            _controlPanelLeft.DoorOpenRequested += () => _controller.OpenDoorManual(1);
            _controlPanelLeft.DoorCloseRequested += () => _controller.CloseDoorManual(1);

            // Elevator 2 internal panel
            _controlPanelRight.FloorSelected += (floor) =>
            {
                _controller.HandleInternalRequest(2, floor);
                _controlPanelRight.LightFloorButton(floor, true);
            };
            _controlPanelRight.DoorOpenRequested += () => _controller.OpenDoorManual(2);
            _controlPanelRight.DoorCloseRequested += () => _controller.CloseDoorManual(2);

            // Door safety sensors (cursor in doorway = passenger's hand)
            _leftElevator.DoorObstacleDetected += () => _controller.DoorSensorReopen(1);
            _leftElevator.DoorHoldRequested += () => _controller.HoldDoorsOpen(1);
            _rightElevator.DoorObstacleDetected += () => _controller.DoorSensorReopen(2);
            _rightElevator.DoorHoldRequested += () => _controller.HoldDoorsOpen(2);

            _controller.ElevatorUpdated += OnElevatorUpdated;
            _controller.LogMessage += msg => { /* status logging */ };
        }

        private void CallElevator(int elevatorId, int floor, Button btn)
        {
            btn.Tag = true;
            btn.Invalidate();
            _controller.HandleElevatorCall(elevatorId, floor);
        }

        private void OnElevatorUpdated(Elevator elev)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => OnElevatorUpdated(elev)));
                return;
            }

            if (elev.Id == 1)
            {
                _leftElevator.UpdateElevator(elev);
                _controlPanelLeft.UpdateDisplay(elev.CurrentFloor, elev.IsDoorOpen, elev.IsMoving, elev.CurrentDirection);

                if (!elev.IsMoving)
                {
                    _controlPanelLeft.ClearLights();
                    ClearRequestLights(1, elev.CurrentFloor);
                }
            }
            else
            {
                _rightElevator.UpdateElevator(elev);
                _controlPanelRight.UpdateDisplay(elev.CurrentFloor, elev.IsDoorOpen, elev.IsMoving, elev.CurrentDirection);

                if (!elev.IsMoving)
                {
                    _controlPanelRight.ClearLights();
                    ClearRequestLights(2, elev.CurrentFloor);
                }
            }
        }

        private void ClearRequestLights(int elevatorId, int floor)
        {
            if (elevatorId == 1)
            {
                if (floor == 2)
                {
                    _btnFloor1Down.Tag = false;
                    _btnFloor1Down.Invalidate();
                }
                else
                {
                    _btnFloorGUp.Tag = false;
                    _btnFloorGUp.Invalidate();
                }
            }
            else
            {
                if (floor == 2)
                {
                    _btnFloor1Down2.Tag = false;
                    _btnFloor1Down2.Invalidate();
                }
                else
                {
                    _btnFloorGUp2.Tag = false;
                    _btnFloorGUp2.Invalidate();
                }
            }
        }

        private void UpdateAllDisplays()
        {
            foreach (var e in _controller.Elevators)
            {
                OnElevatorUpdated(e);
            }
        }

        private void BtnLog_Click(object? sender, EventArgs e)
        {
            SoundHelper.PlayButtonClick();
            var worker = new BackgroundWorker();
            worker.DoWork += (s, args) =>
            {
                args.Result = DatabaseHelper.GetAllLogs();
            };
            worker.RunWorkerCompleted += (s, args) =>
            {
                if (args.Error != null)
                {
                    MessageBox.Show("Error loading logs: " + args.Error.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var logs = args.Result as System.Collections.Generic.List<LogEntry> ?? new();
                using var logForm = new LogForm(logs);
                logForm.ShowDialog(this);
            };
            worker.RunWorkerAsync();
        }

        private GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}