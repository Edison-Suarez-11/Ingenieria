using System.Drawing.Drawing2D;

namespace InventarioApp.Forms;

public static class UiTheme
{
    // Paleta basada en el logo (verde/teal + acento morado)
    public static readonly Color Background = Color.FromArgb(242, 251, 246);
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceSoft = Color.FromArgb(249, 255, 252);
    public static readonly Color Primary = Color.FromArgb(0, 179, 107);      // verde
    public static readonly Color PrimaryDark = Color.FromArgb(0, 152, 91);   // verde oscuro
    public static readonly Color Accent = Color.FromArgb(124, 92, 255);      // morado
    public static readonly Color AccentDark = Color.FromArgb(95, 67, 234);   // morado oscuro
    public static readonly Color Teal = Color.FromArgb(20, 184, 166);        // teal
    public static readonly Color TextMain = Color.FromArgb(34, 40, 49);
    public static readonly Color TextMuted = Color.FromArgb(108, 117, 125);
    public static readonly Color GridHeader = Color.FromArgb(226, 247, 238);
    public static readonly Color BorderSoft = Color.FromArgb(230, 243, 236);

    public static void StylePrimaryButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = Primary;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.Padding = new Padding(8, 0, 8, 0);
        button.MouseEnter += (_, _) => button.BackColor = PrimaryDark;
        button.MouseLeave += (_, _) => button.BackColor = Primary;
        MakeRounded(button, 12);
    }

    public static void StyleSecondaryButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = Accent;
        button.FlatAppearance.BorderSize = 1;
        button.BackColor = Color.White;
        button.ForeColor = AccentDark;
        button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
        button.Padding = new Padding(8, 0, 8, 0);
        button.MouseEnter += (_, _) =>
        {
            button.BackColor = Color.FromArgb(241, 238, 255);
            button.FlatAppearance.BorderColor = AccentDark;
            button.ForeColor = AccentDark;
        };
        button.MouseLeave += (_, _) =>
        {
            button.BackColor = Color.White;
            button.FlatAppearance.BorderColor = Accent;
            button.ForeColor = AccentDark;
        };
        MakeRounded(button, 12);
    }

    public static void StyleTextBox(TextBox textBox)
    {
        textBox.BackColor = Color.White;
        textBox.ForeColor = TextMain;
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
    }

    public static void StyleComboBox(ComboBox comboBox)
    {
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.BackColor = Color.White;
        comboBox.ForeColor = TextMain;
        comboBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
    }

    public static void StyleCard(Panel panel)
    {
        panel.BackColor = Surface;
        panel.Padding = new Padding(10);
        panel.Paint += (_, e) =>
        {
            using Pen pen = new(BorderSoft, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
        };
        MakeRounded(panel, 12);
    }

    /// <summary>
    /// Agrupa campos con título claro (sin “textos flotando”).
    /// </summary>
    public static void StyleGroupBox(GroupBox groupBox)
    {
        groupBox.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        groupBox.ForeColor = TextMain;
        groupBox.BackColor = Surface;
        groupBox.Padding = new Padding(14, 8, 14, 12);
    }

    public static Panel CreateGradientHeader(int height)
    {
        Panel panel = new()
        {
            Dock = DockStyle.Top,
            Height = height
        };

        panel.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle r = panel.ClientRectangle;
            using LinearGradientBrush brush = new(r, Accent, Primary, 0f);
            e.Graphics.FillRectangle(brush, r);
        };

        return panel;
    }

    public static Panel CreateLogoBadge(int left, int top, int size)
    {
        Panel badge = new()
        {
            Left = left,
            Top = top,
            Width = size,
            Height = size,
            BackColor = Color.FromArgb(245, 255, 255, 255)
        };
        MakeRounded(badge, Math.Max(12, size / 5));

        PictureBox pic = new()
        {
            Left = 6,
            Top = 6,
            Width = size - 12,
            Height = size - 12,
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent
        };

        Image? logo = TryLoadBrandLogo(size - 12);
        if (logo is not null)
        {
            pic.Image = logo;
        }

        badge.Controls.Add(pic);
        badge.Paint += (_, e) =>
        {
            using Pen pen = new(Color.FromArgb(120, 255, 255, 255), 1);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawRectangle(pen, 0, 0, badge.Width - 1, badge.Height - 1);
        };

        return badge;
    }

    public static void StyleHeaderTitle(Label label)
    {
        label.BackColor = Color.Transparent;
        label.ForeColor = Color.White;
    }

    public static void StyleHeaderSubtitle(Label label)
    {
        label.BackColor = Color.Transparent;
        label.ForeColor = Color.FromArgb(235, 255, 245);
    }

    public static Image? TryLoadBrandLogo(int sizePx)
    {
        try
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "logo-empresa.png");
            if (!File.Exists(path))
            {
                return null;
            }

            using Image img = Image.FromFile(path);
            return new Bitmap(img, new Size(sizePx, sizePx));
        }
        catch
        {
            return null;
        }
    }

    public static void StyleGrid(DataGridView grid)
    {
        grid.BackgroundColor = Surface;
        grid.BorderStyle = BorderStyle.None;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = GridHeader;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextMain;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
        grid.ColumnHeadersHeight = 36;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 248, 233);
        grid.DefaultCellStyle.SelectionForeColor = TextMain;
        grid.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        grid.RowHeadersVisible = false;
        grid.GridColor = BorderSoft;
        grid.RowTemplate.Height = 30;
    }

    private static void MakeRounded(Control control, int radius)
    {
        void ApplyRegion()
        {
            if (control.Width <= 0 || control.Height <= 0)
            {
                return;
            }

            using GraphicsPath path = new();
            int diameter = radius * 2;
            Rectangle rect = new(0, 0, control.Width, control.Height);

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            control.Region = new Region(path);
        }

        control.SizeChanged += (_, _) => ApplyRegion();
        ApplyRegion();
    }
}
