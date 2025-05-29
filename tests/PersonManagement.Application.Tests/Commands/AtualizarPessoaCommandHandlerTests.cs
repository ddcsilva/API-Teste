using AutoMapper;
using Moq;
using PersonManagement.Application.DTOs;
using PersonManagement.Application.Features.Pessoas.Commands.AtualizarPessoa;
using PersonManagement.Application.Mappings;
using PersonManagement.Domain.Common;
using PersonManagement.Domain.Entities;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.Application.Tests.Application.Commands;

public class AtualizarPessoaCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly AtualizarPessoaCommandHandler _handler;

    public AtualizarPessoaCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var mockRepository = new Mock<IPessoaRepository>();
        _unitOfWorkMock.Setup(x => x.PessoaRepository).Returns(mockRepository.Object);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PessoaMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new AtualizarPessoaCommandHandler(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_DeveRetornarSucessoQuandoAtualizarPessoaComSucesso()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var command = new AtualizarPessoaCommand
        {
            Id = pessoaId,
            Nome = "João Atualizado",
            Sobrenome = "Silva Santos",
            Email = "joao.atualizado@email.com",
            DataNascimento = new DateTime(1990, 5, 15)
        };

        var pessoaExistente = new Pessoa(
            "João", "Silva", "joao.original@email.com",
            new DateTime(1990, 5, 15), "12345678901");

        // Simular que a pessoa já tem o ID definido
        var propriedadeId = typeof(Pessoa).GetProperty("Id");
        propriedadeId?.SetValue(pessoaExistente, pessoaId);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoaExistente);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.EmailExisteAsync(command.Email, pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.AtualizarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.False(resultado.IsFailure);
        Assert.NotNull(resultado.Value);
        Assert.Equal(command.Nome, resultado.Value.Nome);
        Assert.Equal(command.Sobrenome, resultado.Value.Sobrenome);
        Assert.Equal(command.Email, resultado.Value.Email);
        Assert.Equal(command.DataNascimento, resultado.Value.DataNascimento);
        Assert.Equal(pessoaId, resultado.Value.Id);

        // Verificar se os métodos foram chamados
        _unitOfWorkMock.Verify(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.EmailExisteAsync(command.Email, pessoaId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.AtualizarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarFalhaQuandoPessoaNaoEncontrada()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var command = new AtualizarPessoaCommand
        {
            Id = pessoaId,
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao@email.com",
            DataNascimento = new DateTime(1990, 5, 15)
        };

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pessoa)null!);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.True(resultado.IsFailure);
        Assert.Null(resultado.Value);
        Assert.Equal("Pessoa não encontrada", resultado.ErrorMessage);
        Assert.Contains("Pessoa não encontrada", resultado.Errors);
        Assert.Single(resultado.Errors);

        // Verificar que só tentou buscar a pessoa
        _unitOfWorkMock.Verify(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.EmailExisteAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.AtualizarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveRetornarFalhaQuandoEmailJaExisteParaOutraPessoa()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var command = new AtualizarPessoaCommand
        {
            Id = pessoaId,
            Nome = "João",
            Sobrenome = "Silva",
            Email = "email.duplicado@email.com",
            DataNascimento = new DateTime(1990, 5, 15)
        };

        var pessoaExistente = new Pessoa(
            "João", "Silva", "joao.original@email.com",
            new DateTime(1990, 5, 15), "12345678901");

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoaExistente);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.EmailExisteAsync(command.Email, pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.True(resultado.IsFailure);
        Assert.Null(resultado.Value);
        Assert.Equal("Email já existe", resultado.ErrorMessage);
        Assert.Contains("Email já existe", resultado.Errors);
        Assert.Single(resultado.Errors);

        // Verificar sequência de chamadas
        _unitOfWorkMock.Verify(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.EmailExisteAsync(command.Email, pessoaId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.AtualizarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveMapearCorretamentePessoaParaDto()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var command = new AtualizarPessoaCommand
        {
            Id = pessoaId,
            Nome = "Maria",
            Sobrenome = "Santos",
            Email = "maria.santos.atualizada@email.com",
            DataNascimento = new DateTime(1985, 8, 22)
        };

        var pessoaExistente = new Pessoa(
            "Maria", "Silva", "maria.original@email.com",
            new DateTime(1985, 8, 22), "98765432109");

        // Simular que a pessoa já tem o ID definido
        var propriedadeId = typeof(Pessoa).GetProperty("Id");
        propriedadeId?.SetValue(pessoaExistente, pessoaId);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoaExistente);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.EmailExisteAsync(command.Email, pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.AtualizarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal("Maria Santos", resultado.Value.NomeCompleto);
        Assert.True(resultado.Value.Idade > 0); // Verifica se a idade foi calculada
        Assert.Equal(command.Nome, resultado.Value.Nome);
        Assert.Equal(command.Sobrenome, resultado.Value.Sobrenome);
        Assert.Equal(command.Email, resultado.Value.Email);
        Assert.Equal(command.DataNascimento, resultado.Value.DataNascimento);
    }

    [Fact]
    public async Task Handle_DeveRespeitarCancellationToken()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var command = new AtualizarPessoaCommand
        {
            Id = pessoaId,
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao@email.com",
            DataNascimento = new DateTime(1990, 5, 15)
        };

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(command, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_DeveDefinirDataAtualizacaoAutomaticamente()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var command = new AtualizarPessoaCommand
        {
            Id = pessoaId,
            Nome = "Pedro",
            Sobrenome = "Oliveira",
            Email = "pedro.oliveira@email.com",
            DataNascimento = new DateTime(1992, 12, 3)
        };

        var pessoaExistente = new Pessoa(
            "Pedro", "Silva", "pedro.original@email.com",
            new DateTime(1992, 12, 3), "11122233344");

        // Simular que a pessoa já tem o ID definido
        var propriedadeId = typeof(Pessoa).GetProperty("Id");
        propriedadeId?.SetValue(pessoaExistente, pessoaId);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoaExistente);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.EmailExisteAsync(command.Email, pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.AtualizarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dataAntes = DateTime.UtcNow;

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        var dataDepois = DateTime.UtcNow;

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.NotNull(resultado.Value.DataAtualizacao);
        Assert.True(resultado.Value.DataAtualizacao >= dataAntes);
        Assert.True(resultado.Value.DataAtualizacao <= dataDepois);
    }
}
