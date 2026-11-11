using pryHeladeriaBravoSegura.Models;
using pryHeladeriaBravoSegura.Services;

namespace pryHeladeriaBravoSegura.Forms;

public sealed class FormularioVistaPreviaPedido : Form
{
    private readonly ServiciosAplicacion servicios;
    private readonly DataGridView grilla = new();
    private readonly TextBox txtObservaciones = new();
    private readonly Button btnConfirmar = new() { Text = "Confirmar pedido", Width = 135, Height = 34 };
    private readonly Button btnCopiar = new() { Text = "Copiar para WhatsApp", Width = 145, Height = 34, Enabled = false };
    private readonly Button btnAbrir = new() { Text = "Abrir WhatsApp Web", Width = 145, Height = 34, Enabled = false };
    private Pedido? pedidoConfirmado;

    public FormularioVistaPreviaPedido(ServiciosAplicacion servicios)
    {
        this.servicios = servicios;
        Text = "Vista previa del pedido";
        StartPosition = FormStartPosition.CenterParent;
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(800, 500);
        ConstruirInterfaz();
        CargarPropuesta();
    }

    private void ConstruirInterfaz()
    {
        var encabezado = new Label
        {
            Text = "Productos sugeridos para pedir al proveedor",
            Dock = DockStyle.Top,
            Height = 42,
            Font = new Font(Font.FontFamily, 13, FontStyle.Bold),
            Padding = new Padding(12, 10, 0, 0)
        };
        PrepararGrilla();
        var observaciones = new Panel { Dock = DockStyle.Bottom, Height = 65, Padding = new Padding(12, 8, 12, 8) };
        var lbl = new Label { Text = "Observaciones:", AutoSize = true, Location = new Point(12, 15) };
        txtObservaciones.Location = new Point(115, 10);
        txtObservaciones.Width = 500;
        txtObservaciones.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        observaciones.Controls.Add(lbl);
        observaciones.Controls.Add(txtObservaciones);
        var botones = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 55, Padding = new Padding(12, 8, 12, 8), FlowDirection = FlowDirection.RightToLeft };
        botones.Controls.Add(btnAbrir);
        botones.Controls.Add(btnCopiar);
        botones.Controls.Add(btnConfirmar);

        Controls.Add(grilla);
        Controls.Add(botones);
        Controls.Add(observaciones);
        Controls.Add(encabezado);
        btnConfirmar.Click += (_, _) => Confirmar();
        btnCopiar.Click += (_, _) => Copiar();
        btnAbrir.Click += (_, _) => AbrirWhatsApp();
    }

    private void PrepararGrilla()
    {
        grilla.Dock = DockStyle.Fill;
        grilla.AllowUserToAddRows = false;
        grilla.AllowUserToResizeColumns = false;
        grilla.AllowUserToResizeRows = false;
        grilla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grilla.AutoGenerateColumns = false;
        grilla.RowHeadersVisible = false;
        grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = "Descripcion", HeaderText = "PRODUCTO", DataPropertyName = "Descripcion", ReadOnly = true });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = "StockActual", HeaderText = "STOCK ACTUAL", DataPropertyName = "StockActual", ReadOnly = true });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = "StockMinimo", HeaderText = "STOCK MÍNIMO", DataPropertyName = "StockMinimo", ReadOnly = true });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { Name = "CantAPedir", HeaderText = "CANTIDAD A PEDIR", DataPropertyName = "CantAPedir" });
    }

    private void CargarPropuesta()
    {
        List<DetallePedidoPropuesto> propuesta = servicios.Pedidos.GenerarPropuesta().ToList();
        grilla.DataSource = propuesta;
        btnConfirmar.Enabled = propuesta.Count > 0;
        if (propuesta.Count == 0)
        {
            MessageBox.Show(this, "No hay productos en estado crítico o agotado para pedir.", "Pedido", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void Confirmar()
    {
        try
        {
            grilla.EndEdit();
            var propuesta = grilla.Rows
                .Cast<DataGridViewRow>()
                .Where(fila => fila.DataBoundItem is DetallePedidoPropuesto)
                .Select(fila =>
                {
                    var detalle = (DetallePedidoPropuesto)fila.DataBoundItem!;
                    if (int.TryParse(Convert.ToString(fila.Cells["CantAPedir"].Value), out int cantidad))
                    {
                        detalle.CantAPedir = cantidad;
                    }
                    return detalle;
                })
                .ToList();

            if (MessageBox.Show(this, "¿Confirma guardar este pedido?", "Confirmar pedido", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            pedidoConfirmado = servicios.Pedidos.ConfirmarPedido(
                propuesta,
                txtObservaciones.Text,
                servicios.Auth.UsuarioActual!.IdUsuario);
            btnConfirmar.Enabled = false;
            btnCopiar.Enabled = true;
            btnAbrir.Enabled = true;
            MessageBox.Show(this, "El pedido fue guardado correctamente.", "Pedido confirmado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (ReglaNegocioException excepcion)
        {
            MessageBox.Show(this, excepcion.Message, "No se pudo confirmar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception)
        {
            MessageBox.Show(this, "No se pudo guardar el pedido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Copiar()
    {
        if (pedidoConfirmado is null)
        {
            return;
        }
        Clipboard.SetText(servicios.Pedidos.GenerarTextoWhatsapp(pedidoConfirmado));
        MessageBox.Show(this, "El pedido fue copiado al portapapeles.", "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void AbrirWhatsApp()
    {
        if (pedidoConfirmado is null)
        {
            return;
        }
        string enlace = servicios.Pedidos.GenerarEnlaceWhatsApp(pedidoConfirmado);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(enlace) { UseShellExecute = true });
    }
}
