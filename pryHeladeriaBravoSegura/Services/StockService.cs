using pryHeladeriaBravoSegura.Data;
using pryHeladeriaBravoSegura.Models;

namespace pryHeladeriaBravoSegura.Services;

public sealed class StockService
{
    private readonly RepositorioJson repositorio;

    public StockService(RepositorioJson repositorio)
    {
        this.repositorio = repositorio;
    }

    public Remito RegistrarEntrada(int idProducto, int cantidad, int idUsuario, string numeroRemito, DateTime fechaRemito)
    {
        return RegistrarEntradas(
                idUsuario,
                numeroRemito,
                fechaRemito,
                [new EntradaStock(idProducto, cantidad)])
            .Single();
    }

    public IReadOnlyList<Remito> RegistrarEntradas(
        int idUsuario,
        string numeroRemito,
        DateTime fechaRemito,
        IReadOnlyList<EntradaStock> entradas)
    {
        ValidarUsuario(idUsuario);
        if (string.IsNullOrWhiteSpace(numeroRemito))
        {
            throw new ReglaNegocioException("El número de remito es obligatorio.");
        }

        if (entradas.Count == 0 || entradas.Any(entrada => entrada.Cantidad <= 0))
        {
            throw new ReglaNegocioException("Todas las cantidades del remito deben ser mayores que cero.");
        }

        List<Remito> remitosCreados = [];
        repositorio.EjecutarTransaccion(datos =>
        {
            foreach (EntradaStock entrada in entradas)
            {
                ValidarCantidad(entrada.Cantidad);
                Producto producto = ObtenerProducto(datos, entrada.IdProducto);
                Stock stock = ObtenerStock(datos, entrada.IdProducto);
                stock.Cantidad += entrada.Cantidad;

                Remito remito = new()
                {
                    IdRemito = repositorio.ObtenerProximoId(datos.Remitos, elemento => elemento.IdRemito),
                    NumeroRemito = numeroRemito.Trim(),
                    FechaRemito = fechaRemito.Date,
                    IdStock = stock.IdStock,
                    IdProducto = producto.IdProducto,
                    IdUsuario = idUsuario,
                    Fecha = DateTime.Now,
                    Cantidad = entrada.Cantidad
                };
                datos.Remitos.Add(remito);
                remitosCreados.Add(remito);
            }
        });

        return remitosCreados;
    }

    public Salida RegistrarSalida(int idProducto, int cantidad, int idUsuario)
    {
        ValidarCantidad(cantidad);
        ValidarUsuario(idUsuario);

        Salida? salidaCreada = null;
        repositorio.EjecutarTransaccion(datos =>
        {
            Producto producto = ObtenerProducto(datos, idProducto);
            Stock stock = ObtenerStock(datos, idProducto);

            if (stock.Cantidad == 0)
            {
                throw new ReglaNegocioException("No se puede registrar una salida porque el stock está agotado.");
            }

            if (cantidad > stock.Cantidad)
            {
                throw new ReglaNegocioException("La cantidad de salida no puede superar el stock disponible.");
            }

            stock.Cantidad -= cantidad;
            salidaCreada = new Salida
            {
                IdSalida = repositorio.ObtenerProximoId(datos.Salidas, salida => salida.IdSalida),
                IdProducto = producto.IdProducto,
                IdUsuario = idUsuario,
                IdStock = stock.IdStock,
                Cantidad = cantidad,
                Fecha = DateTime.Now
            };
            datos.Salidas.Add(salidaCreada);
        });

        return salidaCreada!;
    }

    public int ObtenerStockActual(int idProducto)
    {
        return ObtenerStock(repositorio.Datos, idProducto).Cantidad;
    }

    private static void ValidarCantidad(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ReglaNegocioException("La cantidad debe ser mayor que cero.");
        }
    }

    private void ValidarUsuario(int idUsuario)
    {
        if (!repositorio.Datos.Usuarios.Any(usuario => usuario.IdUsuario == idUsuario))
        {
            throw new ReglaNegocioException("La sesión del usuario no es válida.");
        }
    }

    private static Producto ObtenerProducto(DatosApp datos, int idProducto)
    {
        return datos.Productos.FirstOrDefault(producto => producto.IdProducto == idProducto)
            ?? throw new ReglaNegocioException("El producto seleccionado no existe en el catálogo.");
    }

    private static Stock ObtenerStock(DatosApp datos, int idProducto)
    {
        return datos.Stocks.FirstOrDefault(stock => stock.IdProducto == idProducto)
            ?? throw new ReglaNegocioException("El producto no tiene un registro de stock válido.");
    }
}

public sealed record EntradaStock(int IdProducto, int Cantidad);
