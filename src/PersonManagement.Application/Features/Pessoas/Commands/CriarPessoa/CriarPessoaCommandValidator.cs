using FluentValidation;
using System.Text.RegularExpressions;

namespace PersonManagement.Application.Features.Pessoas.Commands.CriarPessoa;

public partial class CriarPessoaCommandValidator : AbstractValidator<CriarPessoaCommand>
{
    // Regex com GeneratedRegex para melhor performance
    [GeneratedRegex(@"^[a-zA-Z0-9]+([a-zA-Z0-9._+-]*[a-zA-Z0-9])?@[a-zA-Z0-9]+([a-zA-Z0-9.-]*[a-zA-Z0-9])?\.[a-zA-Z]{2,}$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    public CriarPessoaCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(100).WithMessage("Nome não pode exceder 100 caracteres");

        RuleFor(x => x.Sobrenome)
            .NotEmpty().WithMessage("Sobrenome é obrigatório")
            .MaximumLength(100).WithMessage("Sobrenome não pode exceder 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .Must(BeAValidEmail).WithMessage("Email deve ser um endereço válido")
            .MaximumLength(255).WithMessage("Email não pode exceder 255 caracteres");

        RuleFor(x => x.DataNascimento)
            .NotEmpty().WithMessage("Data de nascimento é obrigatória")
            .LessThan(DateTime.Today).WithMessage("Data de nascimento deve ser no passado")
            .GreaterThan(DateTime.Today.AddYears(-120)).WithMessage("Data de nascimento deve estar dentro dos últimos 120 anos");

        RuleFor(x => x.Documento)
            .NotEmpty().WithMessage("Documento é obrigatório")
            .MaximumLength(20).WithMessage("Documento não pode exceder 20 caracteres");
    }

    private static bool BeAValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        // Verificação de tamanho primeiro
        if (email.Length > 255)
            return false;

        if (email.Contains(".."))
            return false;

        // Usar char ao invés de string para melhor performance
        if (email.StartsWith('.') || email.EndsWith('.'))
            return false;

        if (email.StartsWith('@') || email.EndsWith('@'))
            return false;

        // Deve ter exatamente um @
        var atCount = email.Count(c => c == '@');
        if (atCount != 1)
            return false;

        // Verificar se tem pelo menos um caractere antes e depois do @
        var parts = email.Split('@');
        if (parts.Length != 2 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
            return false;

        // Usar GeneratedRegex
        return EmailRegex().IsMatch(email);
    }
}
