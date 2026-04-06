namespace InventarioApp.Forms;

public class MainForm : Form
{
    private readonly Label lblTitulo;
    private readonly Label lblSubtitulo;
    private readonly Button btnCategorias;
    private readonly Button btnProductos;
    private readonly Button btnInventario;
    private readonly Button btnStock;

    public MainForm()
    {
        Text = "Vertice Muisca - Supermercado";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(800, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = UiTheme.Background;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);

        Panel panelHeader = UiTheme.CreateGradientHeader(130);

        Panel badge = UiTheme.CreateLogoBadge(24, 33, 64);

        lblTitulo = new Label
        {
            Text = "Vertice Muisca",
            Left = 105,
            Top = 26,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 23F, FontStyle.Bold),
            ForeColor = Color.White
        };
        UiTheme.StyleHeaderTitle(lblTitulo);

        lblSubtitulo = new Label
        {
            Text = "Sistema de gestion para supermercado",
            Left = 108,
            Top = 78,
            AutoSize = true,
            Font = new Font("Segoe UI", 11F, FontStyle.Regular),
            ForeColor = Color.FromArgb(245, 255, 250)
        };
        UiTheme.StyleHeaderSubtitle(lblSubtitulo);

        panelHeader.Controls.Add(badge);
        panelHeader.Controls.Add(lblTitulo);
        panelHeader.Controls.Add(lblSubtitulo);

        Panel panelAcciones = new()
        {
            Left = 30,
            Top = 148,
            Width = 740,
            Height = 200,
            BackColor = UiTheme.SurfaceSoft
        };
        UiTheme.StyleCard(panelAcciones);

        btnCategorias = new Button
        {
            Text = "Gestionar Categorias",
            Width = 350,
            Height = 62,
            Left = 40,
            Top = 30
        };
        UiTheme.StylePrimaryButton(btnCategorias);
        btnCategorias.Click += (_, _) =>
        {
            using CategoriaForm form = new();
            form.ShowDialog();
        };

        btnProductos = new Button
        {
            Text = "Gestionar Productos",
            Width = 350,
            Height = 62,
            Left = 400,
            Top = 30
        };
        UiTheme.StyleSecondaryButton(btnProductos);
        btnProductos.Click += (_, _) =>
        {
            using ProductoForm form = new();
            form.ShowDialog();
        };

        btnInventario = new Button
        {
            Text = "Gestionar Inventario",
            Width = 350,
            Height = 62,
            Left = 40,
            Top = 110
        };
        UiTheme.StylePrimaryButton(btnInventario);
        btnInventario.Click += (_, _) =>
        {
            using InventarioForm form = new();
            form.ShowDialog();
        };

        btnStock = new Button
        {
            Text = "Consulta de Stock",
            Width = 350,
            Height = 62,
            Left = 400,
            Top = 110
        };
        UiTheme.StyleSecondaryButton(btnStock);
        btnStock.Click += (_, _) =>
        {
            using StockForm form = new();
            form.ShowDialog();
        };

        panelAcciones.Controls.Add(btnCategorias);
        panelAcciones.Controls.Add(btnProductos);
        panelAcciones.Controls.Add(btnInventario);
        panelAcciones.Controls.Add(btnStock);

        Controls.Add(panelHeader);
        Controls.Add(panelAcciones);
    }
}
