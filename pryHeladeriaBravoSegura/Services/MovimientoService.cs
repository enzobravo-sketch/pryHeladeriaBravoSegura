using pryHeladeriaBravoSegura.Data;
using pryHeladeriaBravoSegura.Models;

namespace pryHeladeriaBravoSegura.Services;

public sealed class MovimientoService
{
    private readonly RepositorioJson repositorio;

    public MovimientoService(RepositorioJson repositorio)
    {
        this.repositorio = repositorio;
    }

    public IReadOnlyList<MovimientoResumen> ObtenerMovimientos(
        DateTime? desde = null,
        DateTime? hasta = null,
        int? idProducto = null,
        TipoMovimiento? tipo = null)
    {
        IEnumerable<MovimientoResumen> entradas = repositorio.Datos.Remitos
            .Join(
                repositorio.Datos.Productos,
                remito => remito.IdProducto,
                producto => producto.IdProducto,
                (remito, producto) => new { Remito = remito, Producto = producto })
            .Join(
                repositorio.Datos.Usuarios,
                elemento => elemento.Remito.IdUsuario,
                usuario => usuario.IdUsuario,
                (elemento, usuario) => new MovimientoResumen
                {
                    IdMovimiento = elemento.Remito.IdRemito,
                    IdProducto = elemento.Remito.IdProducto,
                    Fecha = elemento.Remito.Fecha,
                    DescripcionProducto = elemento.Producto.Descripcion,
                    Tipo = TipoMovimiento.Entrada,
                    Cantidad = elemento.Remito.Cantidad,
                    NombreUsuario = usuario.Nombre
                });

        IEnumerable<MovimientoResumen> salidas = repositorio.Datos.Salidas
            .Join(
                repositorio.Datos.Productos,
                salida => salida.IdProducto,
                producto => producto.IdProducto,
                (salida, producto) => new { Salida = salida, Producto = producto })
            .Join(
                repositorio.Datos.Usuarios,
                elemento => elemento.Salida.IdUsuario,
                usuario => usuario.IdUsuario,
                (elemento, usuario) => new MovimientoResumen
                {
                    IdMovimiento = elemento.Salida.IdSalida,
                    IdProducto = elemento.Salida.IdProducto,
                    Fecha = elemento.Salida.Fecha,
                    DescripcionProducto = elemento.Producto.Descripcion,
                    Tipo = TipoMovimiento.Salida,
                    Cantidad = elemento.Salida.Cantidad,
                    NombreUsuario = usuario.Nombre
                });

        return entradas
            .Concat(salidas)
            .Where(movimiento => EstaDentroDelRango(movimiento.Fecha, desde, hasta))
            .Where(movimiento => !idProducto.HasValue || movimiento.IdProducto == idProducto.Value)
            .Where(movimiento => !tipo.HasValue || movimiento.Tipo == tipo.Value)
            .OrderByDescending(movimiento => movimiento.Fecha)
            .ToList();
    }

    public void RevertirSalida(int idSalida, int idUsuarioAdministrador)
    {
        ValidarAdministrador(idUsuarioAdministrador);

        repositorio.EjecutarTransaccion(datos =>
        {
            Salida salida = datos.Salidas.FirstOrDefault(elemento => elemento.IdSalida == idSalida)
                ?? throw new ReglaNegocioException("La salida seleccionada ya no existe.");
            Stock stock = datos.Stocks.FirstOrDefault(elemento => elemento.IdStock == salida.IdStock)
                ?? throw new ReglaNegocioException("La salida no tiene un registro de stock válido.");

            stock.Cantidad += salida.Cantidad;
            datos.Salidas.Remove(salida);
        });
    }

    private void ValidarAdministrador(int idUsuario)
    {
        if (!repositorio.Datos.Usuarios.Any(usuario => usuario.IdUsuario == idUsuario && usuario.EsAdministrador))
        {
            throw new ReglaNegocioException("Solo un administrador puede deshacer salidas.");
        }
    }

    private static bool EstaDentroDelRango(DateTime fecha, DateTime? desde, DateTime? hasta)
    {
        DateTime inicio = desde?.Date ?? DateTime.MinValue;
        DateTime finExclusivo = hasta?.Date.AddDays(1) ?? DateTime.MaxValue;
        return fecha >= inicio && fecha < finExclusivo;
    }
}
