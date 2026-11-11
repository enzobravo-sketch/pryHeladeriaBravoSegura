namespace pryHeladeriaBravoSegura.Models;

public class Remito
{
    public int IdRemito { get; set; }
    public string NumeroRemito { get; set; } = string.Empty;
    public DateTime FechaRemito { get; set; }
    public int IdStock { get; set; }
    public int IdProducto { get; set; }
    public int IdUsuario { get; set; }
    public DateTime Fecha { get; set; }
    public int Cantidad { get; set; }
}
