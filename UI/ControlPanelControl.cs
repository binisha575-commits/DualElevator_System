using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Lift.Models;

namespace Lift.UI
{
    /// <summary>
    /// In-car control panel finished in the same brushed stainless steel
    /// as the elevator doors — square geometry, industrial push buttons.
    /// </summary>
    public class ControlPanelControl : Panel
    {
        private readonly int _elevatorId;
        private int _currentFloor = 1;
        private bool _isDoorOpen = false;
        private bool _isMoving = false;
        private Direction _direction = Direction.Idle;

        private Button _btn1 = null!;
        private Button _btnG = null!;
        private Button _btnDoorOpen = null!;
        private Button _btnDoorClose = null!;
        private readonly ToolTip _toolTip = new ToolTip();

        public event Action<int>? FloorSelected;
        public event Action? DoorOpenRequested;
        public event Action? DoorCloseRequested;

        public ControlPanelControl(int elevatorId)
        {
            _elevatorId = elevatorId;
            DoubleBuffered = true;
            this.Size = new Size(165, 420);
            this.BackColor = Color.FromArgb(232, 230, 226);
            this.BorderStyle = BorderStyle.None;

            InitializeUI();
        }

        private void InitializeUI()
        {
            int btnW = 56;
            int btnH = 48;
            int btnX = (Width - btnW) / 2;

            _btn1 = CreateFloorButton("1", "LEVEL 1", new Point(btnX, 158), new Size(btnW, btnH));
            _toolTip.SetToolTip(_btn1, "Travel to Floor 1");
            _btn1.Click += (s, e) =>
            {
                SoundHelper.PlayButtonClick();
                FloorSelected?.Invoke(2);
            };

            _btnG = CreateFloorButton("G", "GROUND", new Point(btnX, 220), new Size(btnW, btnH));
            _toolTip.SetToolTip(_btnG, "Travel to Ground Floor (G)");
            _btnG.Click += (s, e) =>
            {
                SoundHelper.PlayButtonClick();
                FloorSelected?.Invoke(1);
            };

            int doorW = 52;
            int doorH = 44;
            int gap = 10;
            int startDoorX = (Width - (doorW * 2 + gap)) / 2;
            int doorY = 292;

            _btnDoorOpen = CreateDoorButton(isOpen: true, new Point(startDoorX, doorY), new Size(doorW, doorH));
            _toolTip.SetToolTip(_btnDoorOpen, "Open Elevator Doors");
            _btnDoorOpen.Click += (s, e) =>
            {
                SoundHelper.PlayButtonClick();
                DoorOpenRequested?.Invoke();
            };

            _btnDoorClose = CreateDoorButton(isOpen: false, new Point(startDoorX + doorW + gap, doorY), new Size(doorW, doorH));
            _toolTip.SetToolTip(_btnDoorClose, "Close Elevator Doors");
            _btnDoorClose.Click += (s, e) =>
            {
                SoundHelper.PlayButtonClick();
                DoorCloseRequested?.Invoke();
            };
        }

        private Button CreateFloorButton(string mainText, string subText, Point loc, Size size)
        {
            var btn = new Button
            {
                Location = loc,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = false
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.None;
                var rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                bool isLit = (bool)(btn.Tag ?? false);

                // Flat brushed metal face — square, no soft AI rounding
                Color faceTop = isLit ? Color.FromArgb(230, 210, 90) : Color.FromArgb(200, 200, 204);
                Color faceBot = isLit ? Color.FromArgb(190, 150, 40) : Color.FromArgb(155, 155, 160);

                using (var brush = new LinearGradientBrush(rect, faceTop, faceBot, LinearGradientMode.Vertical))
                    g.FillRectangle(brush, rect);

                // Vertical grain
                using (var grain = new Pen(Color.FromArgb(22, 255, 255, 255), 1))
                {
                    for (int x = 2; x < btn.Width - 2; x += 3)
                        g.DrawLine(grain, x, 2, x, btn.Height - 3);
                }

                // Bevel: light top/left, dark bottom/right
                using (var lightPen = new Pen(Color.FromArgb(230, 230, 232), 1.5f))
                using (var darkPen = new Pen(Color.FromArgb(95, 95, 100), 1.5f))
                {
                    g.DrawLine(lightPen, 0, 0, btn.Width - 1, 0);
                    g.DrawLine(lightPen, 0, 0, 0, btn.Height - 1);
                    g.DrawLine(darkPen, btn.Width - 1, 0, btn.Width - 1, btn.Height - 1);
                    g.DrawLine(darkPen, 0, btn.Height - 1, btn.Width - 1, btn.Height - 1);
                }

                Color mainColor = isLit ? Color.FromArgb(40, 30, 0) : Color.FromArgb(35, 36, 40);
                using (var font = new Font("Segoe UI", 15F, FontStyle.Bold))
                using (var brush = new SolidBrush(mainColor))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near };
                    g.DrawString(mainText, font, brush, new RectangleF(0, 5, btn.Width, 24), sf);
                }

                Color subColor = isLit ? Color.FromArgb(70, 50, 10) : Color.FromArgb(90, 92, 98);
                using (var subFont = new Font("Segoe UI", 6.5F, FontStyle.Bold))
                using (var subBrush = new SolidBrush(subColor))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
                    g.DrawString(subText, subFont, subBrush, new RectangleF(0, 28, btn.Width, 16), sf);
                }
            };

            this.Controls.Add(btn);
            return btn;
        }

        private Button CreateDoorButton(bool isOpen, Point loc, Size size)
        {
            var btn = new Button
            {
                Location = loc,
                Size = size,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Tag = false
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.None;
                var rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                bool isLit = (bool)(btn.Tag ?? false);

                Color faceTop = isLit ? Color.FromArgb(180, 210, 185) : Color.FromArgb(195, 195, 198);
                Color faceBot = isLit ? Color.FromArgb(110, 160, 120) : Color.FromArgb(150, 150, 154);

                using (var brush = new LinearGradientBrush(rect, faceTop, faceBot, LinearGradientMode.Vertical))
                    g.FillRectangle(brush, rect);

                using (var grain = new Pen(Color.FromArgb(20, 255, 255, 255), 1))
                {
                    for (int x = 2; x < btn.Width - 2; x += 3)
                        g.DrawLine(grain, x, 2, x, btn.Height - 3);
                }

                using (var lightPen = new Pen(Color.FromArgb(225, 225, 228), 1.5f))
                using (var darkPen = new Pen(Color.FromArgb(95, 95, 100), 1.5f))
                {
                    g.DrawLine(lightPen, 0, 0, btn.Width - 1, 0);
                    g.DrawLine(lightPen, 0, 0, 0, btn.Height - 1);
                    g.DrawLine(darkPen, btn.Width - 1, 0, btn.Width - 1, btn.Height - 1);
                    g.DrawLine(darkPen, 0, btn.Height - 1, btn.Width - 1, btn.Height - 1);
                }

                int cx = btn.Width / 2;
                int cy = btn.Height / 2 - 4;
                Color iconColor = isLit ? Color.FromArgb(25, 55, 30) : Color.FromArgb(45, 46, 50);
                using var iconBrush = new SolidBrush(iconColor);

                g.FillRectangle(iconBrush, cx - 1, cy - 6, 2, 12);

                if (isOpen)
                {
                    Point[] leftOut = { new Point(cx - 3, cy - 5), new Point(cx - 3, cy + 5), new Point(cx - 10, cy) };
                    Point[] rightOut = { new Point(cx + 3, cy - 5), new Point(cx + 3, cy + 5), new Point(cx + 10, cy) };
                    g.FillPolygon(iconBrush, leftOut);
                    g.FillPolygon(iconBrush, rightOut);
                }
                else
                {
                    Point[] leftIn = { new Point(cx - 10, cy - 5), new Point(cx - 10, cy + 5), new Point(cx - 3, cy) };
                    Point[] rightIn = { new Point(cx + 10, cy - 5), new Point(cx + 10, cy + 5), new Point(cx + 3, cy) };
                    g.FillPolygon(iconBrush, leftIn);
                    g.FillPolygon(iconBrush, rightIn);
                }

                string label = isOpen ? "OPEN" : "CLOSE";
                using var lblFont = new Font("Segoe UI", 6.5F, FontStyle.Bold);
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString(label, lblFont, iconBrush, new RectangleF(0, btn.Height - 15, btn.Width, 12), sf);
            };

            this.Controls.Add(btn);
            return btn;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;

            var panelRect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Same brushed stainless as the doors — square plate, thick edge
            using (var brush = new LinearGradientBrush(
                panelRect,
                Color.FromArgb(198, 198, 202),
                Color.FromArgb(158, 158, 164),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, panelRect);
            }

            // Vertical brushed grain across the whole plate
            using (var grainLight = new Pen(Color.FromArgb(26, 255, 255, 255), 1))
            using (var grainDark = new Pen(Color.FromArgb(14, 40, 40, 45), 1))
            {
                for (int x = 3; x < Width - 3; x += 2)
                {
                    g.DrawLine(grainLight, x, 3, x, Height - 4);
                    if (x % 4 == 0)
                        g.DrawLine(grainDark, x + 1, 3, x + 1, Height - 4);
                }
            }

            // Thick frame like the door architrave
            using (var outerPen = new Pen(Color.FromArgb(105, 104, 100), 2.5f))
            using (var innerPen = new Pen(Color.FromArgb(210, 210, 212), 1.5f))
            {
                g.DrawRectangle(outerPen, panelRect);
                g.DrawRectangle(innerPen, 3, 3, Width - 7, Height - 7);
            }

            // Title stamped into the metal
            using (var titleFont = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.FromArgb(55, 56, 60)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString($"ELEVATOR {_elevatorId}", titleFont, titleBrush,
                    new RectangleF(0, 10, Width, 16), sf);
            }

            // Recessed LED display well
            int screenX = 14;
            int screenY = 32;
            int screenW = Width - 28;
            int screenH = 100;
            var screenRect = new Rectangle(screenX, screenY, screenW, screenH);

            using (var wellBrush = new SolidBrush(Color.FromArgb(12, 12, 14)))
                g.FillRectangle(wellBrush, screenRect);

            // Recessed bevel around the display
            using (var darkPen = new Pen(Color.FromArgb(70, 70, 74), 1.5f))
            using (var lightPen = new Pen(Color.FromArgb(180, 180, 184), 1f))
            {
                g.DrawRectangle(darkPen, screenRect);
                g.DrawLine(lightPen, screenX, screenY + screenH, screenX + screenW, screenY + screenH);
                g.DrawLine(lightPen, screenX + screenW, screenY, screenX + screenW, screenY + screenH);
            }

            Color glowColor = Color.FromArgb(240, 55, 45);
            string floorChar = _currentFloor == 2 ? "1" : "G";
            string dirSymbol = _direction == Direction.Up ? "▲" :
                               (_direction == Direction.Down ? "▼" : "◆");
            Color dirColor = _isMoving
                ? (_direction == Direction.Up ? Color.FromArgb(50, 200, 70) : Color.FromArgb(240, 170, 40))
                : Color.FromArgb(120, 122, 128);

            using (var floorFont = new Font("Consolas", 34F, FontStyle.Bold))
            using (var floorBrush = new SolidBrush(glowColor))
                g.DrawString(floorChar, floorFont, floorBrush, screenX + 28, screenY + 12);

            using (var dirFont = new Font("Segoe UI Symbol", 18F, FontStyle.Bold))
            using (var dirBrush = new SolidBrush(dirColor))
                g.DrawString(dirSymbol, dirFont, dirBrush, screenX + 82, screenY + 24);

            using (var divPen = new Pen(Color.FromArgb(40, 40, 44), 1f))
                g.DrawLine(divPen, screenX + 8, screenY + 68, screenX + screenW - 8, screenY + 68);

            string statusText;
            if (_isDoorOpen)
                statusText = "DOORS OPEN";
            else if (_isMoving)
                statusText = _direction == Direction.Up ? "GOING UP" : "GOING DOWN";
            else
                statusText = _currentFloor == 2 ? "AT FLOOR 1" : "AT GROUND";

            using (var statusFont = new Font("Consolas", 8F, FontStyle.Bold))
            using (var statusBrush = new SolidBrush(_isDoorOpen ? Color.FromArgb(60, 200, 80) : Color.FromArgb(230, 55, 50)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(statusText, statusFont, statusBrush,
                    new RectangleF(screenX + 4, screenY + 72, screenW - 8, 24), sf);
            }

            // Small indicator lamps beside floor buttons
            DrawStatusLamp(g, 24, 180, isLit: _currentFloor == 2);
            DrawStatusLamp(g, 24, 242, isLit: _currentFloor == 1);
        }

        private void DrawStatusLamp(Graphics g, int x, int y, bool isLit)
        {
            int r = 5;
            using (var ringPen = new Pen(Color.FromArgb(90, 90, 94), 1.5f))
                g.DrawEllipse(ringPen, x - r - 1, y - r - 1, (r + 1) * 2, (r + 1) * 2);

            Color core = isLit ? Color.FromArgb(240, 190, 40) : Color.FromArgb(100, 102, 108);
            using (var coreBrush = new SolidBrush(core))
                g.FillEllipse(coreBrush, x - r, y - r, r * 2, r * 2);
        }

        public void UpdateDisplay(int floor, bool doorOpen, bool isMoving, Direction direction)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateDisplay(floor, doorOpen, isMoving, direction)));
                return;
            }

            _currentFloor = floor;
            _isDoorOpen = doorOpen;
            _isMoving = isMoving;
            _direction = direction;

            _btnDoorOpen.Tag = doorOpen;
            _btnDoorOpen.Invalidate();
            Invalidate();
        }

        public void UpdateDisplay(int floor, bool doorOpen)
        {
            UpdateDisplay(floor, doorOpen, _isMoving, _direction);
        }

        public void LightFloorButton(int floor, bool on)
        {
            if (floor == 1)
            {
                _btnG.Tag = on;
                _btnG.Invalidate();
            }
            else if (floor == 2)
            {
                _btn1.Tag = on;
                _btn1.Invalidate();
            }
        }

        public void ClearLights()
        {
            _btnG.Tag = false;
            _btn1.Tag = false;
            _btnDoorClose.Tag = false;
            _btnDoorOpen.Tag = false;
            _btnG.Invalidate();
            _btn1.Invalidate();
            _btnDoorClose.Invalidate();
            _btnDoorOpen.Invalidate();
            Invalidate();
        }
    }
}
