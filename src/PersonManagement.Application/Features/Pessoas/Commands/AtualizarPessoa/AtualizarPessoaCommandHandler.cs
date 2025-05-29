using AutoMapper;
using MediatR;
using PersonManagement.Application.DTOs;
using PersonManagement.Domain.Common;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.Application.Features.Pessoas.Commands.AtualizarPessoa;

public class AtualizarPessoaCommandHandler : IRequestHandler<AtualizarPessoaCommand, Result<PessoaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AtualizarPessoaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PessoaDto>> Handle(AtualizarPessoaCommand request, CancellationToken cancellationToken)
    {
        // Verificar se a pessoa existe
        var pessoa = await _unitOfWork.PessoaRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (pessoa == null)
        {
            return Result<PessoaDto>.Failure("Pessoa não encontrada");
        }

        var erros = new List<string>();

        // Verificar se o email já existe para outra pessoa
        if (await _unitOfWork.PessoaRepository.EmailExisteAsync(request.Email, request.Id, cancellationToken))
        {
            erros.Add("Email já existe");
        }

        // Se há erros, retornar falha
        if (erros.Any())
        {
            return Result<PessoaDto>.Failure(erros);
        }

        // Atualizar a pessoa
        pessoa.AtualizarInformacoesPessoais(
            request.Nome,
            request.Sobrenome,
            request.Email,
            request.DataNascimento
        );

        await _unitOfWork.PessoaRepository.AtualizarAsync(pessoa, cancellationToken);
        await _unitOfWork.SalvarMudancasAsync(cancellationToken);

        var pessoaDto = _mapper.Map<PessoaDto>(pessoa);
        return Result<PessoaDto>.Success(pessoaDto);
    }
}