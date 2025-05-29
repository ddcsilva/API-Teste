using Microsoft.EntityFrameworkCore;
using PersonManagement.Domain.Entities;
using PersonManagement.Infrastructure.Data;
using PersonManagement.Infrastructure.Repositories;

namespace PersonManagement.Infrastructure.Tests.Infrastructure.Repositories;

public class PessoaRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly PessoaRepository _repository;

    public PessoaRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new PessoaRepository(_context);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarPessoaQuandoEncontrada()
    {
        // Arrange
        var pessoa = new Pessoa("João", "Silva", "joao.silva@email.com", new DateTime(1990, 5, 15), "12345678901");
        await _context.Pessoas.AddAsync(pessoa);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ObterPorIdAsync(pessoa.Id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(pessoa.Id, resultado.Id);
        Assert.Equal(pessoa.Nome, resultado.Nome);
        Assert.Equal(pessoa.Email, resultado.Email);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNullQuandoNaoEncontrada()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var resultado = await _repository.ObterPorIdAsync(idInexistente);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterPorEmailAsync_DeveRetornarPessoaQuandoEncontrada()
    {
        // Arrange
        var pessoa = new Pessoa("Maria", "Santos", "maria.santos@email.com", new DateTime(1985, 8, 22), "98765432109");
        await _context.Pessoas.AddAsync(pessoa);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ObterPorEmailAsync("maria.santos@email.com");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(pessoa.Id, resultado.Id);
        Assert.Equal("maria.santos@email.com", resultado.Email);
    }

    [Fact]
    public async Task ObterPorEmailAsync_DeveRetornarNullQuandoNaoEncontrada()
    {
        // Arrange
        var emailInexistente = "inexistente@email.com";

        // Act
        var resultado = await _repository.ObterPorEmailAsync(emailInexistente);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterPorDocumentoAsync_DeveRetornarPessoaQuandoEncontrada()
    {
        // Arrange
        var pessoa = new Pessoa("Pedro", "Oliveira", "pedro.oliveira@email.com", new DateTime(1992, 12, 3), "11122233344");
        await _context.Pessoas.AddAsync(pessoa);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ObterPorDocumentoAsync("11122233344");

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(pessoa.Id, resultado.Id);
        Assert.Equal("11122233344", resultado.Documento);
    }

    [Fact]
    public async Task ObterPorDocumentoAsync_DeveRetornarNullQuandoNaoEncontrada()
    {
        // Arrange
        var documentoInexistente = "99999999999";

        // Act
        var resultado = await _repository.ObterPorDocumentoAsync(documentoInexistente);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarTodasAsPessoas()
    {
        // Arrange
        var pessoas = new[]
        {
            new Pessoa("João", "Silva", "joao.silva@email.com", new DateTime(1990, 5, 15), "12345678901"),
            new Pessoa("Maria", "Santos", "maria.santos@email.com", new DateTime(1985, 8, 22), "98765432109"),
            new Pessoa("Pedro", "Oliveira", "pedro.oliveira@email.com", new DateTime(1992, 12, 3), "11122233344")
        };

        pessoas[1].Desativar(); // Maria inativa

        await _context.Pessoas.AddRangeAsync(pessoas);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ObterTodosAsync();

        // Assert
        Assert.Equal(3, resultado.Count());
        Assert.Contains(resultado, p => p.Nome == "João");
        Assert.Contains(resultado, p => p.Nome == "Maria");
        Assert.Contains(resultado, p => p.Nome == "Pedro");
    }

    [Fact]
    public async Task ObterAtivosAsync_DeveRetornarApenasAsPessoasAtivas()
    {
        // Arrange
        var pessoas = new[]
        {
            new Pessoa("João", "Silva", "joao.silva@email.com", new DateTime(1990, 5, 15), "12345678901"),
            new Pessoa("Maria", "Santos", "maria.santos@email.com", new DateTime(1985, 8, 22), "98765432109"),
            new Pessoa("Pedro", "Oliveira", "pedro.oliveira@email.com", new DateTime(1992, 12, 3), "11122233344")
        };

        pessoas[1].Desativar(); // Maria inativa

        await _context.Pessoas.AddRangeAsync(pessoas);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ObterAtivosAsync();

        // Assert
        Assert.Equal(2, resultado.Count());
        Assert.Contains(resultado, p => p.Nome == "João" && p.EstaAtivo);
        Assert.Contains(resultado, p => p.Nome == "Pedro" && p.EstaAtivo);
        Assert.DoesNotContain(resultado, p => p.Nome == "Maria");
    }

    [Fact]
    public async Task AdicionarAsync_DeveAdicionarPessoaComSucesso()
    {
        // Arrange
        var pessoa = new Pessoa("Ana", "Costa", "ana.costa@email.com", new DateTime(1988, 3, 10), "55566677788");

        // Act
        var resultado = await _repository.AdicionarAsync(pessoa);
        await _context.SaveChangesAsync();

        // Assert
        Assert.Equal(pessoa, resultado);

        var pessoaNoBanco = await _context.Pessoas.FindAsync(pessoa.Id);
        Assert.NotNull(pessoaNoBanco);
        Assert.Equal("Ana", pessoaNoBanco.Nome);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarPessoaComSucesso()
    {
        // Arrange
        var pessoa = new Pessoa("Carlos", "Lima", "carlos.lima@email.com", new DateTime(1995, 11, 25), "99988877766");
        await _context.Pessoas.AddAsync(pessoa);
        await _context.SaveChangesAsync();

        // Act
        pessoa.AtualizarInformacoesPessoais("Carlos Atualizado", "Lima Santos", "carlos.novo@email.com", new DateTime(1995, 11, 25));
        await _repository.AtualizarAsync(pessoa);
        await _context.SaveChangesAsync();

        // Assert
        var pessoaAtualizada = await _context.Pessoas.FindAsync(pessoa.Id);
        Assert.NotNull(pessoaAtualizada);
        Assert.Equal("Carlos Atualizado", pessoaAtualizada.Nome);
        Assert.Equal("Lima Santos", pessoaAtualizada.Sobrenome);
        Assert.Equal("carlos.novo@email.com", pessoaAtualizada.Email);
    }

    [Fact]
    public async Task ExcluirAsync_DeveRemoverPessoaComSucesso()
    {
        // Arrange
        var pessoa = new Pessoa("Lucia", "Ferreira", "lucia.ferreira@email.com", new DateTime(1980, 7, 18), "44455566677");
        await _context.Pessoas.AddAsync(pessoa);
        await _context.SaveChangesAsync();

        // Act
        await _repository.ExcluirAsync(pessoa.Id);
        await _context.SaveChangesAsync();

        // Assert
        var pessoaRemovida = await _context.Pessoas.FindAsync(pessoa.Id);
        Assert.Null(pessoaRemovida);
    }

    [Fact]
    public async Task ExcluirAsync_NaoDeveFalharQuandoPessoaNaoExiste()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act & Assert
        await _repository.ExcluirAsync(idInexistente);
        await _context.SaveChangesAsync();
        // Não deve lançar exceção
    }

    [Fact]
    public async Task ExisteAsync_DeveRetornarTrueQuandoPessoaExiste()
    {
        // Arrange
        var pessoa = new Pessoa("Roberto", "Silva", "roberto.silva@email.com", new DateTime(1987, 4, 12), "33344455566");
        await _context.Pessoas.AddAsync(pessoa);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ExisteAsync(pessoa.Id);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public async Task ExisteAsync_DeveRetornarFalseQuandoPessoaNaoExiste()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();

        // Act
        var resultado = await _repository.ExisteAsync(idInexistente);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task EmailExisteAsync_DeveRetornarTrueQuandoEmailExiste()
    {
        // Arrange
        var pessoa = new Pessoa("Sandra", "Oliveira", "sandra.oliveira@email.com", new DateTime(1993, 9, 8), "77788899900");
        await _context.Pessoas.AddAsync(pessoa);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.EmailExisteAsync("sandra.oliveira@email.com");

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public async Task EmailExisteAsync_DeveRetornarFalseQuandoEmailNaoExiste()
    {
        // Arrange
        var emailInexistente = "inexistente@email.com";

        // Act
        var resultado = await _repository.EmailExisteAsync(emailInexistente);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task EmailExisteAsync_DeveExcluirIdEspecificado()
    {
        // Arrange
        var pessoa1 = new Pessoa("Bruno", "Costa", "bruno.costa@email.com", new DateTime(1991, 6, 14), "11111111111");
        var pessoa2 = new Pessoa("Carla", "Santos", "carla.santos@email.com", new DateTime(1989, 2, 28), "22222222222");

        await _context.Pessoas.AddRangeAsync(pessoa1, pessoa2);
        await _context.SaveChangesAsync();

        // Act - Verificar se email existe excluindo o próprio ID
        var resultado = await _repository.EmailExisteAsync("bruno.costa@email.com", pessoa1.Id);

        // Assert
        Assert.False(resultado); // Deve retornar false porque excluiu o próprio ID
    }

    [Fact]
    public async Task DocumentoExisteAsync_DeveRetornarTrueQuandoDocumentoExiste()
    {
        // Arrange
        var pessoa = new Pessoa("Diego", "Lima", "diego.lima@email.com", new DateTime(1994, 10, 22), "88899900011");
        await _context.Pessoas.AddAsync(pessoa);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.DocumentoExisteAsync("88899900011");

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public async Task DocumentoExisteAsync_DeveRetornarFalseQuandoDocumentoNaoExiste()
    {
        // Arrange
        var documentoInexistente = "99999999999";

        // Act
        var resultado = await _repository.DocumentoExisteAsync(documentoInexistente);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public async Task DocumentoExisteAsync_DeveExcluirIdEspecificado()
    {
        // Arrange
        var pessoa1 = new Pessoa("Elena", "Fernandes", "elena.fernandes@email.com", new DateTime(1986, 12, 5), "11111111111");
        var pessoa2 = new Pessoa("Felipe", "Rodrigues", "felipe.rodrigues@email.com", new DateTime(1990, 1, 17), "22222222222");

        await _context.Pessoas.AddRangeAsync(pessoa1, pessoa2);
        await _context.SaveChangesAsync();

        // Act - Verificar se documento existe excluindo o próprio ID
        var resultado = await _repository.DocumentoExisteAsync("11111111111", pessoa1.Id);

        // Assert
        Assert.False(resultado); // Deve retornar false porque excluiu o próprio ID
    }

    [Fact]
    public async Task ObterTodosAsync_DeveRetornarListaVaziaQuandoNaoHaPessoas()
    {
        // Act
        var resultado = await _repository.ObterTodosAsync();

        // Assert
        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObterAtivosAsync_DeveRetornarListaVaziaQuandoNaoHaPessoasAtivas()
    {
        // Arrange
        var pessoa = new Pessoa("Gabriela", "Souza", "gabriela.souza@email.com", new DateTime(1992, 8, 30), "33333333333");
        pessoa.Desativar();
        await _context.Pessoas.AddAsync(pessoa);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ObterAtivosAsync();

        // Assert
        Assert.Empty(resultado);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}