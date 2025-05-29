namespace PersonManagement.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime DataCriacao { get; protected set; }
    public DateTime? DataAtualizacao { get; protected set; }
}