using pryHeladeriaBravoSegura.Models;
using pryHeladeriaBravoSegura.Services;

namespace pryHeladeriaBravoSegura.Forms;

public sealed class FormularioUsuarios : Form
{
    private readonly ServiciosAplicacion servicios;
    private readonly TextBox txtNombre = new();
    private readonly TextBox txtContrasena = new();
    private readonly CheckBox chkAdministrador = new() { Text = "Administrador" };
    private readonly DataGridView grilla = new();

    public FormularioUsuarios(ServiciosAplicacion servicios)
    {
        this.servicios = servicios;
        Text = "Gestión de usuarios";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(620, 420);
        MinimizeBox = false;
        ConstruirInterfaz();
        ActualizarGrilla();
    }

    private void ConstruirInterfaz()
    {
        var alta = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 115,
            Padding = new Padding(12),
            ColumnCount = 3,
            RowCount = 3
        };
        alta.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
        alta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        alta.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        alta.Controls.Add(new Label { Text = "Nombre:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
        alta.Controls.Add(txtNombre, 1, 0);
        alta.Controls.Add(new Label { Text = "Contraseña:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
        alta.Controls.Add(txtContrasena, 1, 1);
        alta.Controls.Add(chkAdministrador, 1, 2);
        var btnCrear = new Button { Text = "Crear usuario", Dock = DockStyle.Fill };
        alta.Controls.Add(btnCrear, 2, 0);
        alta.SetRowSpan(btnCrear, 3);

        grilla.Dock = DockStyle.Fill;
        grilla.ReadOnly = true;
        grilla.AllowUserToAddRows = false;
        grilla.AllowUserToDeleteRows = false;
        grilla.RowHeadersVisible = false;
        grilla.AutoGenerateColumns = false;
        grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grilla.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "USUARIO", DataPropertyName = "Nombre", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        grilla.Columns.Add(new DataGridViewCheckBoxColumn { HeaderText = "ADMINISTRADOR", DataPropertyName = "EsAdministrador", Width = 120 });

        var inferior = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(12, 8, 12, 8) };
        var btnEliminar = new Button { Text = "Eliminar seleccionado", AutoSize = true };
        inferior.Controls.Add(btnEliminar);
        Controls.Add(grilla);
        Controls.Add(inferior);
        Controls.Add(alta);

        txtContrasena.UseSystemPasswordChar = true;
        btnCrear.Click += (_, _) => CrearUsuario();
        btnEliminar.Click += (_, _) => EliminarUsuario();
    }

    private void ActualizarGrilla()
    {
        int idAdministrador = servicios.Auth.UsuarioActual!.IdUsuario;
        grilla.DataSource = servicios.Usuarios.ObtenerUsuarios(idAdministrador).ToList();
    }

    private void CrearUsuario()
    {
        try
        {
            servicios.Usuarios.CrearUsuario(
                servicios.Auth.UsuarioActual!.IdUsuario,
                txtNombre.Text,
                txtContrasena.Text,
                chkAdministrador.Checked);
            txtNombre.Clear();
            txtContrasena.Clear();
            chkAdministrador.Checked = false;
            ActualizarGrilla();
            MessageBox.Show(this, "Usuario creado correctamente.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (ReglaNegocioException excepcion)
        {
            MessageBox.Show(this, excepcion.Message, "No se pudo crear el usuario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void EliminarUsuario()
    {
        if (grilla.CurrentRow?.DataBoundItem is not Usuario usuario)
        {
            MessageBox.Show(this, "Seleccione un usuario.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (MessageBox.Show(this, $"¿Desea eliminar a {usuario.Nombre}?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        try
        {
            servicios.Usuarios.EliminarUsuario(servicios.Auth.UsuarioActual!.IdUsuario, usuario.IdUsuario);
            ActualizarGrilla();
        }
        catch (ReglaNegocioException excepcion)
        {
            MessageBox.Show(this, excepcion.Message, "No se pudo eliminar el usuario", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
