using pryHeladeriaBravoSegura.Data;

namespace pryHeladeriaBravoSegura.Services;

public sealed class ServiciosAplicacion
{
    public RepositorioJson Repositorio { get; }
    public AuthService Auth { get; }
    public ProductoService Productos { get; }
    public StockService Stock { get; }
    public PedidoService Pedidos { get; }
    public MovimientoService Movimientos { get; }
    public UsuarioService Usuarios { get; }

    public ServiciosAplicacion()
    {
        Repositorio = new RepositorioJson();
        Auth = new AuthService(Repositorio);
        Productos = new ProductoService(Repositorio);
        Stock = new StockService(Repositorio);
        Pedidos = new PedidoService(Repositorio);
        Movimientos = new MovimientoService(Repositorio);
        Usuarios = new UsuarioService(Repositorio);
    }
}
