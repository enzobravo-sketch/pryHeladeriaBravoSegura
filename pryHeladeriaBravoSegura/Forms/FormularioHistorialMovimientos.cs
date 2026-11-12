using pryHeladeriaBravoSegura.Models;
using pryHeladeriaBravoSegura.Services;

namespace pryHeladeriaBravoSegura.Forms;

public sealed class FormularioHistorialMovimientos : Form
{
    private readonly ServiciosAplicacion servicios;
    private readonly DateTimePicker dtpDesde = CrearFecha();
    private readonly DateTimePicker dtpHasta = CrearFecha();
    private readonly ComboBox cmbProducto = new();
    private readonly ComboBox cmbTipo = new();
    private readonly DataGridView grilla = new();

    public FormularioHistorialMovimientos(ServiciosAplicacion servicios)
    {
        this.servicios = servicios;
        Text = "Historial de movimientos";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;
        ConstruirInterfaz();
        CargarFiltros();
        Actualizar();
    }

    private void ConstruirInterfaz()
    {
        var filtros = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 55, Padding = new Padding(12, 10, 12, 5), WrapContents = false };
        var btnFiltrar = new Button { Text = "Filtrar", Width = 90, Height = 30 };
        cmbProducto.Width = 190;
        cmbTipo.Width = 120;
        filtros.Controls.Add(new Label { Text = "Desde:", AutoSize = true, Padding = new Padding(0, 7, 0, 0) });
        filtros.Controls.Add(dtpDesde);
        filtros.Controls.Add(new Label { Text = "Hasta:", AutoSize = true, Padding = new Padding(12, 7, 0, 0) });
        filtros.Controls.Add(dtpHasta);
        filtros.Controls.Add(new Label { Text = "Producto:", AutoSize = true, Padding = new Padding(12, 7, 0, 0) });
        filtros.Controls.Add(cmbProducto);
        filtros.Controls.Add(new Label { Text = "Tipo:", AutoSize = true, Padding = new Padding(12, 7, 0, 0) });
        filtros.Controls.Add(cmbTipo);
        filtros.Controls.Add(btnFiltrar);
        PrepararGrilla();
        Controls.Add(grilla);
        Controls.Add(filtros);
        btnFiltrar.Click += (_, _) => Actualizar();
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
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "FECHA Y HORA", DataPropertyName = "Fecha" });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PRODUCTO", DataPropertyName = "DescripcionProducto" });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "TIPO", DataPropertyName = "Tipo" });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CANTIDAD", DataPropertyName = "Cantidad" });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "USUARIO", DataPropertyName = "NombreUsuario" });
        if (servicios.Auth.EsAdministrador)
        {
            grilla.Columns.Add(new DataGridViewButtonColumn { Name = "Deshacer", HeaderText = "ACCIÓN", Text = "Deshacer", UseColumnTextForButtonValue = true, Width = 100 });
            grilla.CellContentClick += (_, evento) => Deshacer(evento);
        }
    }

    private void CargarFiltros()
    {
        cmbProducto.Items.Add(new Producto { IdProducto = 0, Descripcion = "Todos" });
        foreach (Producto producto in servicios.Repositorio.Datos.Productos.OrderBy(producto => producto.Descripcion))
        {
            cmbProducto.Items.Add(producto);
        }
        cmbProducto.DisplayMember = "Descripcion";
        cmbProducto.SelectedIndex = 0;
        cmbTipo.Items.Add("Todos");
        cmbTipo.Items.Add("Entrada");
        cmbTipo.Items.Add("Salida");
        cmbTipo.SelectedIndex = 0;
    }

    private void Actualizar()
    {
        int? idProducto = cmbProducto.SelectedItem is Producto producto && producto.IdProducto != 0 ? producto.IdProducto : null;
        TipoMovimiento? tipo = cmbTipo.SelectedIndex switch
        {
            1 => TipoMovimiento.Entrada,
            2 => TipoMovimiento.Salida,
            _ => null
        };
        var filas = servicios.Movimientos.ObtenerMovimientos(
                dtpDesde.Checked ? dtpDesde.Value.Date : null,
                dtpHasta.Checked ? dtpHasta.Value.Date : null,
                idProducto,
                tipo)
            .ToList();
        grilla.DataSource = filas;
    }

    private void Deshacer(DataGridViewCellEventArgs evento)
    {
        if (evento.RowIndex < 0 || evento.ColumnIndex < 0 ||
            grilla.Columns[evento.ColumnIndex].Name != "Deshacer" ||
            grilla.Rows[evento.RowIndex].DataBoundItem is not MovimientoResumen movimiento ||
            movimiento.Tipo != TipoMovimiento.Salida)
        {
            return;
        }

        if (MessageBox.Show(this, $"¿Deshacer la salida de {movimiento.Cantidad} unidad(es) de {movimiento.DescripcionProducto}?", "Confirmar reversión", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            servicios.Movimientos.RevertirSalida(movimiento.IdMovimiento, servicios.Auth.UsuarioActual!.IdUsuario);
            Actualizar();
        }
        catch (ReglaNegocioException excepcion)
        {
            MessageBox.Show(this, excepcion.Message, "No se pudo deshacer la salida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private static DateTimePicker CrearFecha()
    {
        return new DateTimePicker { Format = DateTimePickerFormat.Short, Width = 105, ShowCheckBox = true, Checked = false };
    }
}
