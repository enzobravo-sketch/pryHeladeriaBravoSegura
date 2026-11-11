namespace pryHeladeriaBravoSegura.Models;

public class DatosApp
{
    public List<Tipo> Tipos { get; set; } = [];
    public List<Producto> Productos { get; set; } = [];
    public List<Stock> Stocks { get; set; } = [];
    public List<Remito> Remitos { get; set; } = [];
    public List<Salida> Salidas { get; set; } = [];
    public List<Usuario> Usuarios { get; set; } = [];
    public List<Pedido> Pedidos { get; set; } = [];
    public List<DetallePedido> DetallesPedido { get; set; } = [];
}
