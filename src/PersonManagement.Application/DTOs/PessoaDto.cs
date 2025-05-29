namespace PersonManagement.Application.DTOs;

public class PessoaDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Sobrenome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DataNascimento { get; set; }
    public string Documento { get; set; } = string.Empty;
    public bool EstaAtivo { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public int Idade { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}