using Moq;
using PersonManagement.Application.Features.Pessoas.Commands.ExcluirPessoa;
using PersonManagement.Domain.Common;
using PersonManagement.Domain.Entities;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.Application.Tests.Application.Commands;

public class ExcluirPessoaCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ExcluirPessoaCommandHandler _handler;

    public ExcluirPessoaCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var mockRepository = new Mock<IPessoaRepository>();
        _unitOfWorkMock.Setup(x => x.PessoaRepository).Returns(mockRepository.Object);

        _handler = new ExcluirPessoaCommandHandler(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_DeveRetornarSucessoQuandoExcluirPessoaComSucesso()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var command = new ExcluirPessoaCommand(pessoaId);

        var pessoaExistente = new Pessoa(
            "João", "Silva", "joao@email.com",
            new DateTime(1990, 5, 15), "12345678901");

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoaExistente);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ExcluirAsync(pessoaId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.False(resultado.IsFailure);
        Assert.True(resultado.Value);
        Assert.Null(resultado.ErrorMessage);
        Assert.Empty(resultado.Errors);

        // Verificar se os métodos foram chamados
        _unitOfWorkMock.Verify(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.ExcluirAsync(pessoaId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarFalhaQuandoPessoaNaoEncontrada()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var command = new ExcluirPessoaCommand(pessoaId);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pessoa)null);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.True(resultado.IsFailure);
        Assert.False(resultado.Value);
        Assert.Equal("Pessoa não encontrada", resultado.ErrorMessage);
        Assert.Contains("Pessoa não encontrada", resultado.Errors);
        Assert.Single(resultado.Errors);

        // Verificar que só tentou buscar a pessoa
        _unitOfWorkMock.Verify(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.ExcluirAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveRespeitarCancellationToken()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var command = new ExcluirPessoaCommand(pessoaId);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(command, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_DevePassarParametrosCorretamente()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var command = new ExcluirPessoaCommand(pessoaId);

        var pessoaExistente = new Pessoa(
            "Maria", "Santos", "maria@email.com",
            new DateTime(1985, 8, 22), "98765432109");

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoaExistente);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ExcluirAsync(pessoaId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado.IsSuccess);

        // Verificar que os parâmetros foram passados corretamente
        _unitOfWorkMock.Verify(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.ExcluirAsync(pessoaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveManterSequenciaCorretaDeOperacoes()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var command = new ExcluirPessoaCommand(pessoaId);

        var pessoaExistente = new Pessoa(
            "Pedro", "Oliveira", "pedro@email.com",
            new DateTime(1992, 12, 3), "11122233344");

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoaExistente);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ExcluirAsync(pessoaId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado.IsSuccess);

        // Verificar sequência: primeiro busca, depois exclui, depois salva
        var sequence = new MockSequence();
        _unitOfWorkMock.InSequence(sequence).Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()));
        _unitOfWorkMock.InSequence(sequence).Setup(x => x.PessoaRepository.ExcluirAsync(pessoaId, It.IsAny<CancellationToken>()));
        _unitOfWorkMock.InSequence(sequence).Setup(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()));
    }
}