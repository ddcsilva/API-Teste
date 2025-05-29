using MediatR;
using PersonManagement.Domain.Common;

namespace PersonManagement.Application.Features.Pessoas.Commands.ExcluirPessoa;

public class ExcluirPessoaCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }

    public ExcluirPessoaCommand(Guid id)
    {
        Id = id;
    }
}