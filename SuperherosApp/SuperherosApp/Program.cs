using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;

/*
 Members:
 - Jordann Heunis: 603115
 - Miles Pieterse: 602327
 - Emmanuel Teodor Booysen Joao: 601270
 - Riekus Grobler: 601246
 - Group V_2PM
 */


namespace SuperheroesApp
{
    static class Program
    {
        // File paths for storing hero data and summary
        static readonly string HeroesFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "superheroes.txt");
        static readonly string SummaryFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "summary.txt");

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        public class MainForm : Form
        {
            // Input fields
            readonly TextBox txtId = new TextBox();
            readonly TextBox txtName = new TextBox();
            readonly TextBox txtAge = new TextBox();
            readonly TextBox txtPower = new TextBox();
            readonly TextBox txtScore = new TextBox();

            // Action buttons
            readonly Button btnAdd = new Button();
            readonly Button btnUpdate = new Button();
            readonly Button btnDelete = new Button();
            readonly Button btnLoad = new Button();
            readonly Button btnReport = new Button();
            readonly Button btnClear = new Button();
            readonly Button btnExit = new Button();

            // Display components
            readonly DataGridView dgv = new DataGridView();
            readonly Label lblStatus = new Label();
            readonly Panel sidebar = new Panel();
            readonly Panel header = new Panel();
            readonly ChartPanel chartPanel = new ChartPanel();
            readonly DonutControl donut = new DonutControl();

            // Theme colors
            readonly Color bg = Color.FromArgb(37, 43, 50);
            readonly Color sidebarBg = Color.FromArgb(30, 34, 38);
            readonly Color accent = Color.FromArgb(193, 203, 53);
            readonly Color accent2 = Color.FromArgb(98, 183, 174);
            readonly Color textLight = Color.FromArgb(220, 224, 228);

            public MainForm()
            {
                Text = "One Kick Heroes Academy Manager";
                Width = 1200;
                Height = 700;
                StartPosition = FormStartPosition.CenterScreen;
                BackColor = bg;
                ForeColor = textLight;
                Font = new Font("Segoe UI", 9f);

                InitializeLayout();      // Build UI
                EnsureFilesExist();      // Make sure data files exist
                LoadHeroesToGrid();      // Load data into grid
            }

            // Set up sidebar, inputs, buttons, charts, and DataGridView
            void InitializeLayout()
            {
                // Sidebar panel
                sidebar.Left = 20;
                sidebar.Top = 20;
                sidebar.Width = 240;
                sidebar.Height = ClientSize.Height - 40;
                sidebar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
                sidebar.BackColor = sidebarBg;
                Controls.Add(sidebar);

                // Sidebar logo
                var logoStrip = new Panel { Left = 0, Top = 0, Width = sidebar.Width, Height = 68, BackColor = accent };
                var logoLabel = new Label
                {
                    Left = 12,
                    Top = 6,
                    Width = logoStrip.Width - 24,
                    Height = 56,
                    Text = "m",
                    ForeColor = sidebarBg,
                    Font = new Font("Segoe Script", 28f, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                logoStrip.Controls.Add(logoLabel);
                sidebar.Controls.Add(logoStrip);

                // Input fields panel
                var inputsPanel = new Panel { Left = 8, Top = 90, Width = sidebar.Width - 16, Height = 340, BackColor = Color.FromArgb(36, 42, 50) };
                inputsPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                sidebar.Controls.Add(inputsPanel);

                int y = 15;
                AddInputField(inputsPanel, "Hero ID:", txtId, ref y);
                y += 8;
                AddInputField(inputsPanel, "Name:", txtName, ref y);
                y += 8;
                AddInputField(inputsPanel, "Age:", txtAge, ref y);
                y += 8;
                AddInputField(inputsPanel, "Quirk:", txtPower, ref y);
                y += 8;
                AddInputField(inputsPanel, "Exam Score (0%-100%):", txtScore, ref y);

                // Buttons layout
                btnAdd.Text = "Add"; btnUpdate.Text = "Update"; btnDelete.Text = "Delete";
                btnAdd.Width = btnUpdate.Width = btnDelete.Width = 70;
                btnAdd.Height = btnUpdate.Height = btnDelete.Height = 32;

                btnAdd.Left = inputsPanel.Left;
                btnAdd.Top = inputsPanel.Bottom + 12;
                btnUpdate.Left = btnAdd.Right + 8; btnUpdate.Top = btnAdd.Top;
                btnDelete.Left = btnUpdate.Right + 8; btnDelete.Top = btnAdd.Top;

                StyleButton(btnAdd, accent, sidebarBg);
                StyleButton(btnUpdate, accent2, sidebarBg);
                StyleButton(btnDelete, Color.FromArgb(200, 80, 80), Color.White);

                sidebar.Controls.Add(btnAdd); sidebar.Controls.Add(btnUpdate); sidebar.Controls.Add(btnDelete);

                // Assign button events
                btnAdd.Click += BtnAdd_Click;
                btnUpdate.Click += BtnUpdate_Click;
                btnDelete.Click += BtnDelete_Click;

                // Header panel
                header.Left = sidebar.Right + 18;
                header.Top = 20;
                header.Width = ClientSize.Width - sidebar.Right - 38;
                header.Height = 80;
                header.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                header.BackColor = Color.FromArgb(30, 34, 38);
                Controls.Add(header);

                var title = new Label
                {
                    Left = 18,
                    Top = 28,
                    Width = 400,
                    Height = 26,
                    Text = "One Kick Hero Academy Analytics",
                    ForeColor = textLight,
                    Font = new Font("Segoe UI", 14f, FontStyle.Regular)
                };
                header.Controls.Add(title);

                // Wave chart panel
                chartPanel.Left = header.Left;
                chartPanel.Top = header.Bottom + 12;
                chartPanel.Width = (int)(header.Width * 0.52);
                chartPanel.Height = 160;
                chartPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                chartPanel.BackColor = Color.FromArgb(28, 32, 36);
                Controls.Add(chartPanel);

                // Donut chart
                donut.Left = chartPanel.Left;
                donut.Top = chartPanel.Bottom + 12;
                donut.Width = chartPanel.Width;
                donut.Height = 220;
                donut.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                donut.Percentage = 0;
                donut.ForeColor = textLight;
                donut.Accent = accent;
                donut.SecondAccent = accent2;
                Controls.Add(donut);

                // DataGridView for hero records
                dgv.Left = chartPanel.Right + 12;
                dgv.Top = header.Bottom + 12;
                dgv.Width = ClientSize.Width - dgv.Left - 20;
                dgv.Height = ClientSize.Height - dgv.Top - 40;
                dgv.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Left;
                dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgv.MultiSelect = false;
                dgv.ReadOnly = true;
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgv.CellClick += Dgv_CellClick;
                StyleDataGridViewForDarkTheme(dgv);
                Controls.Add(dgv);

                // Status label
                lblStatus.Left = sidebar.Left;
                lblStatus.Top = sidebar.Bottom + 4;
                lblStatus.Width = 600;
                lblStatus.Height = 22;
                lblStatus.Text = "Status: Ready";
                lblStatus.ForeColor = Color.FromArgb(180, 180, 180);
                lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
                Controls.Add(lblStatus);

                // Additional buttons below charts
                btnLoad.Text = "Load All"; btnReport.Text = "Generate Report";
                btnClear.Text = "Clear"; btnExit.Text = "Exit";
                btnLoad.Width = 90; btnReport.Width = 120; btnClear.Width = 70; btnExit.Width = 70;
                btnLoad.Height = btnReport.Height = btnClear.Height = btnExit.Height = 32;

                btnLoad.Left = donut.Left; btnLoad.Top = donut.Bottom + 18;
                btnReport.Left = btnLoad.Right + 8; btnReport.Top = btnLoad.Top;
                btnClear.Left = btnReport.Right + 8; btnClear.Top = btnLoad.Top;
                btnExit.Left = btnClear.Right + 8; btnExit.Top = btnLoad.Top;

                StyleButton(btnLoad, Color.FromArgb(45, 50, 56), textLight);
                StyleButton(btnReport, accent2, sidebarBg);
                StyleButton(btnClear, Color.FromArgb(60, 65, 70), textLight);
                StyleButton(btnExit, Color.FromArgb(90, 40, 40), Color.White);

                Controls.Add(btnLoad); Controls.Add(btnReport); Controls.Add(btnClear); Controls.Add(btnExit);

                // Assign events
                btnLoad.Click += (s, e) => LoadHeroesToGrid();
                btnReport.Click += BtnReport_Click;
                btnClear.Click += (s, e) => ClearForm();
                btnExit.Click += (s, e) =>
                {
                    var confirm = MessageBox.Show("Are you sure?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirm == DialogResult.Yes) this.Close();
                    else lblStatus.Text = "Exit cancelled.";
                };
            }

            // Add a labeled TextBox to a panel
            void AddInputField(Panel parent, string label, TextBox textbox, ref int y)
            {
                var lbl = new Label { Left = 10, Top = y, Width = parent.Width - 20, Text = label, ForeColor = Color.FromArgb(170, 170, 170), Font = new Font("Segoe UI", 8.5f) };
                parent.Controls.Add(lbl);
                y += 20;
                textbox.Left = 10; textbox.Top = y; textbox.Width = parent.Width - 20; textbox.Height = 24;
                textbox.BackColor = Color.FromArgb(28, 33, 38);
                textbox.ForeColor = textLight;
                textbox.BorderStyle = BorderStyle.FixedSingle;
                parent.Controls.Add(textbox);
                y += 32;
            }

            // Style buttons for dark theme
            void StyleButton(Button btn, Color backColor, Color foreColor)
            {
                btn.BackColor = backColor;
                btn.ForeColor = foreColor;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Font = new Font("Segoe UI", 9f);
            }

            // Style DataGridView for dark theme
            void StyleDataGridViewForDarkTheme(DataGridView grid)
            {
                grid.EnableHeadersVisualStyles = false;
                grid.BackgroundColor = Color.FromArgb(28, 32, 36);
                grid.BorderStyle = BorderStyle.None;
                grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
                grid.GridColor = Color.FromArgb(45, 50, 55);
                grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(22, 28, 32);
                grid.ColumnHeadersDefaultCellStyle.ForeColor = textLight;
                grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                grid.DefaultCellStyle.BackColor = Color.FromArgb(30, 34, 38);
                grid.DefaultCellStyle.ForeColor = textLight;
                grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 100, 120);
                grid.DefaultCellStyle.SelectionForeColor = Color.White;
                grid.RowHeadersVisible = false;
            }

            // Ensure hero and summary files exist
            void EnsureFilesExist()
            {
                try
                {
                    if (!File.Exists(HeroesFile)) File.WriteAllText(HeroesFile, "");
                    if (!File.Exists(SummaryFile)) File.WriteAllText(SummaryFile, "");
                }
                catch (Exception ex)
                {
                    lblStatus.Text = "File init error: " + ex.Message;
                }
            }

            // Sanitize input
            string Sanitize(string s) => (s ?? "").Replace(",", ";").Trim();

            // Read heroes from file
            List<string[]> ReadAllHeroes()
            {
                var list = new List<string[]>();
                try
                {
                    foreach (var line in File.ReadAllLines(HeroesFile))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var parts = line.Split(',').Select(p => p.Trim()).ToArray();
                        if (parts.Length >= 7) list.Add(parts);
                    }
                }
                catch (Exception ex) { lblStatus.Text = "Read error: " + ex.Message; }
                return list;
            }

            // Write heroes to file
            void WriteAllHeroes(List<string[]> rows)
            {
                try
                {
                    string temp = HeroesFile + ".tmp";
                    var lines = rows.Select(r => string.Join(",", r));
                    File.WriteAllLines(temp, lines);
                    File.Copy(temp, HeroesFile, true);
                    File.Delete(temp);
                }
                catch (Exception ex) { lblStatus.Text = "Write error: " + ex.Message; }
            }

            // Determine hero rank and threat based on score
            Tuple<string, string> GetRankAndThreat(int score)
            {
                if (score >= 81 && score <= 100) return Tuple.Create("S-Rank", "Finals Week");
                if (score >= 61) return Tuple.Create("A-Rank", "Midterm Madness");
                if (score >= 41) return Tuple.Create("B-Rank", "Group Project Gone Wrong");
                return Tuple.Create("C-Rank", "Pop Quiz");
            }

            // Load heroes into DataGridView
            void LoadHeroesToGrid()
            {
                var rows = ReadAllHeroes();
                var dt = new DataTable();
                dt.Columns.Add("HeroID");
                dt.Columns.Add("Name");
                dt.Columns.Add("Age");
                dt.Columns.Add("Superpower");
                dt.Columns.Add("Score");
                dt.Columns.Add("Rank");
                dt.Columns.Add("Threat");
                foreach (var r in rows) dt.Rows.Add(r);
                dgv.DataSource = dt;
                lblStatus.Text = $"Loaded {rows.Count} records.";
                UpdateDonutChart();
            }

            // Update donut chart based on average score
            void UpdateDonutChart()
            {
                var rows = ReadAllHeroes();
                donut.Percentage = rows.Count == 0 ? 0 : (int)rows.Average(r => int.Parse(r[4]));
                donut.Invalidate();
            }

            // Add hero record
            void BtnAdd_Click(object sender, EventArgs e)
            {
                string id = Sanitize(txtId.Text);
                string name = Sanitize(txtName.Text);
                string power = Sanitize(txtPower.Text);
                if (!int.TryParse(txtAge.Text.Trim(), out int age) || !int.TryParse(txtScore.Text.Trim(), out int score))
                {
                    lblStatus.Text = "Age and Score must be numbers.";
                    return;
                }
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(power))
                { lblStatus.Text = "Please fill all fields."; return; }
                if (age < 1 || age > 150 || score < 0 || score > 100) { lblStatus.Text = "Invalid age or score."; return; }

                var rows = ReadAllHeroes();
                if (rows.Any(r => string.Equals(r[0], id, StringComparison.OrdinalIgnoreCase))) { lblStatus.Text = "HeroID exists."; return; }

                var rankAndThreat = GetRankAndThreat(score);
                rows.Add(new string[] { id, name, age.ToString(), power, score.ToString(), rankAndThreat.Item1, rankAndThreat.Item2 });
                WriteAllHeroes(rows);
                LoadHeroesToGrid();
                lblStatus.Text = "Hero added successfully.";
                ClearForm();
            }

            // Fill form fields when a row is selected
            void Dgv_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                if (e.RowIndex < 0) return;
                var row = dgv.Rows[e.RowIndex];
                txtId.Text = row.Cells["HeroID"].Value?.ToString() ?? "";
                txtName.Text = row.Cells["Name"].Value?.ToString() ?? "";
                txtAge.Text = row.Cells["Age"].Value?.ToString() ?? "";
                txtPower.Text = row.Cells["Superpower"].Value?.ToString() ?? "";
                txtScore.Text = row.Cells["Score"].Value?.ToString() ?? "";
                txtId.Enabled = false;
            }

            // Update hero record
            void BtnUpdate_Click(object sender, EventArgs e)
            {
                string id = Sanitize(txtId.Text);
                if (string.IsNullOrWhiteSpace(id)) { lblStatus.Text = "Select a record to update."; return; }
                if (!int.TryParse(txtAge.Text.Trim(), out int age) || !int.TryParse(txtScore.Text.Trim(), out int score)) { lblStatus.Text = "Age and Score must be numbers."; return; }
                if (age < 1 || age > 150 || score < 0 || score > 100) { lblStatus.Text = "Invalid age or score."; return; }

                var rows = ReadAllHeroes();
                var existing = rows.FirstOrDefault(r => string.Equals(r[0], id, StringComparison.OrdinalIgnoreCase));
                if (existing == null) { lblStatus.Text = "Record not found."; return; }

                existing[1] = Sanitize(txtName.Text);
                existing[2] = age.ToString();
                existing[3] = Sanitize(txtPower.Text);
                existing[4] = score.ToString();
                var rankAndThreat = GetRankAndThreat(score);
                existing[5] = rankAndThreat.Item1; existing[6] = rankAndThreat.Item2;

                WriteAllHeroes(rows);
                LoadHeroesToGrid();
                lblStatus.Text = "Record updated.";
                txtId.Enabled = true;
                ClearForm();
            }

            // Delete hero record
            void BtnDelete_Click(object sender, EventArgs e)
            {
                if (dgv.SelectedRows.Count == 0) { lblStatus.Text = "Select a row to delete."; return; }
                var id = dgv.SelectedRows[0].Cells["HeroID"].Value?.ToString();
                if (string.IsNullOrWhiteSpace(id)) { lblStatus.Text = "No ID found."; return; }

                if (MessageBox.Show($"Delete record {id}?", "Confirm Delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
                { lblStatus.Text = "Delete cancelled."; return; }

                var rows = ReadAllHeroes();
                rows.RemoveAll(r => string.Equals(r[0], id, StringComparison.OrdinalIgnoreCase));
                WriteAllHeroes(rows);
                LoadHeroesToGrid();
                lblStatus.Text = "Record deleted.";
                ClearForm();
            }

            // Generate summary report
            void BtnReport_Click(object sender, EventArgs e)
            {
                var rows = ReadAllHeroes();
                int total = rows.Count;
                double avgAge = total > 0 ? rows.Average(r => int.Parse(r[2])) : 0;
                double avgScore = total > 0 ? rows.Average(r => int.Parse(r[4])) : 0;
                int s = rows.Count(r => r[5] == "S-Rank");
                int a = rows.Count(r => r[5] == "A-Rank");
                int b = rows.Count(r => r[5] == "B-Rank");
                int c = rows.Count(r => r[5] == "C-Rank");

                var lines = new List<string>
                {
                    $"Total Heroes: {total}",
                    $"Average Age: {avgAge:F1}",
                    $"Average Score: {avgScore:F1}",
                    $"S-Rank: {s}",
                    $"A-Rank: {a}",
                    $"B-Rank: {b}",
                    $"C-Rank: {c}"
                };

                try
                {
                    File.WriteAllLines(SummaryFile, lines);
                    lblStatus.Text = "Summary generated and saved to summary.txt";
                }
                catch (Exception ex) { lblStatus.Text = "Summary write error: " + ex.Message; }

                MessageBox.Show(string.Join(Environment.NewLine, lines), "Summary");
            }

            // Reset form inputs
            void ClearForm()
            {
                txtId.Enabled = true;
                txtId.Text = "";
                txtName.Text = "";
                txtAge.Text = "";
                txtPower.Text = "";
                txtScore.Text = "";
            }
        }

        // Panel with animated wave chart
        class ChartPanel : Panel
        {
            private Timer animTimer;
            private float animOffset = 0f;

            public ChartPanel()
            {
                this.DoubleBuffered = true;
                SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

                animTimer = new Timer();
                animTimer.Interval = 30;
                animTimer.Tick += (s, e) =>
                {
                    animOffset += 0.02f;
                    if (animOffset > 6.28f) animOffset = 0f;
                    Invalidate();
                };
                animTimer.Start();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = ClientRectangle;

                using (var b = new LinearGradientBrush(rect, Color.FromArgb(32, 37, 41), Color.FromArgb(26, 30, 34), 90f))
                    g.FillRectangle(b, rect);

                DrawWave(g, rect, 0.55f, Color.FromArgb(70, 140, 190), 0.22f, animOffset);
                DrawWave(g, rect, 0.35f, Color.FromArgb(80, 200, 150), 0.20f, animOffset + 1.0f);
                DrawWave(g, rect, 0.15f, Color.FromArgb(200, 210, 90), 0.18f, animOffset + 2.0f);
            }

            void DrawWave(Graphics g, Rectangle rect, float heightFactor, Color color, float opacity, float offset)
            {
                var w = rect.Width; var h = rect.Height;
                var path = new GraphicsPath();
                var points = new List<PointF>();
                int segments = 100;

                for (int i = 0; i <= segments; i++)
                {
                    float x = (float)i / segments * w;
                    float wave1 = (float)Math.Sin((i / (float)segments) * Math.PI * 4 + offset) * 15;
                    float wave2 = (float)Math.Sin((i / (float)segments) * Math.PI * 2 + offset * 1.5f) * 10;
                    float y = h * (1f - heightFactor) + wave1 + wave2;
                    points.Add(new PointF(x, y));
                }

                path.StartFigure();
                if (points.Count > 1) path.AddLines(points.ToArray());
                path.AddLine(w, h, 0, h);
                path.CloseFigure();

                using (var b = new SolidBrush(Color.FromArgb((int)(opacity * 255), color)))
                    g.FillPath(b, path);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing && animTimer != null)
                {
                    animTimer.Stop();
                    animTimer.Dispose();
                }
                base.Dispose(disposing);
            }
        }

        // Donut chart showing average score and rank distribution
        class DonutControl : Control
        {
            public int Percentage { get; set; }
            public Color Accent { get; set; }
            public Color SecondAccent { get; set; }

            public DonutControl()
            {
                Percentage = 0;
                Accent = Color.Lime;
                SecondAccent = Color.Teal;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = ClientRectangle;
                g.Clear(Color.FromArgb(32, 36, 40));

                int size = Math.Min(rect.Width, rect.Height) - 60;
                var center = new Point(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
                var r = new Rectangle(center.X - size / 2, center.Y - size / 2, size, size);

                using (var bg = new Pen(Color.FromArgb(60, 60, 60), 18)) g.DrawEllipse(bg, r);
                using (var p = new Pen(Accent, 18))
                    g.DrawArc(p, r, -90, Math.Max(0, Math.Min(100, Percentage)) / 100f * 360f);

                using (var p2 = new Pen(SecondAccent, 8))
                    g.DrawArc(p2, new Rectangle(r.Left + 10, r.Top + 10, r.Width - 20, r.Height - 20), -90, Math.Max(0, Math.Min(100, Percentage)) / 100f * 360f);

                using (var f = new Font("Segoe UI", 18f, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.FromArgb(230, 230, 230)))
                {
                    var txt = Percentage + "%";
                    var sz = g.MeasureString(txt, f);
                    g.DrawString(txt, f, brush, center.X - sz.Width / 2, center.Y - sz.Height / 2);
                }

                using (var f = new Font("Segoe UI", 9f))
                using (var brush = new SolidBrush(Color.FromArgb(200, 200, 200)))
                {
                    g.DrawString("S-Rank Heroes", f, brush, rect.Left + 10, rect.Top + 10);
                    using (var b = new SolidBrush(Accent)) g.FillEllipse(b, rect.Right - 130, rect.Top + 12, 10, 10);

                    g.DrawString("A-Rank Heroes", f, brush, rect.Left + 10, rect.Top + 30);
                    using (var b = new SolidBrush(SecondAccent)) g.FillEllipse(b, rect.Right - 130, rect.Top + 32, 10, 10);

                    g.DrawString("Other Ranks", f, brush, rect.Left + 10, rect.Top + 50);
                    using (var b = new SolidBrush(Color.FromArgb(130, 130, 130))) g.FillEllipse(b, rect.Right - 130, rect.Top + 52, 10, 10);

                    g.DrawString("Avg Score", f, brush, rect.Left + 10, rect.Bottom - 30);
                }
            }
        }
    }
}
