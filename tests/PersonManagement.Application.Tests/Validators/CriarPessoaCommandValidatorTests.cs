using PersonManagement.Application.Features.Pessoas.Commands.CriarPessoa;

namespace PersonManagement.Application.Tests.Application.Validators;

public class CriarPessoaCommandValidatorTests
{
    private readonly CriarPessoaCommandValidator _validator;

    public CriarPessoaCommandValidatorTests()
    {
        _validator = new CriarPessoaCommandValidator();
    }

    [Fact]
    public void Validacao_DevePassarParaCommandValido()
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

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.True(resultado.IsValid);
        Assert.Empty(resultado.Errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validacao_DeveFalharQuandoNomeEstaVazio(string nome)
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = nome,
            Sobrenome = "Silva",
            Email = "joao.silva@email.com",
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriarPessoaCommand.Nome));
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Nome é obrigatório");
    }

    [Fact]
    public void Validacao_DeveFalharQuandoNomeExcedeTamanhoMaximo()
    {
        // Arrange
        var nomeGrande = new string('A', 101); // 101 caracteres
        var command = new CriarPessoaCommand
        {
            Nome = nomeGrande,
            Sobrenome = "Silva",
            Email = "joao.silva@email.com",
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriarPessoaCommand.Nome));
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Nome não pode exceder 100 caracteres");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validacao_DeveFalharQuandoSobrenomeEstaVazio(string sobrenome)
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = sobrenome,
            Email = "joao.silva@email.com",
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriarPessoaCommand.Sobrenome));
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Sobrenome é obrigatório");
    }

    [Fact]
    public void Validacao_DeveFalharQuandoSobrenomeExcedeTamanhoMaximo()
    {
        // Arrange
        var sobrenomeGrande = new string('B', 101); // 101 caracteres
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = sobrenomeGrande,
            Email = "joao.silva@email.com",
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriarPessoaCommand.Sobrenome));
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Sobrenome não pode exceder 100 caracteres");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validacao_DeveFalharQuandoEmailEstaVazio(string email)
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = email,
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriarPessoaCommand.Email));
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Email é obrigatório");
    }

    [Theory]
    [InlineData("email-invalido")]
    [InlineData("@email.com")]
    [InlineData("email@")]
    [InlineData("email.com")]
    [InlineData("email..duplo@email.com")]
    [InlineData("email@email")]
    public void Validacao_DeveFalharQuandoEmailEstaInvalido(string emailInvalido)
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = emailInvalido,
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriarPessoaCommand.Email));
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Email deve ser um endereço válido");
    }

    [Fact]
    public void Validacao_DeveFalharQuandoEmailExcedeTamanhoMaximo()
    {
        // Arrange
        var emailGrande = new string('a', 246) + "@email.com"; // 256 caracteres (> 255)
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = emailGrande,
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriarPessoaCommand.Email));
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Email deve ser um endereço válido" || e.ErrorMessage == "Email não pode exceder 255 caracteres");
    }

    [Fact]
    public void Validacao_DeveFalharQuandoDataNascimentoEstaVazia()
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao.silva@email.com",
            DataNascimento = default(DateTime),
            Documento = "12345678901"
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriarPessoaCommand.DataNascimento));
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Data de nascimento é obrigatória");
    }

    [Fact]
    public void Validacao_DeveFalharQuandoDataNascimentoEstaNoFuturo()
    {
        // Arrange
        var dataFutura = DateTime.Today.AddDays(1);
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao.silva@email.com",
            DataNascimento = dataFutura,
            Documento = "12345678901"
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriarPessoaCommand.DataNascimento));
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Data de nascimento deve ser no passado");
    }

    [Fact]
    public void Validacao_DeveFalharQuandoDataNascimentoEhMuitoAntiga()
    {
        // Arrange
        var dataMuitoAntiga = DateTime.Today.AddYears(-121); // Mais de 120 anos
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao.silva@email.com",
            DataNascimento = dataMuitoAntiga,
            Documento = "12345678901"
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriarPessoaCommand.DataNascimento));
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Data de nascimento deve estar dentro dos últimos 120 anos");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validacao_DeveFalharQuandoDocumentoEstaVazio(string documento)
    {
        // Arrange
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao.silva@email.com",
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = documento
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriarPessoaCommand.Documento));
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Documento é obrigatório");
    }

    [Fact]
    public void Validacao_DeveFalharQuandoDocumentoExcedeTamanhoMaximo()
    {
        // Arrange
        var documentoGrande = new string('1', 21); // 21 caracteres
        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao.silva@email.com",
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = documentoGrande
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors, e => e.PropertyName == nameof(CriarPessoaCommand.Documento));
        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "Documento não pode exceder 20 caracteres");
    }

    [Fact]
    public void Validacao_DevePermitirEmailsValidosVariados()
    {
        // Arrange
        var emailsValidos = new[]
        {
            "test@example.com",
            "user.name@domain.co.uk",
            "user+tag@example.com",
            "123@numeric-domain.com",
            "a@b.co"
        };

        foreach (var email in emailsValidos)
        {
            var command = new CriarPessoaCommand
            {
                Nome = "João",
                Sobrenome = "Silva",
                Email = email,
                DataNascimento = new DateTime(1990, 5, 15),
                Documento = "12345678901"
            };

            // Act
            var resultado = _validator.Validate(command);

            // Assert
            Assert.True(resultado.IsValid, $"Email {email} deveria ser válido");
        }
    }

    [Fact]
    public void Validacao_DevePermitirNomesEDocumentosNoLimite()
    {
        // Arrange
        var nome100Chars = new string('A', 100);
        var sobrenome100Chars = new string('B', 100);
        var email255Chars = new string('c', 245) + "@email.com"; // Exatamente 255 caracteres
        var documento20Chars = new string('1', 20);

        var command = new CriarPessoaCommand
        {
            Nome = nome100Chars,
            Sobrenome = sobrenome100Chars,
            Email = email255Chars,
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = documento20Chars
        };

        // Act
        var resultado = _validator.Validate(command);

        // Assert
        Assert.True(resultado.IsValid);
    }
}