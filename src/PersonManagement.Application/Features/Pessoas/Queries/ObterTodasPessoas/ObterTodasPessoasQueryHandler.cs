using AutoMapper;
using MediatR;
using PersonManagement.Application.DTOs;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.Application.Features.Pessoas.Queries.ObterTodasPessoas;

public class ObterTodasPessoasQueryHandler : IRequestHandler<ObterTodasPessoasQuery, IEnumerable<PessoaDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ObterTodasPessoasQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PessoaDto>> Handle(ObterTodasPessoasQuery request, CancellationToken cancellationToken)
    {
        var pessoas = request.ApenasAtivos
            ? await _unitOfWork.PessoaRepository.ObterAtivosAsync(cancellationToken)
            : await _unitOfWork.PessoaRepository.ObterTodosAsync(cancellationToken);

        return _mapper.Map<IEnumerable<PessoaDto>>(pessoas);
    }
}