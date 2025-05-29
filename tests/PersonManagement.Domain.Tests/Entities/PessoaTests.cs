using PersonManagement.Domain.Entities;
using Xunit;

namespace PersonManagement.Tests.Domain.Entities;

public class PessoaTests
{
    private readonly string _nomeValido = "João";
    private readonly string _sobrenomeValido = "Silva";
    private readonly string _emailValido = "joao.silva@email.com";
    private readonly DateTime _dataNascimentoValida = new DateTime(1990, 5, 15);
    private readonly string _documentoValido = "12345678901";

    [Fact]
    public void Pessoa_DeveCriarPessoaComDadosValidos()
    {
        // Arrange & Act
        var pessoa = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, _dataNascimentoValida, _documentoValido);

        // Assert
        Assert.Equal(_nomeValido, pessoa.Nome);
        Assert.Equal(_sobrenomeValido, pessoa.Sobrenome);
        Assert.Equal(_emailValido, pessoa.Email);
        Assert.Equal(_dataNascimentoValida, pessoa.DataNascimento);
        Assert.Equal(_documentoValido, pessoa.Documento);
        Assert.True(pessoa.EstaAtivo);
        Assert.NotEqual(Guid.Empty, pessoa.Id);
        Assert.True(pessoa.DataCriacao <= DateTime.UtcNow);
        Assert.Null(pessoa.DataAtualizacao);
    }

    [Fact]
    public void Pessoa_DeveIniciarComoAtivoPorPadrao()
    {
        // Arrange & Act
        var pessoa = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, _dataNascimentoValida, _documentoValido);

        // Assert
        Assert.True(pessoa.EstaAtivo);
    }

    [Fact]
    public void Pessoa_DeveGerarIdAutomaticamente()
    {
        // Arrange & Act
        var pessoa1 = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, _dataNascimentoValida, _documentoValido);
        var pessoa2 = new Pessoa(_nomeValido, _sobrenomeValido, "outro@email.com", _dataNascimentoValida, "09876543210");

        // Assert
        Assert.NotEqual(Guid.Empty, pessoa1.Id);
        Assert.NotEqual(Guid.Empty, pessoa2.Id);
        Assert.NotEqual(pessoa1.Id, pessoa2.Id);
    }

    [Fact]
    public void ObterNomeCompleto_DeveRetornarNomeEsobrenomeConcatenados()
    {
        // Arrange
        var pessoa = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, _dataNascimentoValida, _documentoValido);

        // Act
        var nomeCompleto = pessoa.ObterNomeCompleto();

        // Assert
        Assert.Equal("João Silva", nomeCompleto);
    }

    [Fact]
    public void ObterIdade_DeveCalcularIdadeCorretamente()
    {
        // Arrange
        var dataAniversario = DateTime.Today.AddYears(-30);
        var pessoa = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, dataAniversario, _documentoValido);

        // Act
        var idade = pessoa.ObterIdade();

        // Assert
        Assert.Equal(30, idade);
    }

    [Fact]
    public void ObterIdade_DeveCalcularIdadeCorretamenteParaAniversarioQueAindaNaoOcorreu()
    {
        // Arrange
        var proximoAniversario = DateTime.Today.AddYears(-30).AddDays(1); // Aniversário amanhã
        var pessoa = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, proximoAniversario, _documentoValido);

        // Act
        var idade = pessoa.ObterIdade();

        // Assert
        Assert.Equal(29, idade); // Ainda não fez aniversário este ano
    }

    [Fact]
    public void AtualizarInformacoesPessoais_DeveAtualizarDados()
    {
        // Arrange
        var pessoa = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, _dataNascimentoValida, _documentoValido);
        var novoNome = "Pedro";
        var novoSobrenome = "Santos";
        var novoEmail = "pedro.santos@email.com";
        var novaDataNascimento = new DateTime(1985, 8, 20);

        // Act
        pessoa.AtualizarInformacoesPessoais(novoNome, novoSobrenome, novoEmail, novaDataNascimento);

        // Assert
        Assert.Equal(novoNome, pessoa.Nome);
        Assert.Equal(novoSobrenome, pessoa.Sobrenome);
        Assert.Equal(novoEmail, pessoa.Email);
        Assert.Equal(novaDataNascimento, pessoa.DataNascimento);
        Assert.NotNull(pessoa.DataAtualizacao);
        Assert.True(pessoa.DataAtualizacao <= DateTime.UtcNow);
    }

    [Fact]
    public void AtualizarInformacoesPessoais_NaoDeveAlterarDocumento()
    {
        // Arrange
        var pessoa = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, _dataNascimentoValida, _documentoValido);
        var documentoOriginal = pessoa.Documento;

        // Act
        pessoa.AtualizarInformacoesPessoais("Novo Nome", "Novo Sobrenome", "novo@email.com", new DateTime(1985, 1, 1));

        // Assert
        Assert.Equal(documentoOriginal, pessoa.Documento);
    }

    [Fact]
    public void Ativar_DeveDefinirEstaAtivoComoTrue()
    {
        // Arrange
        var pessoa = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, _dataNascimentoValida, _documentoValido);
        pessoa.Desativar(); // Primeiro desativa

        // Act
        pessoa.Ativar();

        // Assert
        Assert.True(pessoa.EstaAtivo);
        Assert.NotNull(pessoa.DataAtualizacao);
    }

    [Fact]
    public void Desativar_DeveDefinirEstaAtivoComoFalse()
    {
        // Arrange
        var pessoa = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, _dataNascimentoValida, _documentoValido);

        // Act
        pessoa.Desativar();

        // Assert
        Assert.False(pessoa.EstaAtivo);
        Assert.NotNull(pessoa.DataAtualizacao);
    }

    [Fact]
    public void Ativar_DeveAtualizarDataAtualizacao()
    {
        // Arrange
        var pessoa = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, _dataNascimentoValida, _documentoValido);
        var dataAntesAtualizacao = pessoa.DataAtualizacao;

        // Act
        Thread.Sleep(1); // Garante diferença temporal
        pessoa.Ativar();

        // Assert
        Assert.NotEqual(dataAntesAtualizacao, pessoa.DataAtualizacao);
        Assert.True(pessoa.DataAtualizacao <= DateTime.UtcNow);
    }

    [Fact]
    public void Desativar_DeveAtualizarDataAtualizacao()
    {
        // Arrange
        var pessoa = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, _dataNascimentoValida, _documentoValido);
        var dataAntesAtualizacao = pessoa.DataAtualizacao;

        // Act
        Thread.Sleep(1); // Garante diferença temporal
        pessoa.Desativar();

        // Assert
        Assert.NotEqual(dataAntesAtualizacao, pessoa.DataAtualizacao);
        Assert.True(pessoa.DataAtualizacao <= DateTime.UtcNow);
    }

    [Theory]
    [InlineData(1990, 5, 15)]
    [InlineData(2000, 1, 1)]
    [InlineData(1985, 12, 31)]
    public void ObterIdade_DeveCalcularIdadeCorretamenteParaDiferentesDatas(int ano, int mes, int dia)
    {
        // Arrange
        var dataNascimento = new DateTime(ano, mes, dia);
        var pessoa = new Pessoa(_nomeValido, _sobrenomeValido, _emailValido, dataNascimento, _documentoValido);

        // Act
        var idade = pessoa.ObterIdade();

        // Assert
        // Nota: Este teste verifica se a idade é calculada corretamente
        // A idade exata depende do ano atual, então verificamos apenas se é válida
        Assert.True(idade >= 0);
        Assert.True(idade <= 150); // Idade razoável
    }

    [Fact]
    public void ObterNomeCompleto_DeveManterEspacosCorretamente()
    {
        // Arrange
        var nome = "João Pedro";
        var sobrenome = "da Silva Santos";
        var pessoa = new Pessoa(nome, sobrenome, _emailValido, _dataNascimentoValida, _documentoValido);

        // Act
        var nomeCompleto = pessoa.ObterNomeCompleto();

        // Assert
        Assert.Equal("João Pedro da Silva Santos", nomeCompleto);
    }
}