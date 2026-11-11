using pryHeladeriaBravoSegura.Models;
using pryHeladeriaBravoSegura.Services;

namespace pryHeladeriaBravoSegura.Forms;

public sealed class FormularioCargaRemito : Form
{
    private readonly TextBox txtBuscar = new();
    private readonly ComboBox cmbTipo = new();
    private readonly ComboBox cmbEstado = new();
    private readonly DateTimePicker dtpFecha = new();
    private readonly TextBox txtNumero = new();
    private readonly DataGridView grilla = new();
    private readonly List<FilaRemito> filas;

    public string NumeroRemito => txtNumero.Text.Trim();
    public DateTime FechaRemito => dtpFecha.Value.Date;
    public IReadOnlyList<DetalleEntrada> Detalles => filas
        .Where(fila => fila.Cantidad > 0)
        .Select(fila => new DetalleEntrada(fila.IdProducto, fila.Descripcion, fila.Cantidad))
        .ToList();

    public FormularioCargaRemito(IReadOnlyList<ProductoResumen> productos, string nombreUsuario, IWin32Window? propietario = null)
    {
        filas = productos
            .Select(producto => new FilaRemito(producto.IdProducto, producto.Descripcion, producto.NombreTipo, producto.StockActual, producto.Estado))
            .ToList();

        Text = "Cargar remito - entrada de stock";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(850, 560);
        ClientSize = new Size(950, 650);

        var encabezado = new Panel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(12, 8, 12, 5) };
        var lblTitulo = new Label
        {
            Text = "PRODUCTOS ENTRANTES",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font(Font.FontFamily, 16, FontStyle.Bold)
        };
        var lblUsuario = new Label { Text = $"Usuario: {nombreUsuario}", AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        encabezado.Controls.Add(lblTitulo);
        encabezado.Controls.Add(lblUsuario);
        encabezado.Resize += (_, _) => lblUsuario.Left = encabezado.ClientSize.Width - lblUsuario.Width;

        var filtros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(12, 8, 12, 5), WrapContents = false };
        txtBuscar.Width = 240;
        txtBuscar.PlaceholderText = "Buscar producto...";
        cmbTipo.Width = 180;
        cmbEstado.Width = 150;
        cmbTipo.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbEstado.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTipo.Items.Add("Todas las categorías");
        cmbTipo.Items.AddRange(filas.Select(fila => fila.NombreTipo).Distinct().OrderBy(nombre => nombre).ToArray());
        cmbEstado.Items.AddRange(["Todos", "Stock crítico", "Agotados", "Disponibles"]);
        cmbTipo.SelectedIndex = 0;
        cmbEstado.SelectedIndex = 0;
        filtros.Controls.Add(new Label { Text = "Buscar productos:", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(0, 5, 0, 0) });
        filtros.Controls.Add(txtBuscar);
        filtros.Controls.Add(new Label { Text = "Categoría:", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(10, 5, 0, 0) });
        filtros.Controls.Add(cmbTipo);
        filtros.Controls.Add(new Label { Text = "Estado:", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(10, 5, 0, 0) });
        filtros.Controls.Add(cmbEstado);
        txtBuscar.TextChanged += (_, _) => ActualizarFilas();
        cmbTipo.SelectedIndexChanged += (_, _) => ActualizarFilas();
        cmbEstado.SelectedIndexChanged += (_, _) => ActualizarFilas();

        var datosRemito = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(12, 10, 12, 5), WrapContents = false };
        txtNumero.Width = 150;
        dtpFecha.Width = 120;
        dtpFecha.Format = DateTimePickerFormat.Short;
        datosRemito.Controls.Add(new Label { Text = "N.º de remito:", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(0, 5, 0, 0) });
        datosRemito.Controls.Add(txtNumero);
        datosRemito.Controls.Add(new Label { Text = "Fecha:", AutoSize = true, Font = new Font(Font, FontStyle.Bold), Padding = new Padding(15, 5, 0, 0) });
        datosRemito.Controls.Add(dtpFecha);

        PrepararGrilla();
        var botones = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55, Padding = new Padding(12, 10, 12, 10), FlowDirection = FlowDirection.RightToLeft };
        var btnAceptar = new Button { Text = "Registrar entrada", AutoSize = true };
        var btnCancelar = new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, AutoSize = true };
        botones.Controls.Add(btnAceptar);
        botones.Controls.Add(btnCancelar);

        Controls.Add(grilla);
        Controls.Add(botones);
        Controls.Add(datosRemito);
        Controls.Add(filtros);
        Controls.Add(encabezado);
        AcceptButton = btnAceptar;
        CancelButton = btnCancelar;
        btnAceptar.Click += (_, _) => ValidarCarga();
        Shown += (_, _) => txtBuscar.Focus();
    }

    private void PrepararGrilla()
    {
        grilla.Dock = DockStyle.Fill;
        grilla.AllowUserToAddRows = false;
        grilla.AllowUserToDeleteRows = false;
        grilla.AllowUserToResizeColumns = false;
        grilla.AllowUserToResizeRows = false;
        grilla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grilla.AutoGenerateColumns = false;
        grilla.SelectionMode = DataGridViewSelectionMode.CellSelect;
        grilla.RowHeadersVisible = false;
        grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PRODUCTO", DataPropertyName = "Descripcion", ReadOnly = true });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CATEGORÍA", DataPropertyName = "NombreTipo", ReadOnly = true, Width = 160 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "ESTADO", DataPropertyName = "EstadoTexto", ReadOnly = true, Width = 130 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "STOCK ACTUAL", DataPropertyName = "StockActual", ReadOnly = true, Width = 120 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CANTIDAD ENTRANTE", DataPropertyName = "Cantidad", Width = 150 });
        grilla.DataSource = filas;
        grilla.KeyDown += GrillaKeyDown;
        grilla.EditingControlShowing += (_, evento) =>
        {
            if (evento.Control is not TextBox editor)
            {
                return;
            }

            editor.KeyPress -= SoloDigitos;
            editor.KeyPress += SoloDigitos;
            editor.PreviewKeyDown -= EditorPreviewKeyDown;
            editor.PreviewKeyDown += EditorPreviewKeyDown;
            editor.KeyDown -= CantidadTabKeyDown;
            editor.KeyDown += CantidadTabKeyDown;
        };
    }

    private static void EditorPreviewKeyDown(object? sender, PreviewKeyDownEventArgs evento)
    {
        if (evento.KeyCode == Keys.Tab)
        {
            evento.IsInputKey = true;
        }
    }

    private void CantidadTabKeyDown(object? sender, KeyEventArgs evento)
    {
        if (evento.KeyCode != Keys.Tab || grilla.CurrentCell?.ColumnIndex != 4 || grilla.Rows.Count == 0)
        {
            return;
        }

        int incremento = evento.Shift ? -1 : 1;
        int siguienteFila = grilla.CurrentCell.RowIndex + incremento;
        if (siguienteFila < 0)
        {
            siguienteFila = grilla.Rows.Count - 1;
        }
        else if (siguienteFila >= grilla.Rows.Count)
        {
            siguienteFila = 0;
        }

        grilla.EndEdit();
        grilla.CurrentCell = grilla.Rows[siguienteFila].Cells[4];
        grilla.BeginEdit(true);
        evento.SuppressKeyPress = true;
        evento.Handled = true;
    }

    private void ActualizarFilas()
    {
        string busqueda = txtBuscar.Text.Trim();
        string? tipo = cmbTipo.SelectedIndex > 0 ? cmbTipo.SelectedItem?.ToString() : null;
        FiltroEstadoProducto filtro = cmbEstado.SelectedIndex switch
        {
            1 => FiltroEstadoProducto.StockCritico,
            2 => FiltroEstadoProducto.Agotados,
            3 => FiltroEstadoProducto.Disponible,
            _ => FiltroEstadoProducto.Todos
        };

        grilla.DataSource = filas
            .Where(fila => string.IsNullOrEmpty(busqueda) || fila.Descripcion.Contains(busqueda, StringComparison.OrdinalIgnoreCase))
            .Where(fila => tipo is null || fila.NombreTipo == tipo)
            .Where(fila => filtro switch
            {
                FiltroEstadoProducto.StockCritico => fila.Estado == EstadoProducto.StockCritico,
                FiltroEstadoProducto.Agotados => fila.Estado == EstadoProducto.Agotado,
                FiltroEstadoProducto.Disponible => fila.Estado == EstadoProducto.Disponible,
                _ => true
            })
            .ToList();
    }

    private void ValidarCarga()
    {
        grilla.EndEdit();
        if (string.IsNullOrWhiteSpace(NumeroRemito))
        {
            MessageBox.Show(this, "Ingrese el número de remito.", "Remito", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            txtNumero.Focus();
            return;
        }

        if (filas.Any(fila => fila.Cantidad < 0))
        {
            MessageBox.Show(this, "Las cantidades deben ser enteros positivos o cero.", "Cantidad inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        if (Detalles.Count == 0)
        {
            MessageBox.Show(this, "Ingrese una cantidad para al menos un producto.", "Remito", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        using var confirmacion = new FormularioConfirmacionRemito(NumeroRemito, FechaRemito, Detalles, this);
        if (confirmacion.ShowDialog(this) == DialogResult.OK)
        {
            DialogResult = DialogResult.OK;
        }
        else
        {
            DialogResult = DialogResult.None;
        }
    }

    private void GrillaKeyDown(object? sender, KeyEventArgs evento)
    {
        if (evento.KeyCode != Keys.Tab || grilla.CurrentCell?.ColumnIndex != 4 || grilla.Rows.Count == 0)
        {
            return;
        }

        int incremento = evento.Shift ? -1 : 1;
        int siguienteFila = grilla.CurrentCell.RowIndex + incremento;
        if (siguienteFila < 0)
        {
            siguienteFila = grilla.Rows.Count - 1;
        }
        else if (siguienteFila >= grilla.Rows.Count)
        {
            siguienteFila = 0;
        }

        grilla.EndEdit();
        grilla.CurrentCell = grilla.Rows[siguienteFila].Cells[4];
        grilla.BeginEdit(true);
        evento.SuppressKeyPress = true;
        evento.Handled = true;
    }

    private static void SoloDigitos(object? sender, KeyPressEventArgs evento)
    {
        if (!char.IsControl(evento.KeyChar) && !char.IsDigit(evento.KeyChar))
        {
            evento.Handled = true;
        }
    }

    public sealed record DetalleEntrada(int IdProducto, string Descripcion, int Cantidad);

    private sealed class FilaRemito
    {
        public int IdProducto { get; }
        public string Descripcion { get; }
        public string NombreTipo { get; }
        public int StockActual { get; }
        public EstadoProducto Estado { get; }
        public string EstadoTexto => Estado switch
        {
            EstadoProducto.Agotado => "Agotado",
            EstadoProducto.StockCritico => "Stock crítico",
            _ => "Disponible"
        };
        public int Cantidad { get; set; }

        public FilaRemito(int idProducto, string descripcion, string nombreTipo, int stockActual, EstadoProducto estado)
        {
            IdProducto = idProducto;
            Descripcion = descripcion;
            NombreTipo = nombreTipo;
            StockActual = stockActual;
            Estado = estado;
        }
    }
}
