using PersonManagement.Domain.Common;

namespace PersonManagement.Domain.Entities;

public class Pessoa : BaseEntity
{
    public string Nome { get; private set; } = string.Empty;
    public string Sobrenome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime DataNascimento { get; private set; }
    public string Documento { get; private set; } = string.Empty;
    public bool EstaAtivo { get; private set; } = true;

    protected Pessoa() { } // Requerido pelo EF Core

    public Pessoa(string nome, string sobrenome, string email, DateTime dataNascimento, string documento)
    {
        Nome = nome;
        Sobrenome = sobrenome;
        Email = email;
        DataNascimento = dataNascimento;
        Documento = documento;
        EstaAtivo = true;
        DataCriacao = DateTime.UtcNow;
    }

    public void AtualizarInformacoesPessoais(string nome, string sobrenome, string email, DateTime dataNascimento)
    {
        Nome = nome;
        Sobrenome = sobrenome;
        Email = email;
        DataNascimento = dataNascimento;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Ativar()
    {
        EstaAtivo = true;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Desativar()
    {
        EstaAtivo = false;
        DataAtualizacao = DateTime.UtcNow;
    }

    public string ObterNomeCompleto() => $"{Nome} {Sobrenome}";

    public int ObterIdade()
    {
        var hoje = DateTime.Today;
        var idade = hoje.Year - DataNascimento.Year;
        if (DataNascimento.Date > hoje.AddYears(-idade)) idade--;
        return idade;
    }
}