using MediatR;
using PersonManagement.Application.DTOs;
using PersonManagement.Domain.Common;

namespace PersonManagement.Application.Features.Pessoas.Commands.CriarPessoa;

public class CriarPessoaCommand : IRequest<Result<PessoaDto>>
{
    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string Documento { get; set; } = string.Empty;
}