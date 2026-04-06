using System.Globalization;
using System.Linq;
using InventarioApp.Models;
using InventarioApp.Services;
using InventarioApp.Utils;

namespace InventarioApp.Forms;

public class InventarioForm : Form
{
    private readonly InventarioService inventarioService = new();
    private readonly ProductoService productoService = new();
    private readonly CategoriaService categoriaService = new();

    private readonly ComboBox cmbCategoriasFiltro;
    private readonly TextBox txtBusquedaProducto;
    private readonly ComboBox cmbProductos;
    private readonly TextBox txtCantidad;
    private readonly DateTimePicker dtpFecha;
    private readonly Label lblStockActualValor;

    private readonly TextBox txtBusquedaMovimientos;
    private readonly DataGridView dgvMovimientos;

    private readonly ErrorProvider errorProvider = new();
    private readonly System.Windows.Forms.Timer productSearchTimer = new();
    private readonly System.Windows.Forms.Timer movementSearchTimer = new();

    public InventarioForm()
    {
        Text = "Vertice Muisca - Inventario";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1180, 760);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = UiTheme.Background;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        AutoScaleMode = AutoScaleMode.Dpi;

        Panel panelHeader = UiTheme.CreateGradientHeader(120);
        Panel badge = UiTheme.CreateLogoBadge(20, 33, 54);

        var lblTitulo = new Label
        {
            Text = "Gestión de Inventario",
            Left = 92,
            Top = 24,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
            ForeColor = Color.White
        };
        UiTheme.StyleHeaderTitle(lblTitulo);

        var lblSubtitulo = new Label
        {
            Text = "Registra inventario inicial y entradas; filtra y busca movimientos en tiempo real",
            Left = 94,
            Top = 68,
            AutoSize = true,
            ForeColor = Color.FromArgb(245, 255, 250)
        };
        UiTheme.StyleHeaderSubtitle(lblSubtitulo);

        panelHeader.Controls.Add(badge);
        panelHeader.Controls.Add(lblTitulo);
        panelHeader.Controls.Add(lblSubtitulo);

        Panel panelFormulario = new() { Dock = DockStyle.Top, Height = 340, MinimumSize = new Size(0, 320), Padding = new Padding(16), BackColor = UiTheme.SurfaceSoft };
        UiTheme.StyleCard(panelFormulario);

        Panel panelTabla = new() { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = UiTheme.SurfaceSoft };
        UiTheme.StyleCard(panelTabla);

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 292));

        var gbSel = new GroupBox { Text = " Selección de producto ", Dock = DockStyle.Fill };
        UiTheme.StyleGroupBox(gbSel);
        var col1 = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 2, Padding = new Padding(0, 6, 0, 0) };
        col1.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        col1.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        col1.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        col1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        col1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var lblFiltro = new Label { Text = "Categoría", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        cmbCategoriasFiltro = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 2, 0, 4) };
        UiTheme.StyleComboBox(cmbCategoriasFiltro);

        var lblBuscarProducto = new Label { Text = "Buscar producto", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        txtBusquedaProducto = new TextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Nombre o código (tiempo real)…", Margin = new Padding(0, 2, 0, 4) };
        UiTheme.StyleTextBox(txtBusquedaProducto);

        var lblProducto = new Label { Text = "Producto elegido", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        cmbProductos = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 2, 0, 0) };
        UiTheme.StyleComboBox(cmbProductos);
        cmbProductos.ValueMember = nameof(Producto.IdProducto);
        cmbProductos.DisplayMember = nameof(Producto.Nombre);
        cmbProductos.Format += (_, e) =>
        {
            if (e.ListItem is Producto p)
                e.Value = $"{p.Nombre} ({p.CodigoBarras})";
        };
        cmbProductos.SelectedIndexChanged += (_, _) => ActualizarStockSeleccionado();

        col1.Controls.Add(lblFiltro, 0, 0);
        col1.Controls.Add(cmbCategoriasFiltro, 1, 0);
        col1.Controls.Add(lblBuscarProducto, 0, 1);
        col1.Controls.Add(txtBusquedaProducto, 1, 1);
        col1.Controls.Add(lblProducto, 0, 2);
        col1.Controls.Add(cmbProductos, 1, 2);
        gbSel.Controls.Add(col1);

        var gbReg = new GroupBox { Text = " Registro y consulta de movimientos ", Dock = DockStyle.Fill };
        UiTheme.StyleGroupBox(gbReg);
        var col2 = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 2, Padding = new Padding(0, 6, 0, 0) };
        col2.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        col2.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        col2.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        col2.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        col2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        col2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var lblCantidad = new Label { Text = "Cantidad", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        txtCantidad = new TextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Ej: 10", Margin = new Padding(0, 2, 0, 4) };
        UiTheme.StyleTextBox(txtCantidad);

        var lblFecha = new Label { Text = "Fecha", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        dtpFecha = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Margin = new Padding(0, 2, 0, 4) };

        var lblStock = new Label { Text = "Stock actual", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        lblStockActualValor = new Label
        {
            Text = "-",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = UiTheme.PrimaryDark,
            Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
            Margin = new Padding(0, 2, 0, 4)
        };

        var lblBuscarMov = new Label { Text = "Filtrar tabla", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        txtBusquedaMovimientos = new TextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Nombre o código (tiempo real)…", Margin = new Padding(0, 2, 0, 0) };
        UiTheme.StyleTextBox(txtBusquedaMovimientos);

        col2.Controls.Add(lblCantidad, 0, 0);
        col2.Controls.Add(txtCantidad, 1, 0);
        col2.Controls.Add(lblFecha, 0, 1);
        col2.Controls.Add(dtpFecha, 1, 1);
        col2.Controls.Add(lblStock, 0, 2);
        col2.Controls.Add(lblStockActualValor, 1, 2);
        col2.Controls.Add(lblBuscarMov, 0, 3);
        col2.Controls.Add(txtBusquedaMovimientos, 1, 3);
        gbReg.Controls.Add(col2);

        var gbAcc = new GroupBox { Text = " Acciones ", Dock = DockStyle.Fill };
        UiTheme.StyleGroupBox(gbAcc);
        var tblAcc = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(4, 10, 4, 4)
        };
        tblAcc.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        tblAcc.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        tblAcc.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

        var btnInicial = new Button { Text = "Registrar inventario inicial", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
        UiTheme.StylePrimaryButton(btnInicial);
        btnInicial.Click += (_, _) => GuardarMovimiento(inicial: true);

        var btnEntrada = new Button { Text = "Registrar entrada", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 10) };
        UiTheme.StyleSecondaryButton(btnEntrada);
        btnEntrada.Click += (_, _) => GuardarMovimiento(inicial: false);

        var btnLimpiar = new Button { Text = "Limpiar formulario", Dock = DockStyle.Fill };
        UiTheme.StyleSecondaryButton(btnLimpiar);
        btnLimpiar.Click += (_, _) => LimpiarFormulario();

        tblAcc.Controls.Add(btnInicial, 0, 0);
        tblAcc.Controls.Add(btnEntrada, 0, 1);
        tblAcc.Controls.Add(btnLimpiar, 0, 2);
        gbAcc.Controls.Add(tblAcc);

        grid.Controls.Add(gbSel, 0, 0);
        grid.Controls.Add(gbReg, 1, 0);
        grid.Controls.Add(gbAcc, 2, 0);

        panelFormulario.Controls.Add(grid);

        dgvMovimientos = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false
        };
        UiTheme.StyleGrid(dgvMovimientos);
        dgvMovimientos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvMovimientos.ColumnHeadersHeight = 40;
        dgvMovimientos.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        dgvMovimientos.Columns.Add(new DataGridViewTextBoxColumn { Name = "IdInventario", HeaderText = "Id", DataPropertyName = nameof(InventarioMovimiento.IdInventario), Visible = false });
        dgvMovimientos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Fecha", HeaderText = "Fecha", DataPropertyName = nameof(InventarioMovimiento.Fecha), FillWeight = 12 });
        dgvMovimientos.Columns.Add(new DataGridViewTextBoxColumn { Name = "NombreProducto", HeaderText = "Producto", DataPropertyName = nameof(InventarioMovimiento.NombreProducto), FillWeight = 30 });
        dgvMovimientos.Columns.Add(new DataGridViewTextBoxColumn { Name = "CodigoBarras", HeaderText = "Código", DataPropertyName = nameof(InventarioMovimiento.CodigoBarras), FillWeight = 18 });
        dgvMovimientos.Columns.Add(new DataGridViewTextBoxColumn { Name = "NombreCategoria", HeaderText = "Categoría", DataPropertyName = nameof(InventarioMovimiento.NombreCategoria), FillWeight = 22 });
        dgvMovimientos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cantidad", HeaderText = "Cantidad", DataPropertyName = nameof(InventarioMovimiento.Cantidad), FillWeight = 10 });

        panelTabla.Controls.Add(dgvMovimientos);

        // Orden correcto para Docking
        Controls.Add(panelTabla);
        Controls.Add(panelFormulario);
        Controls.Add(panelHeader);
        Padding = new Padding(20);

        errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        errorProvider.ContainerControl = this;

        productSearchTimer.Interval = 250;
        productSearchTimer.Tick += (_, _) =>
        {
            productSearchTimer.Stop();
            BuscarProducto();
        };
        txtBusquedaProducto.TextChanged += (_, _) =>
        {
            productSearchTimer.Stop();
            productSearchTimer.Start();
        };

        movementSearchTimer.Interval = 250;
        movementSearchTimer.Tick += (_, _) =>
        {
            movementSearchTimer.Stop();
            CargarMovimientos();
        };
        txtBusquedaMovimientos.TextChanged += (_, _) =>
        {
            movementSearchTimer.Stop();
            movementSearchTimer.Start();
        };

        cmbCategoriasFiltro.SelectedIndexChanged += (_, _) =>
        {
            // Aplica filtro inmediatamente al inventario y a la búsqueda de producto.
            CargarMovimientos();
            BuscarProducto(silencioso: true);
        };

        CargarCategoriasFiltro();
        CargarMovimientos();
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

    private void BuscarProducto(bool silencioso = false)
    {
        string term = txtBusquedaProducto.Text.Trim();
        List<Producto> productos = string.IsNullOrWhiteSpace(term) ? [] : productoService.ListarProductos(term);

        int idCategoriaFiltro = GetCategoriaFiltroId();
        if (idCategoriaFiltro > 0)
        {
            productos = productos.Where(p => p.IdCategoria == idCategoriaFiltro).ToList();
        }

        cmbProductos.DataSource = productos;
        cmbProductos.SelectedIndex = productos.Count > 0 ? 0 : -1;

        ActualizarStockSeleccionado();

        if (!silencioso && productos.Count == 0 && !string.IsNullOrWhiteSpace(term))
        {
            MessageBox.Show("No se encontraron productos con ese nombre/código.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void GuardarMovimiento(bool inicial)
    {
        if (!ValidarVisualMovimiento())
        {
            return;
        }

        int idProducto = Convert.ToInt32(cmbProductos.SelectedValue);
        _ = ParseUtils.TryParseInt(txtCantidad.Text, out int cantidad);

        try
        {
            int idInventario = inicial
                ? inventarioService.RegistrarInventarioInicial(idProducto, cantidad, dtpFecha.Value.Date)
                : inventarioService.RegistrarEntradaInventario(idProducto, cantidad, dtpFecha.Value.Date);

            MessageBox.Show(
                inicial ? $"Inventario inicial registrado correctamente (Id: {idInventario})." : $"Entrada registrada correctamente (Id: {idInventario}).",
                "Confirmación",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            txtCantidad.Clear();
            CargarMovimientos();
            ActualizarStockSeleccionado();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo registrar el movimiento: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LimpiarFormulario()
    {
        txtBusquedaProducto.Clear();
        cmbProductos.DataSource = null;
        txtCantidad.Clear();
        txtBusquedaMovimientos.Clear();
        lblStockActualValor.Text = "-";
        dtpFecha.Value = DateTime.Today;
        errorProvider.Clear();
        ResetInputVisual(txtCantidad);
        ResetInputVisual(txtBusquedaProducto);
        ResetInputVisual(txtBusquedaMovimientos);

        CargarMovimientos();
    }

    private void ActualizarStockSeleccionado()
    {
        if (cmbProductos.SelectedValue is null)
        {
            lblStockActualValor.Text = "-";
            return;
        }

        int idProducto = Convert.ToInt32(cmbProductos.SelectedValue);
        int stock = inventarioService.ObtenerStockActual(idProducto);
        lblStockActualValor.Text = stock.ToString(CultureInfo.InvariantCulture);
    }

    private void CargarMovimientos()
    {
        try
        {
            int idCategoria = GetCategoriaFiltroId();
            int? filtroCategoria = idCategoria > 0 ? idCategoria : null;

            dgvMovimientos.DataSource = inventarioService.ListarMovimientos(filtroCategoria, txtBusquedaMovimientos.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudieron cargar los movimientos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    private bool ValidarVisualMovimiento()
    {
        errorProvider.Clear();
        ResetInputVisual(txtCantidad);

        bool ok = true;

        if (cmbProductos.SelectedValue is null)
        {
            ok = false;
            errorProvider.SetError(cmbProductos, "Selecciona un producto.");
        }

        if (!ParseUtils.TryParseInt(txtCantidad.Text, out int cantidad) || cantidad <= 0)
        {
            ok = false;
            MarkInvalid(txtCantidad, "La cantidad debe ser un entero mayor a 0.");
        }

        if (!ok)
        {
            MessageBox.Show("Corrige los campos marcados antes de continuar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        return ok;
    }

    private static void ResetInputVisual(TextBox tb)
    {
        tb.BackColor = Color.White;
    }

    private void MarkInvalid(TextBox tb, string message)
    {
        tb.BackColor = Color.FromArgb(255, 236, 236);
        errorProvider.SetError(tb, message);
    }
}
