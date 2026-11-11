namespace pryHeladeriaBravoSegura.Models;

public class Pedido
{
    public int IdPedido { get; set; }
    public DateTime Fecha { get; set; }
    public int IdUsuario { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}
