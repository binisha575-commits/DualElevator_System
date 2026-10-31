using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lift.Models;
using Lift.Database;

namespace Lift.UI
{
    public class LogForm : Form
    {
        private ListView _listView = null!;
        private Button _btnRefresh = null!;
        private Button _btnClear = null!;
        private Button _btnClose = null!;

        public LogForm(List<LogEntry> logs)
        {
            InitializeComponent();
            LoadLogs(logs);
        }

        private void InitializeComponent()
        {
            this.Text = "Elevator Operation Log";
            this.Size = new Size(720, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(245, 245, 248);
            this.Font = new Font("Segoe UI", 9F);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            _listView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(15, 15),
                Size = new Size(675, 360),
                Font = new Font("Consolas", 9F)
            };
            _listView.Columns.Add("Time", 150);
            _listView.Columns.Add("Elevator", 80);
            _listView.Columns.Add("Action", 320);
            _listView.Columns.Add("Floor", 60);
            this.Controls.Add(_listView);

            _btnRefresh = new Button
            {
                Text = "Refresh",
                Size = new Size(90, 32),
                Location = new Point(15, 390),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnRefresh.Click += (s, e) => LoadLogs(DatabaseHelper.GetAllLogs());
            this.Controls.Add(_btnRefresh);

            _btnClear = new Button
            {
                Text = "Clear Log",
                Size = new Size(90, 32),
                Location = new Point(120, 390),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(180, 70, 70),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnClear.Click += (s, e) =>
            {
                if (MessageBox.Show("Clear all log entries?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    DatabaseHelper.ClearLogs();
                    LoadLogs(new List<LogEntry>());
                }
            };
            this.Controls.Add(_btnClear);

            _btnClose = new Button
            {
                Text = "Close",
                Size = new Size(90, 32),
                Location = new Point(600, 390),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(100, 100, 110),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            _btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(_btnClose);
        }

        private void LoadLogs(List<LogEntry> logs)
        {
            _listView.Items.Clear();
            foreach (var log in logs)
            {
                var item = new ListViewItem(log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
                item.SubItems.Add(log.ElevatorId.ToString());
                item.SubItems.Add(log.Action);
                string floorLabel = log.Floor == 2 ? "1" : (log.Floor == 1 ? "G" : log.Floor.ToString());
                item.SubItems.Add(floorLabel);
                _listView.Items.Add(item);
            }
        }
    }
}
