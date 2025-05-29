using MediatR;
using PersonManagement.Domain.Common;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.Application.Features.Pessoas.Commands.ExcluirPessoa;

public class ExcluirPessoaCommandHandler : IRequestHandler<ExcluirPessoaCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ExcluirPessoaCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ExcluirPessoaCommand request, CancellationToken cancellationToken)
    {
        // Verificar se a pessoa existe
        var pessoa = await _unitOfWork.PessoaRepository.ObterPorIdAsync(request.Id, cancellationToken);
        if (pessoa == null)
        {
            return Result<bool>.Failure("Pessoa não encontrada");
        }

        // Excluir a pessoa
        await _unitOfWork.PessoaRepository.ExcluirAsync(request.Id, cancellationToken);
        await _unitOfWork.SalvarMudancasAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}