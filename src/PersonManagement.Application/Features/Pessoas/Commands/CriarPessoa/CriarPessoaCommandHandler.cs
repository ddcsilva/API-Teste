using AutoMapper;
using MediatR;
using PersonManagement.Application.DTOs;
using PersonManagement.Domain.Entities;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.Application.Features.Pessoas.Commands.CriarPessoa;

public class CriarPessoaCommandHandler : IRequestHandler<CriarPessoaCommand, PessoaDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CriarPessoaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PessoaDto> Handle(CriarPessoaCommand request, CancellationToken cancellationToken)
    {
        if (await _unitOfWork.PessoaRepository.EmailExisteAsync(request.Email, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Email já existe");
        }

        if (await _unitOfWork.PessoaRepository.DocumentoExisteAsync(request.Documento, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException("Documento já existe");
        }

        var pessoa = new Pessoa(
            request.Nome,
            request.Sobrenome,
            request.Email,
            request.DataNascimento,
            request.Documento
        );

        await _unitOfWork.PessoaRepository.AdicionarAsync(pessoa, cancellationToken);
        await _unitOfWork.SalvarMudancasAsync(cancellationToken);

        var pessoaDto = _mapper.Map<PessoaDto>(pessoa);
        return pessoaDto;
    }
}