using AutoMapper;
using MediatR;
using PersonManagement.Application.DTOs;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.Application.Features.Pessoas.Commands.AtualizarPessoa;

public class AtualizarPessoaCommandHandler : IRequestHandler<AtualizarPessoaCommand, PessoaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AtualizarPessoaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PessoaDto> Handle(AtualizarPessoaCommand request, CancellationToken cancellationToken)
    {
        var pessoa = await _unitOfWork.PessoaRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (pessoa == null)
        {
            throw new InvalidOperationException("Pessoa não encontrada");
        }

        // Verificar se o email já existe para outra pessoa
        if (await _unitOfWork.PessoaRepository.EmailExisteAsync(request.Email, request.Id, cancellationToken))
        {
            throw new InvalidOperationException("Email já existe");
        }

        pessoa.AtualizarInformacoesPessoais(
            request.Nome,
            request.Sobrenome,
            request.Email,
            request.DataNascimento
        );

        await _unitOfWork.PessoaRepository.AtualizarAsync(pessoa, cancellationToken);
        await _unitOfWork.SalvarMudancasAsync(cancellationToken);

        var pessoaDto = _mapper.Map<PessoaDto>(pessoa);
        return pessoaDto;
    }
}