using pryHeladeriaBravoSegura.Data;
using pryHeladeriaBravoSegura.Models;

namespace pryHeladeriaBravoSegura.Services;

public sealed class UsuarioService
{
    private readonly RepositorioJson repositorio;

    public UsuarioService(RepositorioJson repositorio)
    {
        this.repositorio = repositorio;
    }

    public IReadOnlyList<Usuario> ObtenerUsuarios(int idUsuarioAdministrador)
    {
        ValidarAdministrador(idUsuarioAdministrador);
        return repositorio.Datos.Usuarios.OrderBy(usuario => usuario.Nombre).ToList();
    }

    public Usuario CrearUsuario(int idUsuarioAdministrador, string nombre, string contrasena, bool esAdministrador)
    {
        ValidarAdministrador(idUsuarioAdministrador);
        string nombreNormalizado = nombre?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(nombreNormalizado) || string.IsNullOrWhiteSpace(contrasena))
        {
            throw new ReglaNegocioException("El usuario y la contraseña son obligatorios.");
        }

        if (repositorio.Datos.Usuarios.Any(usuario => string.Equals(usuario.Nombre, nombreNormalizado, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ReglaNegocioException("Ya existe un usuario con ese nombre.");
        }

        Usuario nuevoUsuario = new();
        repositorio.EjecutarTransaccion(datos =>
        {
            nuevoUsuario = new Usuario
            {
                IdUsuario = repositorio.ObtenerProximoId(datos.Usuarios, usuario => usuario.IdUsuario),
                Nombre = nombreNormalizado,
                Contrasena = SemillaDatos.CrearHash(contrasena),
                EsAdministrador = esAdministrador
            };
            datos.Usuarios.Add(nuevoUsuario);
        });
        return nuevoUsuario;
    }

    public void EliminarUsuario(int idUsuarioAdministrador, int idUsuarioAEliminar)
    {
        ValidarAdministrador(idUsuarioAdministrador);
        if (idUsuarioAdministrador == idUsuarioAEliminar)
        {
            throw new ReglaNegocioException("No puede eliminar el usuario con el que inició sesión.");
        }

        repositorio.EjecutarTransaccion(datos =>
        {
            Usuario usuario = datos.Usuarios.FirstOrDefault(elemento => elemento.IdUsuario == idUsuarioAEliminar)
                ?? throw new ReglaNegocioException("El usuario seleccionado no existe.");
            if (usuario.EsAdministrador && datos.Usuarios.Count(elemento => elemento.EsAdministrador) <= 1)
            {
                throw new ReglaNegocioException("Debe existir al menos un administrador.");
            }
            datos.Usuarios.Remove(usuario);
        });
    }

    private void ValidarAdministrador(int idUsuario)
    {
        if (!repositorio.Datos.Usuarios.Any(usuario => usuario.IdUsuario == idUsuario && usuario.EsAdministrador))
        {
            throw new ReglaNegocioException("Solo un administrador puede gestionar usuarios.");
        }
    }
}