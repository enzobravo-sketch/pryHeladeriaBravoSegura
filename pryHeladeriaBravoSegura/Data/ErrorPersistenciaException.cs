namespace pryHeladeriaBravoSegura.Data;

public sealed class ErrorPersistenciaException : Exception
{
    public ErrorPersistenciaException(string mensaje)
        : base(mensaje)
    {
    }

    public ErrorPersistenciaException(string mensaje, Exception innerException)
        : base(mensaje, innerException)
    {
    }
}
