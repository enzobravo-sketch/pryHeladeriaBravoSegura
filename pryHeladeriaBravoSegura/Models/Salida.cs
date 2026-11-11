namespace pryHeladeriaBravoSegura.Models;

public class Salida
{
    public int IdSalida { get; set; }
    public int IdProducto { get; set; }
    public int IdUsuario { get; set; }
    public int IdStock { get; set; }
    public int Cantidad { get; set; }
    public DateTime Fecha { get; set; }
}
