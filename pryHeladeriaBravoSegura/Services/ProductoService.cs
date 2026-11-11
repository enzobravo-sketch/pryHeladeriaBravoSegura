using pryHeladeriaBravoSegura.Data;
using pryHeladeriaBravoSegura.Models;

namespace pryHeladeriaBravoSegura.Services;

public sealed class ProductoService
{
    private readonly RepositorioJson repositorio;

    public ProductoService(RepositorioJson repositorio)
    {
        this.repositorio = repositorio;
    }

    public IReadOnlyList<ProductoResumen> ObtenerProductos(
        string? texto = null,
        int? idTipo = null,
        FiltroEstadoProducto filtro = FiltroEstadoProducto.Todos)
    {
        string busqueda = texto?.Trim() ?? string.Empty;

        return repositorio.Datos.Productos
            .Join(
                repositorio.Datos.Tipos,
                producto => producto.IdTipo,
                tipo => tipo.IdTipo,
                (producto, tipo) => new { Producto = producto, Tipo = tipo })
            .Select(elemento =>
            {
                int stock = ObtenerStock(elemento.Producto.IdProducto).Cantidad;
                return new ProductoResumen
                {
                    IdProducto = elemento.Producto.IdProducto,
                    Descripcion = elemento.Producto.Descripcion,
                    NombreTipo = elemento.Tipo.Nombre,
                    StockMinimo = elemento.Producto.StockMinimo,
                    StockActual = stock,
                    Estado = CalcularEstado(stock, elemento.Producto.StockMinimo)
                };
            })
            .Where(resumen => string.IsNullOrEmpty(busqueda)
                || resumen.Descripcion.Contains(busqueda, StringComparison.OrdinalIgnoreCase))
            .Where(resumen => !idTipo.HasValue
                || repositorio.Datos.Productos.First(producto => producto.IdProducto == resumen.IdProducto).IdTipo == idTipo.Value)
            .Where(resumen => filtro switch
            {
                FiltroEstadoProducto.StockCritico => resumen.Estado == EstadoProducto.StockCritico,
                FiltroEstadoProducto.Agotados => resumen.Estado == EstadoProducto.Agotado,
                FiltroEstadoProducto.Disponible => resumen.Estado == EstadoProducto.Disponible,
                _ => true
            })
            .OrderBy(resumen => resumen.Descripcion)
            .ToList();
    }

    public EstadoProducto ObtenerEstado(int idProducto)
    {
        Producto producto = ObtenerProducto(idProducto);
        return CalcularEstado(ObtenerStock(idProducto).Cantidad, producto.StockMinimo);
    }

    public Producto ObtenerProducto(int idProducto)
    {
        return repositorio.Datos.Productos.FirstOrDefault(producto => producto.IdProducto == idProducto)
            ?? throw new ReglaNegocioException("El producto seleccionado no existe en el catálogo.");
    }

    public int ObtenerStockActual(int idProducto)
    {
        return ObtenerStock(idProducto).Cantidad;
    }

    public Producto CrearProductoFaltante(string descripcion, int idTipo, int stockMinimo)
    {
        string descripcionNormalizada = descripcion?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(descripcionNormalizada))
        {
            throw new ReglaNegocioException("Ingrese la descripción del producto.");
        }

        if (stockMinimo <= 0)
        {
            throw new ReglaNegocioException("El stock mínimo debe ser mayor que cero.");
        }

        if (!repositorio.Datos.Tipos.Any(tipo => tipo.IdTipo == idTipo))
        {
            throw new ReglaNegocioException("El tipo seleccionado no existe.");
        }

        if (repositorio.Datos.Productos.Any(producto =>
            string.Equals(producto.Descripcion, descripcionNormalizada, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ReglaNegocioException("Ya existe un producto con esa descripción.");
        }

        Producto nuevoProducto = new();
        repositorio.EjecutarTransaccion(datos =>
        {
            nuevoProducto = new Producto
            {
                IdProducto = repositorio.ObtenerProximoId(datos.Productos, producto => producto.IdProducto),
                Descripcion = descripcionNormalizada,
                StockMinimo = stockMinimo,
                IdTipo = idTipo
            };

            int idStock = repositorio.ObtenerProximoId(datos.Stocks, stock => stock.IdStock);
            datos.Productos.Add(nuevoProducto);
            datos.Stocks.Add(new Stock
            {
                IdStock = idStock,
                IdProducto = nuevoProducto.IdProducto,
                Cantidad = 0
            });
        });

        return nuevoProducto;
    }

    public static EstadoProducto CalcularEstado(int stockActual, int stockMinimo)
    {
        return stockActual switch
        {
            0 => EstadoProducto.Agotado,
            _ when stockActual <= stockMinimo => EstadoProducto.StockCritico,
            _ => EstadoProducto.Disponible
        };
    }

    private Stock ObtenerStock(int idProducto)
    {
        return repositorio.Datos.Stocks.FirstOrDefault(stock => stock.IdProducto == idProducto)
            ?? throw new ReglaNegocioException("El producto no tiene un registro de stock válido.");
    }
}
