using AutoMapper;
using Moq;
using PersonManagement.Application.Features.Pessoas.Queries.ObterPessoaPorId;
using PersonManagement.Application.Mappings;
using PersonManagement.Domain.Common;
using PersonManagement.Domain.Entities;
using PersonManagement.Domain.Interfaces;

namespace PersonManagement.Application.Tests.Application.Queries;

public class ObterPessoaPorIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly ObterPessoaPorIdQueryHandler _handler;

    public ObterPessoaPorIdQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var mockRepository = new Mock<IPessoaRepository>();
        _unitOfWorkMock.Setup(x => x.PessoaRepository).Returns(mockRepository.Object);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<PessoaMappingProfile>();
        });
        _mapper = mapperConfig.CreateMapper();

        _handler = new ObterPessoaPorIdQueryHandler(_unitOfWorkMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_DeveRetornarSucessoComPessoaQuandoEncontrada()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var pessoa = new Pessoa("João", "Silva", "joao.silva@email.com", new DateTime(1990, 5, 15), "12345678901");

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoa);

        var query = new ObterPessoaPorIdQuery(pessoaId);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.False(resultado.IsFailure);
        Assert.NotNull(resultado.Value);
        Assert.Equal(pessoa.Id, resultado.Value.Id);
        Assert.Equal(pessoa.Nome, resultado.Value.Nome);
        Assert.Equal(pessoa.Sobrenome, resultado.Value.Sobrenome);
        Assert.Equal(pessoa.Email, resultado.Value.Email);
        Assert.Equal(pessoa.DataNascimento, resultado.Value.DataNascimento);
        Assert.Equal(pessoa.Documento, resultado.Value.Documento);
        Assert.Equal(pessoa.EstaAtivo, resultado.Value.EstaAtivo);
        Assert.Equal("João Silva", resultado.Value.NomeCompleto);
        Assert.True(resultado.Value.Idade > 0);

        _unitOfWorkMock.Verify(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveRetornarFalhaQuandoPessoaNaoEncontrada()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Pessoa?)null);

        var query = new ObterPessoaPorIdQuery(pessoaId);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.True(resultado.IsFailure);
        Assert.Equal("Pessoa não encontrada", resultado.ErrorMessage);
        Assert.Null(resultado.Value);

        _unitOfWorkMock.Verify(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DeveMapearCorretamentePessoaParaDto()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var pessoa = new Pessoa("Maria", "Santos", "maria.santos@email.com", new DateTime(1985, 8, 22), "98765432109");
        pessoa.Desativar(); // Testa pessoa inativa

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoa);

        var query = new ObterPessoaPorIdQuery(pessoaId);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal("Maria Santos", resultado.Value.NomeCompleto);
        Assert.False(resultado.Value.EstaAtivo);
        Assert.True(resultado.Value.Idade > 0);
        Assert.NotNull(resultado.Value.DataAtualizacao); // Desativar define DataAtualizacao
    }

    [Fact]
    public async Task Handle_DeveRespeitarCancellationToken()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var query = new ObterPessoaPorIdQuery(pessoaId);

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(query, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_DevePassarIdCorretoParaRepositorio()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var pessoa = new Pessoa("Pedro", "Oliveira", "pedro.oliveira@email.com", new DateTime(1992, 12, 3), "11122233344");

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoa);

        var query = new ObterPessoaPorIdQuery(pessoaId);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(resultado.IsSuccess);
        _unitOfWorkMock.Verify(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_DeveCalcularIdadeCorretamente()
    {
        // Arrange
        var pessoaId = Guid.NewGuid();
        var dataAniversario = DateTime.Today.AddYears(-30); // Exatamente 30 anos
        var pessoa = new Pessoa("Carlos", "Lima", "carlos.lima@email.com", dataAniversario, "99988877766");

        _unitOfWorkMock.Setup(x => x.PessoaRepository.ObterPorIdAsync(pessoaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pessoa);

        var query = new ObterPessoaPorIdQuery(pessoaId);

        // Act
        var resultado = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.NotNull(resultado.Value);
        Assert.Equal(30, resultado.Value.Idade);
    }
}