using InventarioApp.Services;
using InventarioApp.Models;

namespace InventarioApp.Forms;

public class CategoriaForm : Form
{
    private readonly Label lblTitulo;
    private readonly Label lblSubtitulo;

    private readonly TextBox txtNombreCategoria;
    private readonly TextBox txtBusqueda;
    private readonly Button btnGuardar;
    private readonly Button btnEditar;
    private readonly Button btnLimpiar;
    private readonly DataGridView dgvCategorias;

    private int? categoriaEnEdicionId;
    private readonly CategoriaService categoriaService = new();
    private readonly ErrorProvider errorProvider = new();
    private readonly System.Windows.Forms.Timer searchTimer = new();

    public CategoriaForm()
    {
        Text = "Vertice Muisca - Categorias";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 600);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = UiTheme.Background;
        Font = new Font("Segoe UI", 10F, FontStyle.Regular);
        AutoScaleMode = AutoScaleMode.Dpi;

        Panel panelHeader = UiTheme.CreateGradientHeader(120);

        Panel badge = UiTheme.CreateLogoBadge(20, 33, 54);

        lblTitulo = new Label
        {
            Text = "Gestion de Categorias",
            Left = 92,
            Top = 24,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
            ForeColor = Color.White
        };
        UiTheme.StyleHeaderTitle(lblTitulo);

        lblSubtitulo = new Label
        {
            Text = "Crea y edita categorias para organizar tu supermercado",
            Left = 94,
            Top = 68,
            AutoSize = true,
            ForeColor = Color.FromArgb(245, 255, 250)
        };
        UiTheme.StyleHeaderSubtitle(lblSubtitulo);

        panelHeader.Controls.Add(badge);
        panelHeader.Controls.Add(lblTitulo);
        panelHeader.Controls.Add(lblSubtitulo);

        // Altura suficiente para 3 botones + GroupBoxes (evita que solo se vea “Guardar”).
        Panel panelFormulario = new() { Dock = DockStyle.Top, Height = 300, MinimumSize = new Size(0, 280), Padding = new Padding(16), BackColor = UiTheme.SurfaceSoft };
        UiTheme.StyleCard(panelFormulario);

        Panel panelTabla = new() { Dock = DockStyle.Fill, Padding = new Padding(16), BackColor = UiTheme.SurfaceSoft };
        UiTheme.StyleCard(panelTabla);

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            AutoSize = false
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 272));

        var colIzq = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        colIzq.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
        colIzq.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));

        var gbDatos = new GroupBox { Text = " Datos de la categoría ", Dock = DockStyle.Fill };
        UiTheme.StyleGroupBox(gbDatos);
        var tblDatos = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 4, 0, 0) };
        tblDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        tblDatos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var lblNombreCategoria = new Label { Text = "Nombre", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        txtNombreCategoria = new TextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 2, 0, 2) };
        UiTheme.StyleTextBox(txtNombreCategoria);
        txtNombreCategoria.PlaceholderText = "Ej: Lácteos";
        tblDatos.Controls.Add(lblNombreCategoria, 0, 0);
        tblDatos.Controls.Add(txtNombreCategoria, 1, 0);
        gbDatos.Controls.Add(tblDatos);

        var gbBuscar = new GroupBox { Text = " Buscar en la lista (tiempo real) ", Dock = DockStyle.Fill };
        UiTheme.StyleGroupBox(gbBuscar);
        var tblBuscar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Padding = new Padding(0, 4, 0, 0) };
        tblBuscar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
        tblBuscar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var lblBusqueda = new Label { Text = "Texto a buscar", AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, ForeColor = UiTheme.TextMain };
        txtBusqueda = new TextBox { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(0, 2, 0, 2) };
        UiTheme.StyleTextBox(txtBusqueda);
        txtBusqueda.PlaceholderText = "Escribe el nombre…";
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
            Padding = new Padding(4, 6, 4, 4)
        };
        tblAcciones.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        tblAcciones.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        tblAcciones.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        btnGuardar = new Button { Text = "Guardar categoría", Dock = DockStyle.Fill, Height = 44, Margin = new Padding(0, 0, 0, 6) };
        UiTheme.StylePrimaryButton(btnGuardar);
        btnGuardar.Click += BtnGuardar_Click;

        btnEditar = new Button { Text = "Cargar para editar", Dock = DockStyle.Fill, Height = 44, Margin = new Padding(0, 0, 0, 6) };
        UiTheme.StyleSecondaryButton(btnEditar);
        btnEditar.Click += BtnEditar_Click;

        btnLimpiar = new Button { Text = "Limpiar formulario", Dock = DockStyle.Fill, Height = 44 };
        UiTheme.StyleSecondaryButton(btnLimpiar);
        btnLimpiar.Click += (_, _) => LimpiarFormulario();

        tblAcciones.Controls.Add(btnGuardar, 0, 0);
        tblAcciones.Controls.Add(btnEditar, 0, 1);
        tblAcciones.Controls.Add(btnLimpiar, 0, 2);
        gbAcciones.Controls.Add(tblAcciones);

        grid.Controls.Add(colIzq, 0, 0);
        grid.Controls.Add(gbAcciones, 1, 0);

        panelFormulario.Controls.Add(grid);

        dgvCategorias = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false
        };
        UiTheme.StyleGrid(dgvCategorias);
        dgvCategorias.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvCategorias.ColumnHeadersHeight = 40;
        dgvCategorias.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        dgvCategorias.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

        dgvCategorias.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "IdCategoria",
            HeaderText = "ID",
            DataPropertyName = nameof(Categoria.IdCategoria),
            FillWeight = 15
        });
        dgvCategorias.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "NombreCategoria",
            HeaderText = "Nombre de categoría",
            DataPropertyName = nameof(Categoria.NombreCategoria),
            FillWeight = 85
        });

        panelTabla.Controls.Add(dgvCategorias);

        // Orden IMPORTANTE para Docking:
        // 1) Fill (tabla), 2) Top (form), 3) Top (header) al final.
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
            CargarCategorias(txtBusqueda.Text);
        };
        txtBusqueda.TextChanged += (_, _) =>
        {
            searchTimer.Stop();
            searchTimer.Start();
        };

        CargarCategorias(null);
    }

    private void CargarCategorias(string? terminoBusqueda)
    {
        try
        {
            dgvCategorias.DataSource = categoriaService.ListarCategorias(terminoBusqueda);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudieron cargar las categorias: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        if (!ValidarVisual())
        {
            return;
        }

        string nombreCategoria = txtNombreCategoria.Text.Trim();

        try
        {
            if (categoriaEnEdicionId.HasValue)
            {
                categoriaService.EditarCategoria(categoriaEnEdicionId.Value, nombreCategoria);
                MessageBox.Show("Categoría actualizada correctamente.", "Confirmación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                categoriaService.RegistrarCategoria(nombreCategoria);
                MessageBox.Show("Categoría registrada correctamente.", "Confirmación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            LimpiarFormulario();
            CargarCategorias(null);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo guardar la categoria: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnEditar_Click(object? sender, EventArgs e)
    {
        if (dgvCategorias.SelectedRows.Count == 0)
        {
            MessageBox.Show("Seleccione una categoria para editar.", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DataGridViewRow fila = dgvCategorias.SelectedRows[0];
        categoriaEnEdicionId = Convert.ToInt32(fila.Cells["IdCategoria"].Value);
        txtNombreCategoria.Text = fila.Cells["NombreCategoria"].Value?.ToString() ?? string.Empty;
        txtNombreCategoria.Focus();
    }

    private void LimpiarFormulario()
    {
        categoriaEnEdicionId = null;
        txtNombreCategoria.Clear();
        txtBusqueda.Clear();
        errorProvider.SetError(txtNombreCategoria, "");
        txtNombreCategoria.BackColor = Color.White;
        CargarCategorias(null);
        txtNombreCategoria.Focus();
    }

    private bool ValidarVisual()
    {
        bool ok = true;
        errorProvider.SetError(txtNombreCategoria, "");
        txtNombreCategoria.BackColor = Color.White;

        if (string.IsNullOrWhiteSpace(txtNombreCategoria.Text))
        {
            ok = false;
            errorProvider.SetError(txtNombreCategoria, "El nombre de la categoría es obligatorio.");
            txtNombreCategoria.BackColor = Color.FromArgb(255, 236, 236);
        }

        if (!ok)
        {
            MessageBox.Show("Corrige los campos marcados antes de continuar.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtNombreCategoria.Focus();
        }

        return ok;
    }
}
