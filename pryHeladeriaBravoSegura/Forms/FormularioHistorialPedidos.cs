using pryHeladeriaBravoSegura.Models;
using pryHeladeriaBravoSegura.Services;

namespace pryHeladeriaBravoSegura.Forms;

public sealed class FormularioHistorialPedidos : Form
{
    private readonly ServiciosAplicacion servicios;
    private readonly DateTimePicker dtpDesde = CrearFecha();
    private readonly DateTimePicker dtpHasta = CrearFecha();
    private readonly DataGridView grilla = new();

    public FormularioHistorialPedidos(ServiciosAplicacion servicios)
    {
        this.servicios = servicios;
        Text = "Historial de pedidos";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;
        ConstruirInterfaz();
        Actualizar();
    }

    private void ConstruirInterfaz()
    {
        var filtros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(12, 10, 12, 5), WrapContents = false };
        var btnFiltrar = new Button { Text = "Filtrar", Width = 90, Height = 30 };
        filtros.Controls.Add(new Label { Text = "Desde:", AutoSize = true, Padding = new Padding(0, 7, 0, 0) });
        filtros.Controls.Add(dtpDesde);
        filtros.Controls.Add(new Label { Text = "Hasta:", AutoSize = true, Padding = new Padding(12, 7, 0, 0) });
        filtros.Controls.Add(dtpHasta);
        filtros.Controls.Add(btnFiltrar);
        PrepararGrilla();
        var btnDetalle = new Button { Text = "Ver detalle", Dock = DockStyle.Bottom, Height = 40 };
        Controls.Add(grilla);
        Controls.Add(btnDetalle);
        Controls.Add(filtros);
        btnFiltrar.Click += (_, _) => Actualizar();
        btnDetalle.Click += (_, _) => VerDetalle();
        grilla.CellDoubleClick += (_, _) => VerDetalle();
    }

    private void PrepararGrilla()
    {
        grilla.Dock = DockStyle.Fill;
        grilla.AllowUserToAddRows = false;
        grilla.AllowUserToResizeColumns = false;
        grilla.AllowUserToResizeRows = false;
        grilla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grilla.ReadOnly = true;
        grilla.AutoGenerateColumns = false;
        grilla.RowHeadersVisible = false;
        grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = "IdPedido", DataPropertyName = "IdPedido", Visible = false });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "FECHA", DataPropertyName = "Fecha" });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "USUARIO", DataPropertyName = "NombreUsuario" });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "OBSERVACIONES", DataPropertyName = "Observaciones" });
    }

    private void Actualizar()
    {
        DateTime? desde = dtpDesde.Checked ? dtpDesde.Value.Date : null;
        DateTime? hasta = dtpHasta.Checked ? dtpHasta.Value.Date : null;
        var filas = servicios.Pedidos.ObtenerPedidos(desde, hasta)
            .Select(pedido => new PedidoFila
            {
                IdPedido = pedido.IdPedido,
                Fecha = pedido.Fecha.ToString("dd/MM/yyyy HH:mm"),
                NombreUsuario = servicios.Repositorio.Datos.Usuarios.FirstOrDefault(usuario => usuario.IdUsuario == pedido.IdUsuario)?.Nombre ?? "Desconocido",
                Observaciones = pedido.Observaciones
            })
            .ToList();
        grilla.DataSource = filas;
    }

    private void VerDetalle()
    {
        if (grilla.CurrentRow?.DataBoundItem is not PedidoFila fila)
        {
            return;
        }
        var pedido = servicios.Repositorio.Datos.Pedidos.First(pedido => pedido.IdPedido == fila.IdPedido);
        var detalles = servicios.Pedidos.ObtenerDetalles(pedido.IdPedido)
            .Join(servicios.Repositorio.Datos.Productos, detalle => detalle.IdProducto, producto => producto.IdProducto, (detalle, producto) => $"- {producto.Descripcion}: {detalle.CantAPedir}");
        MessageBox.Show(this, string.Join(Environment.NewLine, detalles), $"Detalle del pedido #{pedido.IdPedido}", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static DateTimePicker CrearFecha()
    {
        return new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 105, ShowCheckBox = true, Checked = false };
    }

    private sealed class PedidoFila
    {
        public int IdPedido { get; init; }
        public string Fecha { get; init; } = string.Empty;
        public string NombreUsuario { get; init; } = string.Empty;
        public string Observaciones { get; init; } = string.Empty;
    }
}
