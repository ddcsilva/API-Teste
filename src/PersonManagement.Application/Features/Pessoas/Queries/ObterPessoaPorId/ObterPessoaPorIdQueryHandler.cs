using AutoMapper;
using MediatR;
using PersonManagement.Application.DTOs;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.Application.Features.Pessoas.Queries.ObterPessoaPorId;

public class ObterPessoaPorIdQueryHandler : IRequestHandler<ObterPessoaPorIdQuery, PessoaDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ObterPessoaPorIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PessoaDto?> Handle(ObterPessoaPorIdQuery request, CancellationToken cancellationToken)
    {
        var pessoa = await _unitOfWork.PessoaRepository.ObterPorIdAsync(request.Id, cancellationToken);
        return pessoa == null ? null : _mapper.Map<PessoaDto>(pessoa);
    }
}