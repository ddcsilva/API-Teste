using MediatR;
using PersonManagement.Application.DTOs;

namespace PersonManagement.Application.Features.Pessoas.Commands.AtualizarPessoa;

public class AtualizarPessoaCommand : IRequest<PessoaDto>
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
}