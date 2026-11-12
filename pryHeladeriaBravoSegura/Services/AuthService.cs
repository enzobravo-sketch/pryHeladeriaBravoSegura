using System.Security.Cryptography;
using pryHeladeriaBravoSegura.Data;
using pryHeladeriaBravoSegura.Models;

namespace pryHeladeriaBravoSegura.Services;

public sealed class AuthService
{
    private const int Iteraciones = 100_000;
    private readonly RepositorioJson repositorio;

    public Usuario? UsuarioActual { get; private set; }

    public bool EsAdministrador => UsuarioActual?.EsAdministrador == true;

    public AuthService(RepositorioJson repositorio)
    {
        this.repositorio = repositorio;
    }

    public Usuario IniciarSesion(string nombre, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrEmpty(contrasena))
        {
            throw new ReglaNegocioException("Ingrese el usuario y la contraseña.");
        }

        Usuario? usuario = repositorio.Datos.Usuarios.FirstOrDefault(
            elemento => string.Equals(elemento.Nombre, nombre.Trim(), StringComparison.OrdinalIgnoreCase));

        if (usuario is null || !VerificarContrasena(contrasena, usuario.Contrasena))
        {
            throw new ReglaNegocioException("El usuario o la contraseña son incorrectos.");
        }

        UsuarioActual = usuario;
        return usuario;
    }

    public void CerrarSesion()
    {
        UsuarioActual = null;
    }

    private static bool VerificarContrasena(string contrasena, string valorAlmacenado)
    {
        string[] partes = valorAlmacenado.Split(':', 2);
        if (partes.Length != 2)
        {
            return false;
        }

        try
        {
            byte[] sal = Convert.FromBase64String(partes[0]);
            byte[] hashEsperado = Convert.FromBase64String(partes[1]);
            byte[] hashCalculado = Rfc2898DeriveBytes.Pbkdf2(
                contrasena,
                sal,
                Iteraciones,
                HashAlgorithmName.SHA256,
                hashEsperado.Length);

            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
