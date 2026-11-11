using pryHeladeriaBravoSegura.Forms;
using pryHeladeriaBravoSegura.Services;

namespace pryHeladeriaBravoSegura;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new FormularioLogin(new ServiciosAplicacion()));
    }
}
