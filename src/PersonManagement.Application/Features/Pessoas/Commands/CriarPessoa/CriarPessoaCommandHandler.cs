using AutoMapper;
using MediatR;
using PersonManagement.Application.DTOs;
using PersonManagement.Domain.Common;
using PersonManagement.Domain.Entities;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.Application.Features.Pessoas.Commands.CriarPessoa;

public class CriarPessoaCommandHandler : IRequestHandler<CriarPessoaCommand, Result<PessoaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CriarPessoaCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PessoaDto>> Handle(CriarPessoaCommand request, CancellationToken cancellationToken)
    {
        var erros = new List<string>();

        // Validar email duplicado
        if (await _unitOfWork.PessoaRepository.EmailExisteAsync(request.Email, cancellationToken: cancellationToken))
        {
            erros.Add("Email já existe");
        }

        // Validar documento duplicado
        if (await _unitOfWork.PessoaRepository.DocumentoExisteAsync(request.Documento, cancellationToken: cancellationToken))
        {
            erros.Add("Documento já existe");
        }

        // Se há erros, retornar falha
        if (erros.Any())
        {
            return Result<PessoaDto>.Failure(erros);
        }

        // Criar a pessoa
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
        return Result<PessoaDto>.Success(pessoaDto);
    }
}