namespace pryHeladeriaBravoSegura.Forms;

public sealed class FormularioConfirmacionRemito : Form
{
    public FormularioConfirmacionRemito(
        string numeroRemito,
        DateTime fechaRemito,
        IReadOnlyList<FormularioCargaRemito.DetalleEntrada> detalles,
        IWin32Window? propietario = null)
    {
        Text = "Confirmar remito";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(620, 430);

        var encabezado = new Label
        {
            Dock = DockStyle.Top,
            Height = 62,
            Padding = new Padding(12, 12, 12, 6),
            Text = $"Remito: {numeroRemito}    Fecha: {fechaRemito:dd/MM/yyyy}\r\nDetalle de productos que ingresarán al stock",
            Font = new Font(Font, FontStyle.Bold)
        };

        var grilla = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeColumns = false,
            AllowUserToResizeRows = false,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
            AutoGenerateColumns = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        };
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "PRODUCTO", DataPropertyName = "Descripcion" });
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "CANTIDAD", DataPropertyName = "Cantidad", Width = 130 });
        grilla.DataSource = detalles
            .Select(detalle => new { detalle.Descripcion, detalle.Cantidad })
            .ToList();

        var botones = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 58,
            Padding = new Padding(12, 10, 12, 10),
            FlowDirection = FlowDirection.RightToLeft
        };
        var btnConfirmar = new Button { Text = "Confirmar remito", DialogResult = DialogResult.OK, AutoSize = true };
        var btnCancelar = new Button { Text = "Volver", DialogResult = DialogResult.Cancel, AutoSize = true };
        botones.Controls.Add(btnConfirmar);
        botones.Controls.Add(btnCancelar);

        Controls.Add(grilla);
        Controls.Add(botones);
        Controls.Add(encabezado);
        AcceptButton = btnConfirmar;
        CancelButton = btnCancelar;
    }
}
