using AutoMapper;
using Moq;
using PersonManagement.Application.DTOs;
using PersonManagement.Application.Features.Pessoas.Commands.CriarPessoa;
using PersonManagement.Application.Mappings;
using PersonManagement.Domain.Entities;
using PersonManagement.Domain.Exceptions;
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
    public async Task Handle_DeveCriarPessoaComSucesso()
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
        Assert.NotNull(resultado);
        Assert.Equal(command.Nome, resultado.Nome);
        Assert.Equal(command.Sobrenome, resultado.Sobrenome);
        Assert.Equal(command.Email, resultado.Email);
        Assert.Equal(command.DataNascimento, resultado.DataNascimento);
        Assert.Equal(command.Documento, resultado.Documento);
        Assert.True(resultado.EstaAtivo);
        Assert.NotEqual(Guid.Empty, resultado.Id);

        // Verificar se os métodos foram chamados
        _unitOfWorkMock.Verify(x => x.PessoaRepository.EmailExisteAsync(command.Email, null, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.DocumentoExisteAsync(command.Documento, null, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.AdicionarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveLancarExcecaoQuandoEmailJaExiste()
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

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateEntityException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Email já existe", exception.Message);

        // Verificar que não tentou verificar documento nem adicionar
        _unitOfWorkMock.Verify(x => x.PessoaRepository.EmailExisteAsync(command.Email, null, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.DocumentoExisteAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.AdicionarAsync(It.IsAny<Pessoa>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SalvarMudancasAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_DeveLancarExcecaoQuandoDocumentoJaExiste()
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

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DuplicateEntityException>(
            () => _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Documento já existe", exception.Message);

        // Verificar sequência de chamadas
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
        Assert.Equal("Maria Santos", resultado.NomeCompleto);
        Assert.True(resultado.Idade > 0); // Verifica se a idade foi calculada
        Assert.Equal(command.Nome, resultado.Nome);
        Assert.Equal(command.Sobrenome, resultado.Sobrenome);
        Assert.Equal(command.Email, resultado.Email);
        Assert.Equal(command.DataNascimento, resultado.DataNascimento);
        Assert.Equal(command.Documento, resultado.Documento);
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
        Assert.True(resultado.DataCriacao >= dataAntes);
        Assert.True(resultado.DataCriacao <= dataDepois);
        Assert.Null(resultado.DataAtualizacao);
    }
}