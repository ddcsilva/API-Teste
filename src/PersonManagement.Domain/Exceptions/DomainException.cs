namespace PersonManagement.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception innerException) : base(message, innerException) { }
}

public class BusinessValidationException : DomainException
{
    public BusinessValidationException(string message) : base(message) { }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base($"{entityName} com ID '{id}' não foi encontrado.") { }
}

public class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(string message) : base(message) { }
}