using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Lift.Models;

namespace Lift.UI
{
    /// <summary>
    /// Elevator shaft with brushed stainless-steel centre-opening doors
    /// and a thick rectangular architrave at each floor landing.
    /// </summary>
    public class ElevatorPanelControl : Panel
    {
        private readonly int _elevatorId;
        private int _currentFloor = 1;
        private bool _doorOpen;
        private bool _isMoving;
        private float _carY;
        private float _targetY;
        private float _doorProgress = 0f;
        private bool _obstacleNotified;

        private readonly System.Windows.Forms.Timer _animTimer;

        private const int ShaftTop = 40;
        private const int FloorHeight = 175;
        private const int CarHeight = 155;
        private const int FloorGap = 48;
        private const int FrameThickness = 14;

        /// <summary>
        /// Fired when the cursor is in the doorway while the doors are closing
        /// (simulates a passenger blocking the door sensors).
        /// </summary>
        public event Action? DoorObstacleDetected;

        /// <summary>
        /// Fired while doors are open and the cursor stays in the doorway
        /// (keeps the doors from auto-closing, like a light curtain).
        /// </summary>
        public event Action? DoorHoldRequested;

        public ElevatorPanelControl(int elevatorId)
        {
            _elevatorId = elevatorId;
            DoubleBuffered = true;
            BackColor = Color.FromArgb(232, 230, 226);

            _carY = GetYForFloor(1);
            _targetY = _carY;

            _animTimer = new System.Windows.Forms.Timer { Interval = 30 };
            _animTimer.Tick += AnimTimer_Tick;
            _animTimer.Start();
        }

        public void UpdateElevator(Elevator elev)
        {
            _currentFloor = elev.CurrentFloor;
            bool wasOpen = _doorOpen;
            _doorOpen = elev.IsDoorOpen;
            _isMoving = elev.IsMoving;
            _targetY = GetYForFloor(_currentFloor);

            // New open cycle → allow another obstacle trigger on the next close
            if (_doorOpen && !wasOpen)
                _obstacleNotified = false;

            Invalidate();
        }

        private float GetYForFloor(int floor)
        {
            if (floor == 2) return ShaftTop + 10;
            return ShaftTop + FloorHeight + FloorGap + 10;
        }

        private void AnimTimer_Tick(object? sender, EventArgs e)
        {
            bool needRedraw = false;

            float diffY = _targetY - _carY;
            if (Math.Abs(diffY) > 0.5f)
            {
                _carY += diffY * 0.04f;
                needRedraw = true;
            }
            else if (_carY != _targetY)
            {
                _carY = _targetY;
                needRedraw = true;
            }

            float targetDoor = _doorOpen ? 1.0f : 0.0f;
            float diffDoor = targetDoor - _doorProgress;
            if (Math.Abs(diffDoor) > 0.02f)
            {
                _doorProgress += diffDoor * 0.06f;
                needRedraw = true;
            }
            else if (_doorProgress != targetDoor)
            {
                _doorProgress = targetDoor;
                needRedraw = true;
            }

            CheckDoorSensor();

            if (needRedraw)
                Invalidate();
        }

        /// <summary>
        /// Real-life door safety: cursor in the doorway acts like a passenger's hand.
        /// While closing → reopen. While open → hold doors open.
        /// </summary>
        private void CheckDoorSensor()
        {
            if (_isMoving) return;

            var pt = PointToClient(Control.MousePosition);
            if (!ClientRectangle.Contains(pt)) return;
            if (!GetActiveDoorHitRect().Contains(pt)) return;

            // Doors are closing (or mid-close animation) → reopen
            if (!_doorOpen && _doorProgress > 0.08f && !_obstacleNotified)
            {
                _obstacleNotified = true;
                DoorObstacleDetected?.Invoke();
                return;
            }

            // Doors fully / mostly open and hand still in the way → hold open
            if (_doorOpen && _doorProgress > 0.5f)
            {
                DoorHoldRequested?.Invoke();
            }
        }

        private Rectangle GetActiveDoorHitRect()
        {
            int shaftW = 148;
            int shaftX = (Width - shaftW) / 2;
            int ft = FrameThickness;
            int landingY = _currentFloor == 2
                ? ShaftTop
                : ShaftTop + FloorHeight + FloorGap;

            return new Rectangle(
                shaftX + ft,
                landingY + ft,
                shaftW - 2 * ft,
                FloorHeight - 2 * ft);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            int shaftW = 148;
            int shaftX = (Width - shaftW) / 2;
            int fgTop = ShaftTop + FloorHeight + FloorGap;

            using (var font = new Font("Segoe UI Semibold", 9.5F))
            using (var brush = new SolidBrush(Color.FromArgb(70, 72, 78)))
            {
                string title = $"Elevator {_elevatorId}";
                var sz = g.MeasureString(title, font);
                g.DrawString(title, font, brush, (Width - sz.Width) / 2, 2);
            }

            DrawPillIndicator(g, shaftX + (shaftW - 74) / 2, ShaftTop - 22, floor: 2);
            DrawPillIndicator(g, shaftX + (shaftW - 74) / 2, fgTop - 22, floor: 1);

            // Floor 1 landing — doors open only when car is here
            bool carAtFloor1 = Math.Abs(_carY - GetYForFloor(2)) < 12;
            DrawLanding(g, shaftX, ShaftTop, shaftW, FloorHeight,
                doorProgress: carAtFloor1 ? _doorProgress : 0f);

            // Floor G landing
            bool carAtFloorG = Math.Abs(_carY - GetYForFloor(1)) < 12;
            DrawLanding(g, shaftX, fgTop, shaftW, FloorHeight,
                doorProgress: carAtFloorG ? _doorProgress : 0f);

            using (var labelFont = new Font("Segoe UI", 7.5F, FontStyle.Bold))
            using (var labelBrush = new SolidBrush(Color.FromArgb(90, 92, 98)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("FIRST FLOOR", labelFont, labelBrush,
                    new RectangleF(shaftX, ShaftTop + FloorHeight + 4, shaftW, 16), sf);
                g.DrawString("GROUND FLOOR", labelFont, labelBrush,
                    new RectangleF(shaftX, fgTop + FloorHeight + 4, shaftW, 16), sf);
            }
        }

        /// <summary>
        /// Full landing: wall surround, thick brushed frame, sill, and centre-opening doors.
        /// </summary>
        private void DrawLanding(Graphics g, int x, int y, int w, int h, float doorProgress)
        {
            int ft = FrameThickness;
            var outer = new Rectangle(x, y, w, h);

            // Light stone / plaster wall behind the frame
            using (var wallBrush = new SolidBrush(Color.FromArgb(214, 212, 208)))
            {
                g.FillRectangle(wallBrush, outer);
            }

            // Soft wall mottling
            using (var mottPen = new Pen(Color.FromArgb(18, 180, 178, 174), 1))
            {
                for (int i = 0; i < 8; i++)
                {
                    int mx = x + 3 + (i * 17) % (w - 6);
                    int my = y + 4 + (i * 23) % (h - 8);
                    g.DrawLine(mottPen, mx, my, mx + 6, my + 2);
                }
            }

            // Thick brushed stainless architrave (header + sides + sill band)
            DrawBrushedMetalRect(g, new Rectangle(x, y, w, ft));                         // header
            DrawBrushedMetalRect(g, new Rectangle(x, y + h - ft, w, ft));                  // sill band
            DrawBrushedMetalRect(g, new Rectangle(x, y + ft, ft, h - 2 * ft));             // left jamb
            DrawBrushedMetalRect(g, new Rectangle(x + w - ft, y + ft, ft, h - 2 * ft));    // right jamb

            // Outer edge shadow / highlight for depth
            using (var darkPen = new Pen(Color.FromArgb(120, 118, 114), 1.5f))
            using (var lightPen = new Pen(Color.FromArgb(210, 208, 202), 1f))
            {
                g.DrawRectangle(darkPen, outer);
                g.DrawRectangle(lightPen, x + 1, y + 1, w - 3, h - 3);
            }

            // Door opening inside the frame
            var doorArea = new Rectangle(x + ft, y + ft, w - 2 * ft, h - 2 * ft);

            // Dark threshold strip at bottom of opening
            using (var sillBrush = new SolidBrush(Color.FromArgb(70, 72, 76)))
            {
                g.FillRectangle(sillBrush, doorArea.X, doorArea.Bottom - 5, doorArea.Width, 5);
            }

            // Interior cab (visible when doors slide open)
            using (var interiorBrush = new LinearGradientBrush(
                doorArea,
                Color.FromArgb(95, 96, 100),
                Color.FromArgb(55, 56, 60),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(interiorBrush, doorArea);
            }

            // Dim rear wall panel when open
            if (doorProgress > 0.1f)
            {
                int inset = 10;
                using (var rearBrush = new SolidBrush(Color.FromArgb(75, 76, 80)))
                {
                    g.FillRectangle(rearBrush,
                        doorArea.X + inset, doorArea.Y + inset,
                        doorArea.Width - 2 * inset, doorArea.Height - inset - 8);
                }
            }

            int doorH = doorArea.Height - 5; // leave sill visible
            int doorY = doorArea.Y;
            int halfW = doorArea.Width / 2;
            int maxSlide = (int)(halfW * 0.88f);
            int slide = (int)(maxSlide * doorProgress);
            int centerX = doorArea.X + halfW;

            var oldClip = g.Clip;
            g.SetClip(new Rectangle(doorArea.X, doorArea.Y, doorArea.Width, doorH));

            var leftDoor = new Rectangle(doorArea.X - slide, doorY, halfW, doorH);
            var rightDoor = new Rectangle(centerX + slide, doorY, halfW, doorH);

            DrawDoorLeaf(g, leftDoor);
            DrawDoorLeaf(g, rightDoor);

            // Centre seam when mostly closed
            if (doorProgress < 0.08f)
            {
                using var seamPen = new Pen(Color.FromArgb(95, 96, 100), 1.5f);
                g.DrawLine(seamPen, centerX, doorY, centerX, doorY + doorH);
            }

            g.Clip = oldClip;

            // Inner frame edge around door opening
            using (var innerPen = new Pen(Color.FromArgb(130, 128, 124), 1f))
            {
                g.DrawRectangle(innerPen, doorArea.X - 1, doorArea.Y - 1, doorArea.Width + 1, doorArea.Height);
            }
        }

        private void DrawDoorLeaf(Graphics g, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;

            // Soft vertical brushed gradient — brighter near centre, cooler at edges
            using (var brush = new LinearGradientBrush(
                rect,
                Color.FromArgb(198, 198, 200),
                Color.FromArgb(168, 168, 172),
                LinearGradientMode.Horizontal))
            {
                var blend = new ColorBlend(4);
                blend.Colors = new[]
                {
                    Color.FromArgb(155, 155, 158),
                    Color.FromArgb(205, 205, 208),
                    Color.FromArgb(188, 188, 192),
                    Color.FromArgb(150, 150, 154)
                };
                blend.Positions = new[] { 0f, 0.35f, 0.7f, 1f };
                brush.InterpolationColors = blend;
                g.FillRectangle(brush, rect);
            }

            // Fine vertical brushed grain
            using (var grainLight = new Pen(Color.FromArgb(28, 255, 255, 255), 1))
            using (var grainDark = new Pen(Color.FromArgb(18, 40, 40, 45), 1))
            {
                for (int gx = rect.Left + 2; gx < rect.Right - 1; gx += 3)
                {
                    g.DrawLine(grainLight, gx, rect.Top, gx, rect.Bottom);
                    if (gx % 6 == 0)
                        g.DrawLine(grainDark, gx + 1, rect.Top, gx + 1, rect.Bottom);
                }
            }

            // Leaf edge
            using (var edgePen = new Pen(Color.FromArgb(110, 110, 114), 1f))
            {
                g.DrawRectangle(edgePen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
            }
        }

        private void DrawBrushedMetalRect(Graphics g, Rectangle rect)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (var brush = new LinearGradientBrush(
                rect,
                Color.FromArgb(195, 195, 198),
                Color.FromArgb(155, 155, 160),
                LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, rect);
            }

            using (var grainPen = new Pen(Color.FromArgb(22, 255, 255, 255), 1))
            {
                for (int gx = rect.Left + 1; gx < rect.Right; gx += 2)
                {
                    g.DrawLine(grainPen, gx, rect.Top, gx, rect.Bottom);
                }
            }

            using (var edgePen = new Pen(Color.FromArgb(125, 124, 120), 1f))
            {
                g.DrawRectangle(edgePen, rect.X, rect.Y, Math.Max(0, rect.Width - 1), Math.Max(0, rect.Height - 1));
            }
        }

        private void DrawPillIndicator(Graphics g, int x, int y, int floor)
        {
            int w = 74;
            int h = 20;
            var rect = new Rectangle(x, y, w, h);

            using (var path = RoundedRect(rect, 4))
            {
                using (var bgBrush = new SolidBrush(Color.FromArgb(18, 18, 20)))
                    g.FillPath(bgBrush, path);
                using (var borderPen = new Pen(Color.FromArgb(80, 80, 84), 1.2f))
                    g.DrawPath(borderPen, path);

                bool isHereWithDoorsOpen = (_currentFloor == floor && _doorOpen);
                Color ledColor = isHereWithDoorsOpen
                    ? Color.FromArgb(40, 200, 55)
                    : Color.FromArgb(220, 40, 40);

                int ledR = 4;
                int ledX = x + 14;
                int ledY = y + h / 2;

                using (var bulbBrush = new SolidBrush(ledColor))
                    g.FillEllipse(bulbBrush, ledX - ledR, ledY - ledR, ledR * 2, ledR * 2);

                string floorTxt = _currentFloor == 2 ? "1" : "G";
                using (var font = new Font("Consolas", 9.5F, FontStyle.Bold))
                using (var textBrush = new SolidBrush(Color.FromArgb(230, 50, 50)))
                    g.DrawString(floorTxt, font, textBrush, x + 44, y + 2);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
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
