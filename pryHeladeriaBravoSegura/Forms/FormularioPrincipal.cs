using pryHeladeriaBravoSegura.Models;
using pryHeladeriaBravoSegura.Services;

namespace pryHeladeriaBravoSegura.Forms;

public sealed class FormularioPrincipal : Form
{
    private const int ColumnaEstado = 2;
    private const int ColumnaCantidad = 4;
    private const int ColumnaSalida = 5;
    private readonly ServiciosAplicacion servicios;
    private readonly TextBox txtBuscar = new();
    private readonly ComboBox cmbTipo = new();
    private readonly ComboBox cmbEstado = new();
    private readonly DataGridView grilla = new();
    private readonly Label lblUsuario = new();

    public FormularioPrincipal(ServiciosAplicacion servicios)
    {
        this.servicios = servicios;
        Text = "GLUP'S - Control y Monitoreo de Stock";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 600);
        ConstruirInterfaz();
        CargarFiltros();
        ActualizarGrilla();
    }

    private void ConstruirInterfaz()
    {
        var barra = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.FromArgb(43, 61, 80), Padding = new Padding(12, 8, 12, 8) };
        var titulo = new Label { Text = "GLUP'S - Control y Monitoreo de Stock", ForeColor = Color.White, Font = new Font(Font.FontFamily, 14, FontStyle.Bold), AutoSize = true, Location = new Point(12, 17) };
        lblUsuario.Text = $"Usuario: {servicios.Auth.UsuarioActual?.Nombre}";
        lblUsuario.ForeColor = Color.White;
        lblUsuario.AutoSize = true;
        lblUsuario.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblUsuario.Location = new Point(Width - 260, 20);
        var btnSalir = new Button { Text = "Salir", Width = 75, Height = 32, Anchor = AnchorStyles.Top | AnchorStyles.Right };
        btnSalir.Location = new Point(Width - 95, 13);
        btnSalir.Click += (_, _) => Salir();
        barra.Controls.Add(titulo);
        barra.Controls.Add(lblUsuario);
        barra.Controls.Add(btnSalir);
        barra.Resize += (_, _) =>
        {
            lblUsuario.Left = barra.ClientSize.Width - 260;
            btnSalir.Left = barra.ClientSize.Width - 87;
        };

        var filtros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(12, 10, 12, 5), WrapContents = false };
        txtBuscar.Width = 260;
        txtBuscar.PlaceholderText = "Buscar producto...";
        cmbTipo.Width = 170;
        cmbEstado.Width = 160;
        cmbEstado.Items.AddRange(["Todos", "Stock crítico", "Agotados"]);
        cmbEstado.SelectedIndex = 0;
        filtros.Controls.Add(txtBuscar);
        filtros.Controls.Add(new Label { Text = "Categoría:", AutoSize = true, Padding = new Padding(10, 7, 0, 0) });
        filtros.Controls.Add(cmbTipo);
        filtros.Controls.Add(new Label { Text = "Estado:", AutoSize = true, Padding = new Padding(10, 7, 0, 0) });
        filtros.Controls.Add(cmbEstado);

        PrepararGrilla();
        var inferior = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60, Padding = new Padding(12, 10, 12, 10), WrapContents = false };
        var btnNuevo = new Button { Text = "+ Registrar producto faltante", AutoSize = true, Height = 36 };
        var btnPedidos = new Button { Text = "Ver historial de pedidos", AutoSize = true, Height = 36 };
        var btnMovimientos = new Button { Text = "Ver movimientos", AutoSize = true, Height = 36 };
        var btnPedidoNuevo = new Button { Text = "Generar pedido nuevo", AutoSize = true, Height = 36 };
        var btnEntrada = new Button { Text = "Registrar entrada", AutoSize = true, Height = 36 };
        inferior.Controls.AddRange([btnNuevo, btnEntrada, btnPedidos, btnMovimientos, btnPedidoNuevo]);

        Controls.Add(grilla);
        Controls.Add(inferior);
        Controls.Add(filtros);
        Controls.Add(barra);
        AjustarTamanoFormulario();

        txtBuscar.TextChanged += (_, _) => ActualizarGrilla();
        cmbTipo.SelectedIndexChanged += (_, _) => ActualizarGrilla();
        cmbEstado.SelectedIndexChanged += (_, _) => ActualizarGrilla();
        grilla.CellContentClick += (_, evento) => EjecutarAccionGrilla(evento);
        grilla.CellClick += (_, evento) => EnfocarCantidad(evento.RowIndex);
        grilla.EditingControlShowing += GrillaEditingControlShowing;
        grilla.CellFormatting += GrillaCellFormatting;
        grilla.DataError += (_, evento) => evento.ThrowException = false;
        btnNuevo.Click += (_, _) => AbrirNuevoProducto();
        btnPedidos.Click += (_, _) => new FormularioHistorialPedidos(servicios).ShowDialog(this);
        btnMovimientos.Click += (_, _) => new FormularioHistorialMovimientos(servicios).ShowDialog(this);
        btnPedidoNuevo.Click += (_, _) => AbrirPedidoNuevo();
        btnEntrada.Click += (_, _) => RegistrarMovimientoSeleccionado("Entrada");
    }

    private void PrepararGrilla()
    {
        grilla.Dock = DockStyle.Fill;
        grilla.AllowUserToAddRows = false;
        grilla.AllowUserToDeleteRows = false;
        grilla.AllowUserToResizeColumns = false;
        grilla.AllowUserToResizeRows = false;
        grilla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grilla.ReadOnly = false;
        grilla.AutoGenerateColumns = false;
        grilla.RowHeadersVisible = false;
        grilla.SelectionMode = DataGridViewSelectionMode.CellSelect;
        grilla.MultiSelect = false;
        grilla.RowTemplate.Height = 38;
        grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = "Descripcion", HeaderText = "PRODUCTO", DataPropertyName = "Descripcion", ReadOnly = true, Width = 230 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = "NombreTipo", HeaderText = "CATEGORÍA", DataPropertyName = "NombreTipo", ReadOnly = true, Width = 140 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = "Estado", HeaderText = "ESTADO ACTUAL", DataPropertyName = "EstadoTexto", ReadOnly = true, Width = 150 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = "Stock", HeaderText = "STOCK", DataPropertyName = "StockActual", ReadOnly = true, Width = 90 });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cantidad", HeaderText = "CANTIDAD", ReadOnly = false, Width = 125 });
        grilla.Columns.Add(new DataGridViewButtonColumn { Name = "Salida", HeaderText = "REGISTRAR SALIDA", Text = "− Salida", UseColumnTextForButtonValue = true, Width = 130 });

        foreach (DataGridViewColumn columna in grilla.Columns)
        {
            columna.ReadOnly = columna.Index != ColumnaCantidad;
        }

        DataGridViewCellStyle estiloEncabezado = new()
        {
            Alignment = DataGridViewContentAlignment.MiddleCenter,
            Font = new Font(grilla.Font, FontStyle.Bold)
        };
        grilla.ColumnHeadersDefaultCellStyle = estiloEncabezado;
        grilla.Columns[0].HeaderCell.Style = estiloEncabezado;
        grilla.Columns[1].HeaderCell.Style = estiloEncabezado;
        grilla.Columns[2].HeaderCell.Style = estiloEncabezado;
        grilla.Columns[3].HeaderCell.Style = estiloEncabezado;
        grilla.Columns[4].HeaderCell.Style = estiloEncabezado;
        grilla.Columns[5].HeaderCell.Style = estiloEncabezado;
        grilla.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grilla.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grilla.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        grilla.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
    }

    private void AjustarTamanoFormulario()
    {
        Rectangle areaTrabajo = Screen.FromControl(this).WorkingArea;
        int anchoTabla = grilla.Columns.Cast<DataGridViewColumn>()
            .Where(columna => columna.Visible)
            .Sum(columna => columna.Width);
        int anchoDeseado = anchoTabla + grilla.RowHeadersWidth + SystemInformation.VerticalScrollBarWidth + 24;
        int ancho = Math.Min(Math.Max(anchoDeseado, MinimumSize.Width), areaTrabajo.Width);
        int alto = Math.Min(760, areaTrabajo.Height);

        grilla.ScrollBars = ScrollBars.Vertical;
        ClientSize = new Size(ancho, Math.Max(MinimumSize.Height, alto));
        Location = new Point(
            areaTrabajo.Left + Math.Max(0, (areaTrabajo.Width - Width) / 2),
            areaTrabajo.Top + Math.Max(0, (areaTrabajo.Height - Height) / 2));
    }

    private void CargarFiltros()
    {
        cmbTipo.Items.Clear();
        cmbTipo.Items.Add(new Tipo { IdTipo = 0, Nombre = "Todos" });
        foreach (Tipo tipo in servicios.Repositorio.Datos.Tipos.OrderBy(tipo => tipo.Nombre))
        {
            cmbTipo.Items.Add(tipo);
        }
        cmbTipo.DisplayMember = "Nombre";
        cmbTipo.SelectedIndex = 0;
    }

    private void EnfocarCantidad(int rowIndex)
    {
        if (rowIndex < 0 || rowIndex >= grilla.Rows.Count || grilla.Rows[rowIndex].DataBoundItem is not ProductoResumen)
        {
            return;
        }

        BeginInvoke(() =>
        {
            if (IsDisposed || rowIndex >= grilla.Rows.Count)
            {
                return;
            }

            grilla.CurrentCell = grilla.Rows[rowIndex].Cells[ColumnaCantidad];
            grilla.BeginEdit(true);
        });
    }

    private void GrillaEditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs evento)
    {
        if (evento.Control is not TextBox editor)
        {
            return;
        }

        editor.KeyPress -= CantidadKeyPress;
        editor.KeyDown -= CantidadKeyDown;
        if (grilla.CurrentCell?.ColumnIndex == ColumnaCantidad)
        {
            editor.KeyPress += CantidadKeyPress;
            editor.KeyDown += CantidadKeyDown;
        }
    }

    private static void CantidadKeyPress(object? sender, KeyPressEventArgs evento)
    {
        if (!char.IsControl(evento.KeyChar) && !char.IsDigit(evento.KeyChar))
        {
            evento.Handled = true;
        }
    }

    private static void CantidadKeyDown(object? sender, KeyEventArgs evento)
    {
        if (!evento.Control || evento.KeyCode != Keys.V)
        {
            return;
        }

        string textoPegado = Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;
        if (string.IsNullOrEmpty(textoPegado) || textoPegado.Any(caracter => !char.IsDigit(caracter)))
        {
            evento.SuppressKeyPress = true;
        }
    }

    private void ActualizarGrilla()
    {
        int? idTipo = cmbTipo.SelectedItem is Tipo tipo && tipo.IdTipo != 0 ? tipo.IdTipo : null;
        FiltroEstadoProducto filtro = cmbEstado.SelectedIndex switch
        {
            1 => FiltroEstadoProducto.StockCritico,
            2 => FiltroEstadoProducto.Agotados,
            _ => FiltroEstadoProducto.Todos
        };
        grilla.DataSource = servicios.Productos.ObtenerProductos(txtBuscar.Text, idTipo, filtro).ToList();
        foreach (DataGridViewRow fila in grilla.Rows)
        {
            if (fila.DataBoundItem is not ProductoResumen resumen)
            {
                continue;
            }
            fila.Cells[ColumnaSalida].ReadOnly = resumen.StockActual == 0;
            fila.Cells[ColumnaSalida].Style.BackColor = resumen.StockActual == 0 ? Color.LightGray : Color.White;
        }
    }

    private void GrillaCellFormatting(object? sender, DataGridViewCellFormattingEventArgs evento)
    {
        if (evento.RowIndex < 0 || evento.ColumnIndex != ColumnaEstado ||
            grilla.Rows[evento.RowIndex].DataBoundItem is not ProductoResumen resumen)
        {
            return;
        }

        evento.CellStyle.ForeColor = resumen.Estado switch
        {
            EstadoProducto.Agotado => Color.Firebrick,
            EstadoProducto.StockCritico => Color.DarkGoldenrod,
            _ => Color.ForestGreen
        };
        evento.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        evento.FormattingApplied = true;
    }

    private void EjecutarAccionGrilla(DataGridViewCellEventArgs evento)
    {
        if (evento.RowIndex < 0 || evento.ColumnIndex < 0 || grilla.Rows[evento.RowIndex].DataBoundItem is not ProductoResumen resumen)
        {
            return;
        }

        string nombreColumna = grilla.Columns[evento.ColumnIndex].Name;
        if (nombreColumna != "Salida")
        {
            return;
        }

        RegistrarMovimiento(evento.RowIndex, "Salida");
    }

    private void RegistrarMovimientoSeleccionado(string movimiento)
    {
        if (grilla.CurrentCell is null)
        {
            MessageBox.Show(this, "Seleccione un producto.", "Movimiento", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        RegistrarMovimiento(grilla.CurrentCell.RowIndex, movimiento);
    }

    private void RegistrarMovimiento(int rowIndex, string movimiento)
    {
        if (rowIndex < 0 || rowIndex >= grilla.Rows.Count || grilla.Rows[rowIndex].DataBoundItem is not ProductoResumen resumen)
        {
            return;
        }

        int cantidad = 0;
        if (movimiento == "Salida")
        {
            if (!int.TryParse(Convert.ToString(grilla.Rows[rowIndex].Cells[ColumnaCantidad].Value), out cantidad) || cantidad <= 0)
            {
                MessageBox.Show(this, "Ingrese una cantidad mayor que cero.", "Cantidad inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show(this, $"¿Confirma salida de {cantidad} unidad(es) de {resumen.Descripcion}?", "Confirmar movimiento", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
        }

        try
        {
            int idUsuario = servicios.Auth.UsuarioActual!.IdUsuario;
            if (movimiento == "Entrada")
            {
                using var formularioRemito = new FormularioCargaRemito(
                    servicios.Productos.ObtenerProductos().ToList(),
                    servicios.Auth.UsuarioActual?.Nombre ?? string.Empty,
                    this);
                if (formularioRemito.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                servicios.Stock.RegistrarEntradas(
                    idUsuario,
                    formularioRemito.NumeroRemito,
                    formularioRemito.FechaRemito,
                    formularioRemito.Detalles
                        .Select(detalle => new EntradaStock(detalle.IdProducto, detalle.Cantidad))
                        .ToList());
            }
            else
            {
                servicios.Stock.RegistrarSalida(resumen.IdProducto, cantidad, idUsuario);
            }
            ActualizarGrilla();
        }
        catch (ReglaNegocioException excepcion)
        {
            MessageBox.Show(this, excepcion.Message, "Movimiento no registrado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception)
        {
            MessageBox.Show(this, "No se pudo guardar el movimiento.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AbrirNuevoProducto()
    {
        using var formulario = new FormularioNuevoProducto(servicios);
        if (formulario.ShowDialog(this) == DialogResult.OK)
        {
            CargarFiltros();
            ActualizarGrilla();
        }
    }

    private void AbrirPedidoNuevo()
    {
        using var formulario = new FormularioVistaPreviaPedido(servicios);
        formulario.ShowDialog(this);
        ActualizarGrilla();
    }

    private void Salir()
    {
        servicios.Auth.CerrarSesion();
        Close();
    }
}
