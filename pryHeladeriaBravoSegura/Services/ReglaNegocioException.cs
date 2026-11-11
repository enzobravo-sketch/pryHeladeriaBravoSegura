namespace pryHeladeriaBravoSegura.Services;

public sealed class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje)
        : base(mensaje)
    {
    }
}
