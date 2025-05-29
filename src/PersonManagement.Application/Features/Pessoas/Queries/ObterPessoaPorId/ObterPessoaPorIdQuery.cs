using MediatR;
using PersonManagement.Application.DTOs;

namespace PersonManagement.Application.Features.Pessoas.Queries.ObterPessoaPorId;

public class ObterPessoaPorIdQuery : IRequest<PessoaDto?>
{
    public Guid Id { get; set; }

    public ObterPessoaPorIdQuery(Guid id)
    {
        Id = id;
    }
}