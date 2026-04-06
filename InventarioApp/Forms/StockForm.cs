using InventarioApp.Models;
using InventarioApp.Services;

namespace InventarioApp.Forms;

public class StockForm : Form
{
    private readonly ComboBox cmbCategoriasFiltro;
    private readonly TextBox txtBusqueda;
    private readonly DataGridView dgvStock;
    private readonly System.Windows.Forms.Timer searchTimer = new();

    private readonly CategoriaService categoriaService = new();
    private readonly StockService stockService = new();

    public StockForm()
    {
        Text = "Vertice Muisca - Stock";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1050, 700);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = UiTheme.Background;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        AutoScaleMode = AutoScaleMode.Dpi;

        Panel panelHeader = UiTheme.CreateGradientHeader(120);
        Panel badge = UiTheme.CreateLogoBadge(20, 33, 54);

        Label lblTitulo = new()
        {
            Text = "Consulta de Stock Disponible",
            Left = 92,
            Top = 24,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
            ForeColor = Color.White
        };
        UiTheme.StyleHeaderTitle(lblTitulo);

        Label lblSubtitulo = new()
        {
            Text = "Stock calculado desde los movimientos registrados en el sistema",
            Left = 94,
            Top = 68,
            AutoSize = true,
            ForeColor = Color.FromArgb(245, 255, 250)
        };
        UiTheme.StyleHeaderSubtitle(lblSubtitulo);

        panelHeader.Controls.Add(badge);
        panelHeader.Controls.Add(lblTitulo);
        panelHeader.Controls.Add(lblSubtitulo);

        Panel panelFormulario = new() { Dock = DockStyle.Top, Height = 200, MinimumSize = new Size(0, 190), Padding = new Padding(16), BackColor = UiTheme.SurfaceSoft };
        UiTheme.StyleCard(panelFormulario);

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));

        var gbFiltros = new GroupBox { Text = " Filtros y búsqueda ", Dock = DockStyle.Fill };
        UiTheme.StyleGroupBox(gbFiltros);
        var tblFiltros = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(0, 8, 0, 0)
        };
        tblFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        tblFiltros.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tblFiltros.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        tblFiltros.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

        var lblFiltroCategoria = new Label
        {
            Text = "Categoría",
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.TextMain
        };
        cmbCategoriasFiltro = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(0, 2, 0, 6)
        };
        UiTheme.StyleComboBox(cmbCategoriasFiltro);

        var lblBusqueda = new Label
        {
            Text = "Nombre o código",
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.TextMain
        };
        txtBusqueda = new TextBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            PlaceholderText = "Escribe para filtrar (tiempo real)…",
            Margin = new Padding(0, 2, 0, 0)
        };
        UiTheme.StyleTextBox(txtBusqueda);

        tblFiltros.Controls.Add(lblFiltroCategoria, 0, 0);
        tblFiltros.Controls.Add(cmbCategoriasFiltro, 1, 0);
        tblFiltros.Controls.Add(lblBusqueda, 0, 1);
        tblFiltros.Controls.Add(txtBusqueda, 1, 1);
        gbFiltros.Controls.Add(tblFiltros);

        var gbAcciones = new GroupBox { Text = " Acciones ", Dock = DockStyle.Fill };
        UiTheme.StyleGroupBox(gbAcciones);
        var tblAcciones = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            Padding = new Padding(4, 8, 4, 4)
        };
        tblAcciones.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        tblAcciones.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

        var btnActualizar = new Button { Text = "Actualizar lista", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
        UiTheme.StyleSecondaryButton(btnActualizar);
        btnActualizar.Click += (_, _) => CargarStock();

        var btnLimpiar = new Button { Text = "Limpiar filtros", Dock = DockStyle.Fill };
        UiTheme.StyleSecondaryButton(btnLimpiar);
        btnLimpiar.Click += (_, _) =>
        {
            txtBusqueda.Clear();
            cmbCategoriasFiltro.SelectedIndex = 0;
            CargarStock();
        };

        tblAcciones.Controls.Add(btnActualizar, 0, 0);
        tblAcciones.Controls.Add(btnLimpiar, 0, 1);
        gbAcciones.Controls.Add(tblAcciones);

        grid.Controls.Add(gbFiltros, 0, 0);
        grid.Controls.Add(gbAcciones, 1, 0);

        panelFormulario.Controls.Add(grid);

        Panel panelTabla = new() { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = UiTheme.SurfaceSoft };
        UiTheme.StyleCard(panelTabla);

        dgvStock = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false
        };
        UiTheme.StyleGrid(dgvStock);
        dgvStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvStock.ColumnHeadersHeight = 40;
        dgvStock.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        dgvStock.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "NombreProducto",
            HeaderText = "Producto",
            DataPropertyName = nameof(StockDisponible.NombreProducto),
            FillWeight = 36
        });
        dgvStock.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CodigoBarras",
            HeaderText = "Código",
            DataPropertyName = nameof(StockDisponible.CodigoBarras),
            FillWeight = 22
        });
        dgvStock.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "NombreCategoria",
            HeaderText = "Categoría",
            DataPropertyName = nameof(StockDisponible.NombreCategoria),
            FillWeight = 22
        });
        dgvStock.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CantidadDisponible",
            HeaderText = "Cantidad Disponible",
            DataPropertyName = nameof(StockDisponible.CantidadDisponible),
            FillWeight = 20
        });

        panelTabla.Controls.Add(dgvStock);

        // Orden correcto para Docking
        Controls.Add(panelTabla);
        Controls.Add(panelFormulario);
        Controls.Add(panelHeader);
        Padding = new Padding(20);

        CargarCategoriasFiltro();
        CargarStock();

        // Búsqueda en tiempo real
        searchTimer.Interval = 250;
        searchTimer.Tick += (_, _) =>
        {
            searchTimer.Stop();
            CargarStock();
        };
        txtBusqueda.TextChanged += (_, _) =>
        {
            searchTimer.Stop();
            searchTimer.Start();
        };
        cmbCategoriasFiltro.SelectedIndexChanged += (_, _) => CargarStock();
    }

    private void CargarCategoriasFiltro()
    {
        var categorias = categoriaService.ListarCategorias();
        categorias.Insert(0, new Categoria { IdCategoria = 0, NombreCategoria = "Todas" });

        cmbCategoriasFiltro.DataSource = categorias;
        cmbCategoriasFiltro.DisplayMember = nameof(Categoria.NombreCategoria);
        cmbCategoriasFiltro.ValueMember = nameof(Categoria.IdCategoria);
        cmbCategoriasFiltro.SelectedIndex = 0;
    }

    private void CargarStock()
    {
        try
        {
            int idCategoria = GetCategoriaFiltroId();
            int? filtroCategoria = idCategoria > 0 ? idCategoria : null;

            dgvStock.DataSource = stockService.ListarStockDisponible(filtroCategoria, txtBusqueda.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo cargar el stock: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private int GetCategoriaFiltroId()
    {
        object? value = cmbCategoriasFiltro.SelectedValue;
        if (value is int idInt)
        {
            return idInt;
        }

        if (value is Categoria cat)
        {
            return cat.IdCategoria;
        }

        if (value is string s && int.TryParse(s, out int idParsed))
        {
            return idParsed;
        }

        object? item = cmbCategoriasFiltro.SelectedItem;
        if (item is Categoria catItem)
        {
            return catItem.IdCategoria;
        }

        return 0;
    }
}

