using InventarioApp.Services;
using InventarioApp.Models;
using InventarioApp.Utils;

namespace InventarioApp.Forms;

public class ProductoForm : Form
{
    private readonly ProductoService productoService = new();
    private readonly CategoriaService categoriaService = new();

    private readonly TextBox txtNombre;
    private readonly TextBox txtCodigoBarras;
    private readonly TextBox txtPrecio;
    private readonly TextBox txtMarca;
    private readonly ComboBox cmbCategorias;
    private readonly TextBox txtBusqueda;
    private readonly Button btnGuardar;
    private readonly Button btnCargarEdicion;
    private readonly Button btnLimpiar;
    private readonly DataGridView dgvProductos;

    private int? productoEnEdicionId;
    private readonly ErrorProvider errorProvider = new();
    private readonly System.Windows.Forms.Timer searchTimer = new();

    public ProductoForm()
    {
        Text = "Vertice Muisca - Productos";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1100, 720);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = UiTheme.Background;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        AutoScaleMode = AutoScaleMode.Dpi;

        Panel panelHeader = UiTheme.CreateGradientHeader(120);
        Panel badge = UiTheme.CreateLogoBadge(20, 33, 54);

        var lblTitulo = new Label
        {
            Text = "Gestión de Productos",
            Left = 92,
            Top = 24,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
            ForeColor = Color.White
        };
        UiTheme.StyleHeaderTitle(lblTitulo);

        var lblSubtitulo = new Label
        {
            Text = "Registra, edita y busca productos por nombre o código",
            Left = 94,
            Top = 68,
            AutoSize = true,
            ForeColor = Color.FromArgb(245, 255, 250)
        };
        UiTheme.StyleHeaderSubtitle(lblSubtitulo);

        panelHeader.Controls.Add(badge);
        panelHeader.Controls.Add(lblTitulo);
        panelHeader.Controls.Add(lblSubtitulo);

        Panel panelFormulario = new() { Dock = DockStyle.Top, Height = 360, MinimumSize = new Size(0, 340), Padding = new Padding(16), BackColor = UiTheme.SurfaceSoft };
        UiTheme.StyleCard(panelFormulario);

        Panel panelTabla = new() { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = UiTheme.SurfaceSoft };
        UiTheme.StyleCard(panelTabla);

        var formGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        formGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 288));

        var colIzq = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        colIzq.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        colIzq.RowStyles.Add(new RowStyle(SizeType.Percent, 42));

        var gbDatos = new GroupBox { Text = " Datos del producto ", Dock = DockStyle.Fill };
        UiTheme.StyleGroupBox(gbDatos);
        var tblDatos = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 3,
            Padding = new Padding(0, 6, 0, 0)
        };
        tblDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        tblDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tblDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        tblDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tblDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        tblDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        tblDatos.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

        var lblNombre = new Label { Text = "Nombre", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        txtNombre = new TextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Ej: Arroz 500g", Margin = new Padding(0, 2, 8, 2) };
        UiTheme.StyleTextBox(txtNombre);
        var lblCodigo = new Label { Text = "Código", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        txtCodigoBarras = new TextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Ej: 770123…", Margin = new Padding(0, 2, 0, 2) };
        UiTheme.StyleTextBox(txtCodigoBarras);
        var lblPrecio = new Label { Text = "Precio", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        txtPrecio = new TextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Ej: 12500.50", Margin = new Padding(0, 2, 8, 2) };
        UiTheme.StyleTextBox(txtPrecio);
        var lblMarca = new Label { Text = "Marca", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        txtMarca = new TextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Opcional", Margin = new Padding(0, 2, 0, 2) };
        UiTheme.StyleTextBox(txtMarca);
        var lblCategoria = new Label { Text = "Categoría", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        cmbCategorias = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 2, 0, 2) };
        UiTheme.StyleComboBox(cmbCategorias);

        tblDatos.Controls.Add(lblNombre, 0, 0);
        tblDatos.Controls.Add(txtNombre, 1, 0);
        tblDatos.Controls.Add(lblCodigo, 2, 0);
        tblDatos.Controls.Add(txtCodigoBarras, 3, 0);
        tblDatos.Controls.Add(lblPrecio, 0, 1);
        tblDatos.Controls.Add(txtPrecio, 1, 1);
        tblDatos.Controls.Add(lblMarca, 2, 1);
        tblDatos.Controls.Add(txtMarca, 3, 1);
        tblDatos.Controls.Add(lblCategoria, 0, 2);
        tblDatos.Controls.Add(cmbCategorias, 1, 2);
        tblDatos.SetColumnSpan(cmbCategorias, 3);
        gbDatos.Controls.Add(tblDatos);

        var gbBuscar = new GroupBox { Text = " Buscar en la lista (tiempo real) ", Dock = DockStyle.Fill };
        UiTheme.StyleGroupBox(gbBuscar);
        var tblBuscar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 6, 0, 0) };
        tblBuscar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
        tblBuscar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var lblBusqueda = new Label { Text = "Nombre o código", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        txtBusqueda = new TextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, PlaceholderText = "Escribe para filtrar la tabla…" };
        UiTheme.StyleTextBox(txtBusqueda);
        tblBuscar.Controls.Add(lblBusqueda, 0, 0);
        tblBuscar.Controls.Add(txtBusqueda, 1, 0);
        gbBuscar.Controls.Add(tblBuscar);

        colIzq.Controls.Add(gbDatos, 0, 0);
        colIzq.Controls.Add(gbBuscar, 0, 1);

        var gbAcciones = new GroupBox { Text = " Acciones ", Dock = DockStyle.Fill };
        UiTheme.StyleGroupBox(gbAcciones);
        var tblAcciones = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            Padding = new Padding(4, 8, 4, 4)
        };
        tblAcciones.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        tblAcciones.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        tblAcciones.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        btnGuardar = new Button { Text = "Guardar producto", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8) };
        UiTheme.StylePrimaryButton(btnGuardar);
        btnGuardar.Click += BtnGuardar_Click;

        btnCargarEdicion = new Button { Text = "Cargar para editar", Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8) };
        UiTheme.StyleSecondaryButton(btnCargarEdicion);
        btnCargarEdicion.Click += BtnEditar_Click;

        btnLimpiar = new Button { Text = "Limpiar formulario", Dock = DockStyle.Fill };
        UiTheme.StyleSecondaryButton(btnLimpiar);
        btnLimpiar.Click += (_, _) => LimpiarFormulario();

        tblAcciones.Controls.Add(btnGuardar, 0, 0);
        tblAcciones.Controls.Add(btnCargarEdicion, 0, 1);
        tblAcciones.Controls.Add(btnLimpiar, 0, 2);
        gbAcciones.Controls.Add(tblAcciones);

        formGrid.Controls.Add(colIzq, 0, 0);
        formGrid.Controls.Add(gbAcciones, 1, 0);

        panelFormulario.Controls.Add(formGrid);

        dgvProductos = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false
        };
        UiTheme.StyleGrid(dgvProductos);
        dgvProductos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvProductos.ColumnHeadersHeight = 40;
        dgvProductos.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dgvProductos.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "IdProducto", HeaderText = "ID", DataPropertyName = nameof(Producto.IdProducto), FillWeight = 8 });
        dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre", HeaderText = "Nombre", DataPropertyName = nameof(Producto.Nombre), FillWeight = 26 });
        dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "CodigoBarras", HeaderText = "Código", DataPropertyName = nameof(Producto.CodigoBarras), FillWeight = 18 });
        dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "Precio", DataPropertyName = nameof(Producto.Precio), FillWeight = 12 });
        dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Marca", HeaderText = "Marca", DataPropertyName = nameof(Producto.Marca), FillWeight = 14 });
        dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "NombreCategoria", HeaderText = "Categoría", DataPropertyName = nameof(Producto.NombreCategoria), FillWeight = 22 });
        dgvProductos.Columns.Add(new DataGridViewTextBoxColumn { Name = "IdCategoria", HeaderText = "IdCategoria", DataPropertyName = nameof(Producto.IdCategoria), Visible = false });

        panelTabla.Controls.Add(dgvProductos);

        // Orden correcto para Docking
        Controls.Add(panelTabla);
        Controls.Add(panelFormulario);
        Controls.Add(panelHeader);
        Padding = new Padding(20);

        errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
        errorProvider.ContainerControl = this;

        searchTimer.Interval = 250;
        searchTimer.Tick += (_, _) =>
        {
            searchTimer.Stop();
            CargarProductos(txtBusqueda.Text);
        };
        txtBusqueda.TextChanged += (_, _) =>
        {
            searchTimer.Stop();
            searchTimer.Start();
        };

        CargarCategoriasCombo();
        CargarProductos(null);
    }

    private void CargarCategoriasCombo()
    {
        try
        {
            List<Categoria> categorias = categoriaService.ListarCategorias();
            cmbCategorias.DataSource = categorias;
            cmbCategorias.DisplayMember = nameof(Categoria.NombreCategoria);
            cmbCategorias.ValueMember = nameof(Categoria.IdCategoria);
            cmbCategorias.SelectedIndex = categorias.Count > 0 ? 0 : -1;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudieron cargar las categorías: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CargarProductos(string? terminoBusqueda)
    {
        try
        {
            dgvProductos.DataSource = productoService.ListarProductos(terminoBusqueda);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudieron cargar los productos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        if (!ValidarVisual())
        {
            return;
        }

        string nombre = txtNombre.Text.Trim();
        string codigoBarras = txtCodigoBarras.Text.Trim();
        int idCategoria = ObtenerIdCategoriaSeleccionada();
        decimal precio = ParseDecimalSeguro(txtPrecio.Text);

        try
        {
            if (productoEnEdicionId.HasValue)
            {
                productoService.EditarProducto(productoEnEdicionId.Value, nombre, codigoBarras, precio, txtMarca.Text.Trim(), idCategoria);
                MessageBox.Show("Producto actualizado correctamente.", "Confirmación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                productoService.RegistrarProducto(nombre, codigoBarras, precio, txtMarca.Text.Trim(), idCategoria);
                MessageBox.Show("Producto registrado correctamente.", "Confirmación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            LimpiarFormulario();
            CargarCategoriasCombo();
            CargarProductos(null);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo guardar el producto: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnEditar_Click(object? sender, EventArgs e)
    {
        if (dgvProductos.SelectedRows.Count == 0)
        {
            MessageBox.Show("Selecciona un producto de la tabla para cargarlo en edición.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DataGridViewRow fila = dgvProductos.SelectedRows[0];
        productoEnEdicionId = Convert.ToInt32(fila.Cells["IdProducto"].Value);

        txtNombre.Text = fila.Cells["Nombre"].Value?.ToString() ?? string.Empty;
        txtCodigoBarras.Text = fila.Cells["CodigoBarras"].Value?.ToString() ?? string.Empty;
        txtPrecio.Text = fila.Cells["Precio"].Value?.ToString() ?? string.Empty;
        txtMarca.Text = fila.Cells["Marca"].Value?.ToString() ?? string.Empty;

        int idCategoria = Convert.ToInt32(fila.Cells["IdCategoria"].Value);
        cmbCategorias.SelectedValue = idCategoria;

        txtNombre.Focus();
    }

    private void LimpiarFormulario()
    {
        productoEnEdicionId = null;
        txtNombre.Clear();
        txtCodigoBarras.Clear();
        txtPrecio.Clear();
        txtMarca.Clear();
        txtBusqueda.Clear();
        errorProvider.Clear();
        ResetInputVisual(txtNombre);
        ResetInputVisual(txtCodigoBarras);
        ResetInputVisual(txtPrecio);

        cmbCategorias.SelectedIndex = cmbCategorias.Items.Count > 0 ? 0 : -1;
        CargarProductos(null);
        txtNombre.Focus();
    }

    private bool ValidarVisual()
    {
        errorProvider.Clear();
        ResetInputVisual(txtNombre);
        ResetInputVisual(txtCodigoBarras);
        ResetInputVisual(txtPrecio);

        bool ok = true;

        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            ok = false;
            MarkInvalid(txtNombre, "El nombre es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(txtCodigoBarras.Text))
        {
            ok = false;
            MarkInvalid(txtCodigoBarras, "El código es obligatorio.");
        }

        if (cmbCategorias.SelectedIndex < 0 || cmbCategorias.SelectedValue is null)
        {
            ok = false;
            errorProvider.SetError(cmbCategorias, "Selecciona una categoría.");
        }

        if (string.IsNullOrWhiteSpace(txtPrecio.Text) || !ParseUtils.TryParseDecimal(txtPrecio.Text, out decimal precio) || precio <= 0m)
        {
            ok = false;
            MarkInvalid(txtPrecio, "El precio debe ser numérico y mayor a 0.");
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

    private static decimal ParseDecimalSeguro(string input)
    {
        _ = ParseUtils.TryParseDecimal(input, out decimal value);
        return value;
    }

    private int ObtenerIdCategoriaSeleccionada()
    {
        object? value = cmbCategorias.SelectedValue;
        if (value is int id)
            return id;
        if (value is Categoria cat)
            return cat.IdCategoria;
        if (value is string s && int.TryParse(s, out int parsed))
            return parsed;
        if (cmbCategorias.SelectedItem is Categoria catItem)
            return catItem.IdCategoria;
        return 0;
    }
}
