using pryHeladeriaBravoSegura.Services;

namespace pryHeladeriaBravoSegura.Forms;

public sealed class FormularioLogin : Form
{
    private readonly ServiciosAplicacion servicios;
    private readonly TextBox txtUsuario = new();
    private readonly TextBox txtContrasena = new();
    private readonly Label lblError = new();

    public FormularioLogin(ServiciosAplicacion servicios)
    {
        this.servicios = servicios;
        Text = "GLUP'S - Iniciar sesión";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(420, 300);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var titulo = new Label
        {
            Text = "GLUP'S\nControl y Monitoreo de Stock",
            Dock = DockStyle.Top,
            Height = 90,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font(Font.FontFamily, 16, FontStyle.Bold)
        };
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(45, 10, 45, 20),
            ColumnCount = 2,
            RowCount = 4
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        txtUsuario.Dock = DockStyle.Fill;
        txtContrasena.Dock = DockStyle.Fill;
        txtContrasena.UseSystemPasswordChar = true;
        var btnIngresar = new Button { Text = "Ingresar", Dock = DockStyle.Fill, Height = 35 };
        lblError.ForeColor = Color.Firebrick;
        lblError.AutoSize = true;

        panel.Controls.Add(new Label { Text = "Usuario:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
        panel.Controls.Add(txtUsuario, 1, 0);
        panel.Controls.Add(new Label { Text = "Contraseña:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
        panel.Controls.Add(txtContrasena, 1, 1);
        panel.Controls.Add(btnIngresar, 1, 2);
        panel.Controls.Add(lblError, 1, 3);
        Controls.Add(panel);
        Controls.Add(titulo);

        AcceptButton = btnIngresar;
        btnIngresar.Click += (_, _) => Ingresar();
        Shown += (_, _) => txtUsuario.Focus();
    }

    private void Ingresar()
    {
        try
        {
            servicios.Auth.IniciarSesion(txtUsuario.Text, txtContrasena.Text);
            Hide();
            using var principal = new FormularioPrincipal(servicios);
            principal.ShowDialog(this);
            if (servicios.Auth.UsuarioActual is null)
            {
                txtContrasena.Clear();
                Show();
                txtUsuario.Focus();
            }
            else
            {
                Close();
            }
        }
        catch (ReglaNegocioException excepcion)
        {
            lblError.Text = excepcion.Message;
            txtContrasena.SelectAll();
            txtContrasena.Focus();
        }
        catch (Exception)
        {
            lblError.Text = "No se pudo iniciar la aplicación. Verifique los datos almacenados.";
        }
    }
}
