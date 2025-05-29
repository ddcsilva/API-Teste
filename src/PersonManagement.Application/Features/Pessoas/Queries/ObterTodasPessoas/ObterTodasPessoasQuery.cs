using MediatR;
using PersonManagement.Application.DTOs;

namespace PersonManagement.Application.Features.Pessoas.Queries.ObterTodasPessoas;

public class ObterTodasPessoasQuery : IRequest<IEnumerable<PessoaDto>>
{
    public bool ApenasAtivos { get; set; } = false;

    public ObterTodasPessoasQuery(bool apenasAtivos = false)
    {
        ApenasAtivos = apenasAtivos;
    }
}