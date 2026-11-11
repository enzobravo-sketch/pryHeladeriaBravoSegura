namespace pryHeladeriaBravoSegura.Models;

public class DetallePedido
{
    public int IdDetalle { get; set; }
    public int IdPedido { get; set; }
    public int IdProducto { get; set; }
    public int CantAPedir { get; set; }
}
