using pryHeladeriaBravoSegura.Models;

namespace pryHeladeriaBravoSegura.Services;

public enum FiltroEstadoProducto
{
    Todos,
    StockCritico,
    Agotados,
    Disponible
}

public sealed class ProductoResumen
{
    public int IdProducto { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public string NombreTipo { get; init; } = string.Empty;
    public int StockMinimo { get; init; }
    public int StockActual { get; init; }
    public EstadoProducto Estado { get; init; }
    public string EstadoTexto => Estado switch
    {
        EstadoProducto.Agotado => "Agotado",
        EstadoProducto.StockCritico => "Stock crítico",
        _ => "Disponible"
    };
}

public sealed class DetallePedidoPropuesto
{
    public int IdProducto { get; init; }
    public string Descripcion { get; init; } = string.Empty;
    public int StockMinimo { get; init; }
    public int StockActual { get; init; }
    public int CantAPedir { get; set; }
}

public sealed class MovimientoResumen
{
    public int IdProducto { get; init; }
    public DateTime Fecha { get; init; }
    public string DescripcionProducto { get; init; } = string.Empty;
    public TipoMovimiento Tipo { get; init; }
    public int Cantidad { get; init; }
    public string NombreUsuario { get; init; } = string.Empty;
}
