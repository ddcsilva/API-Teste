using AutoMapper;
using Moq;
using PersonManagement.Application.DTOs;
using PersonManagement.Application.Features.Pessoas.Commands.CriarPessoa;
using PersonManagement.Application.Mappings;
using PersonManagement.Domain.Common;
using PersonManagement.Domain.Entities;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.Application.Tests.Application.Commands;

public class CriarPessoaCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly CriarPessoaCommandHandler _handler;

    public CriarPessoaCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var mockRepository = new Mock<IPessoaRepository>();
        _unitOfWorkMock.Setup(x => x.PessoaRepository).Returns(mockRepository.Object);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PessoaMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new CriarPessoaCommandHandler(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_DeveRetornarSucessoQuandoCriarPessoaComSucesso()
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao.silva@email.com",
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        _unitOfWorkMock.Setup(x => x.PessoaRepository.EmailExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.DocumentoExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.AdicionarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pessoa p, CancellationToken _) => p);

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
        Assert.Equal(command.Documento, resultado.Value.Documento);
        Assert.True(resultado.Value.EstaAtivo);
        Assert.NotEqual(Guid.Empty, resultado.Value.Id);

        // Verificar se os métodos foram chamados
        _unitOfWorkMock.Verify(x => x.PessoaRepository.EmailExisteAsync(command.Email, null, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.DocumentoExisteAsync(command.Documento, null, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.AdicionarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarFalhaQuandoEmailJaExiste()
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao.silva@email.com",
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        _unitOfWorkMock.Setup(x => x.PessoaRepository.EmailExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.DocumentoExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.True(resultado.IsFailure);
        Assert.Null(resultado.Value);
        Assert.Contains("Email já existe", resultado.Errors);
        Assert.Single(resultado.Errors);
        Assert.Equal("Email já existe", resultado.ErrorMessage);

        // Verificar que verificou ambos mas não tentou adicionar
        _unitOfWorkMock.Verify(x => x.PessoaRepository.EmailExisteAsync(command.Email, null, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.DocumentoExisteAsync(command.Documento, null, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.AdicionarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveRetornarFalhaQuandoDocumentoJaExiste()
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao.silva@email.com",
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        _unitOfWorkMock.Setup(x => x.PessoaRepository.EmailExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.DocumentoExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.True(resultado.IsFailure);
        Assert.Null(resultado.Value);
        Assert.Contains("Documento já existe", resultado.Errors);
        Assert.Single(resultado.Errors);
        Assert.Equal("Documento já existe", resultado.ErrorMessage);

        // Verificar sequência de chamadas
        _unitOfWorkMock.Verify(x => x.PessoaRepository.EmailExisteAsync(command.Email, null, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.DocumentoExisteAsync(command.Documento, null, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.AdicionarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveRetornarFalhaComMultiplosErrosQuandoEmailEDocumentoJaExistem()
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao.silva@email.com",
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        _unitOfWorkMock.Setup(x => x.PessoaRepository.EmailExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.DocumentoExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.True(resultado.IsFailure);
        Assert.Null(resultado.Value);
        Assert.Equal(2, resultado.Errors.Count);
        Assert.Contains("Email já existe", resultado.Errors);
        Assert.Contains("Documento já existe", resultado.Errors);
        Assert.Equal("Email já existe; Documento já existe", resultado.ErrorMessage);

        // Verificar que verificou ambos mas não tentou adicionar
        _unitOfWorkMock.Verify(x => x.PessoaRepository.EmailExisteAsync(command.Email, null, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.DocumentoExisteAsync(command.Documento, null, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.AdicionarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveMapearCorretamentePessoaParaDto()
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = "Maria",
            Sobrenome = "Santos",
            Email = "maria.santos@email.com",
            DataNascimento = new DateTime(1985, 8, 22),
            Documento = "98765432109"
        };

        _unitOfWorkMock.Setup(x => x.PessoaRepository.EmailExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.DocumentoExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.AdicionarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pessoa p, CancellationToken _) => p);

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
        Assert.Equal(command.Documento, resultado.Value.Documento);
    }

    [Fact]
    public async Task Handle_DeveRespeitarCancellationToken()
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao.silva@email.com",
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _unitOfWorkMock.Setup(x => x.PessoaRepository.EmailExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(command, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_DeveDefinirDataCriacaoAutomaticamente()
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = "Pedro",
            Sobrenome = "Oliveira",
            Email = "pedro.oliveira@email.com",
            DataNascimento = new DateTime(1992, 12, 3),
            Documento = "11122233344"
        };

        _unitOfWorkMock.Setup(x => x.PessoaRepository.EmailExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.DocumentoExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock.Setup(x => x.PessoaRepository.AdicionarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pessoa p, CancellationToken _) => p);

        _unitOfWorkMock.Setup(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var dataAntes = DateTime.UtcNow;

        // Act
        var resultado = await _handler.Handle(command, CancellationToken.None);

        var dataDepois = DateTime.UtcNow;

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.True(resultado.Value.DataCriacao >= dataAntes);
        Assert.True(resultado.Value.DataCriacao <= dataDepois);
        Assert.Null(resultado.Value.DataAtualizacao);
    }
}