using pryHeladeriaBravoSegura.Models;
using pryHeladeriaBravoSegura.Services;

namespace pryHeladeriaBravoSegura.Forms;

public sealed class FormularioNuevoProducto : Form
{
    private readonly ServiciosAplicacion servicios;
    private readonly TextBox txtDescripcion = new();
    private readonly ComboBox cmbTipo = new();
    private readonly NumericUpDown nudStockMinimo = new();

    public FormularioNuevoProducto(ServiciosAplicacion servicios)
    {
        this.servicios = servicios;
        Text = "Registrar producto faltante";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(460, 230);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ConstruirInterfaz();
    }

    private void ConstruirInterfaz()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 2,
            RowCount = 4
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        txtDescripcion.Dock = DockStyle.Fill;
        cmbTipo.Dock = DockStyle.Fill;
        cmbTipo.DisplayMember = "Nombre";
        cmbTipo.DataSource = servicios.Repositorio.Datos.Tipos.OrderBy(tipo => tipo.Nombre).ToList();
        nudStockMinimo.Dock = DockStyle.Left;
        nudStockMinimo.Minimum = 1;
        nudStockMinimo.Maximum = 1_000_000;
        nudStockMinimo.Value = 5;

        panel.Controls.Add(new Label { Text = "Descripción:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        panel.Controls.Add(txtDescripcion, 1, 0);
        panel.Controls.Add(new Label { Text = "Tipo:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        panel.Controls.Add(cmbTipo, 1, 1);
        panel.Controls.Add(new Label { Text = "Stock mínimo:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
        panel.Controls.Add(nudStockMinimo, 1, 2);

        var botones = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var btnCancelar = new Button { Text = "Cancelar", Width = 95, Height = 32, DialogResult = DialogResult.Cancel };
        var btnGuardar = new Button { Text = "Guardar", Width = 95, Height = 32 };
        botones.Controls.Add(btnCancelar);
        botones.Controls.Add(btnGuardar);
        panel.Controls.Add(botones, 1, 3);
        Controls.Add(panel);
        AcceptButton = btnGuardar;
        CancelButton = btnCancelar;
        Shown += (_, _) => txtDescripcion.Focus();
        btnGuardar.Click += (_, _) => Guardar();
    }

    private void Guardar()
    {
        try
        {
            if (cmbTipo.SelectedItem is not Tipo tipo)
            {
                throw new ReglaNegocioException("Seleccione un tipo de producto.");
            }

            servicios.Productos.CrearProductoFaltante(
                txtDescripcion.Text,
                tipo.IdTipo,
                (int)nudStockMinimo.Value);
            MessageBox.Show(this, "El producto fue registrado con stock inicial cero.", "Producto registrado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (ReglaNegocioException excepcion)
        {
            MessageBox.Show(this, excepcion.Message, "No se pudo registrar", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception)
        {
            MessageBox.Show(this, "No se pudo guardar el producto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
