using MediatR;
using PersonManagement.Application.DTOs;
using PersonManagement.Domain.Common;

namespace PersonManagement.Application.Features.Pessoas.Queries.ObterPessoaPorId;

public class ObterPessoaPorIdQuery : IRequest<Result<PessoaDto>>
{
    public Guid Id { get; set; }

    public ObterPessoaPorIdQuery(Guid id)
    {
        Id = id;
    }
}