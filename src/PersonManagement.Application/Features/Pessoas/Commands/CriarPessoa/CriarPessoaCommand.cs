using MediatR;
using PersonManagement.Application.DTOs;

namespace PersonManagement.Application.Features.Pessoas.Commands.CriarPessoa;

public class CriarPessoaCommand : IRequest<PessoaDto>
{
    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string Documento { get; set; } = string.Empty;
}