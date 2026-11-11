namespace pryHeladeriaBravoSegura.Models;

public class Producto
{
    public int IdProducto { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int StockMinimo { get; set; }
    public int IdTipo { get; set; }
}
