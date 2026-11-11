using pryHeladeriaBravoSegura.Data;
using pryHeladeriaBravoSegura.Models;

namespace pryHeladeriaBravoSegura.Services;

public sealed class PedidoService
{
    private readonly RepositorioJson repositorio;

    public PedidoService(RepositorioJson repositorio)
    {
        this.repositorio = repositorio;
    }

    public IReadOnlyList<DetallePedidoPropuesto> GenerarPropuesta()
    {
        return repositorio.Datos.Productos
            .Join(
                repositorio.Datos.Stocks,
                producto => producto.IdProducto,
                stock => stock.IdProducto,
                (producto, stock) => new { Producto = producto, Stock = stock })
            .Where(elemento => ProductoService.CalcularEstado(
                elemento.Stock.Cantidad,
                elemento.Producto.StockMinimo) is EstadoProducto.Agotado or EstadoProducto.StockCritico)
            .OrderBy(elemento => elemento.Producto.Descripcion)
            .Select(elemento => new DetallePedidoPropuesto
            {
                IdProducto = elemento.Producto.IdProducto,
                Descripcion = elemento.Producto.Descripcion,
                StockMinimo = elemento.Producto.StockMinimo,
                StockActual = elemento.Stock.Cantidad,
                CantAPedir = elemento.Producto.StockMinimo * 2 - elemento.Stock.Cantidad
            })
            .ToList();
    }

    public Pedido ConfirmarPedido(
        IEnumerable<DetallePedidoPropuesto> propuesta,
        string? observaciones,
        int idUsuario)
    {
        List<DetallePedidoPropuesto> detalles = propuesta?.ToList()
            ?? throw new ReglaNegocioException("No se recibió una propuesta de pedido.");

        if (detalles.Count == 0)
        {
            throw new ReglaNegocioException("No hay productos faltantes para generar el pedido.");
        }

        if (!repositorio.Datos.Usuarios.Any(usuario => usuario.IdUsuario == idUsuario))
        {
            throw new ReglaNegocioException("La sesión del usuario no es válida.");
        }

        if (detalles.Any(detalle => detalle.CantAPedir <= 0))
        {
            throw new ReglaNegocioException("Todas las cantidades del pedido deben ser mayores que cero.");
        }

        if (detalles.GroupBy(detalle => detalle.IdProducto).Any(grupo => grupo.Count() > 1))
        {
            throw new ReglaNegocioException("El pedido no puede contener productos repetidos.");
        }

        Pedido? pedidoCreado = null;
        repositorio.EjecutarTransaccion(datos =>
        {
            foreach (DetallePedidoPropuesto detalle in detalles)
            {
                if (!datos.Productos.Any(producto => producto.IdProducto == detalle.IdProducto))
                {
                    throw new ReglaNegocioException("Uno de los productos del pedido ya no existe en el catálogo.");
                }
            }

            pedidoCreado = new Pedido
            {
                IdPedido = repositorio.ObtenerProximoId(datos.Pedidos, pedido => pedido.IdPedido),
                Fecha = DateTime.Now,
                IdUsuario = idUsuario,
                Observaciones = observaciones?.Trim() ?? string.Empty
            };
            datos.Pedidos.Add(pedidoCreado);

            foreach (DetallePedidoPropuesto detalle in detalles)
            {
                datos.DetallesPedido.Add(new DetallePedido
                {
                    IdDetalle = repositorio.ObtenerProximoId(
                        datos.DetallesPedido,
                        detalleGuardado => detalleGuardado.IdDetalle),
                    IdPedido = pedidoCreado.IdPedido,
                    IdProducto = detalle.IdProducto,
                    CantAPedir = detalle.CantAPedir
                });
            }
        });

        return pedidoCreado!;
    }

    public IReadOnlyList<Pedido> ObtenerPedidos(DateTime? desde = null, DateTime? hasta = null)
    {
        return repositorio.Datos.Pedidos
            .Where(pedido => EstaDentroDelRango(pedido.Fecha, desde, hasta))
            .OrderByDescending(pedido => pedido.Fecha)
            .ToList();
    }

    public IReadOnlyList<DetallePedido> ObtenerDetalles(int idPedido)
    {
        return repositorio.Datos.DetallesPedido
            .Where(detalle => detalle.IdPedido == idPedido)
            .ToList();
    }

    public string GenerarTextoWhatsapp(Pedido pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        var detalles = repositorio.Datos.DetallesPedido
            .Where(detalle => detalle.IdPedido == pedido.IdPedido)
            .Join(
                repositorio.Datos.Productos,
                detalle => detalle.IdProducto,
                producto => producto.IdProducto,
                (detalle, producto) => new { detalle.CantAPedir, producto.Descripcion })
            .OrderBy(elemento => elemento.Descripcion)
            .ToList();

        if (detalles.Count == 0)
        {
            throw new ReglaNegocioException("El pedido no tiene detalles para enviar.");
        }

        var lineas = new List<string>
        {
            $"Pedido Glup's - {pedido.Fecha:dd/MM/yyyy HH:mm}",
            string.Empty,
            "Productos:"
        };
        lineas.AddRange(detalles.Select(detalle => $"- {detalle.Descripcion}: {detalle.CantAPedir}"));

        if (!string.IsNullOrWhiteSpace(pedido.Observaciones))
        {
            lineas.Add(string.Empty);
            lineas.Add($"Observaciones: {pedido.Observaciones}");
        }

        return string.Join(Environment.NewLine, lineas);
    }

    public string GenerarEnlaceWhatsApp(Pedido pedido)
    {
        string texto = GenerarTextoWhatsapp(pedido);
        return $"https://wa.me/?text={Uri.EscapeDataString(texto)}";
    }

    private static bool EstaDentroDelRango(DateTime fecha, DateTime? desde, DateTime? hasta)
    {
        DateTime inicio = desde?.Date ?? DateTime.MinValue;
        DateTime finExclusivo = hasta?.Date.AddDays(1) ?? DateTime.MaxValue;
        return fecha >= inicio && fecha < finExclusivo;
    }
}
