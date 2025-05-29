using MediatR;
using Microsoft.AspNetCore.Mvc;
using PersonManagement.Application.Features.Pessoas.Commands.CriarPessoa;
using PersonManagement.Application.Features.Pessoas.Commands.AtualizarPessoa;
using PersonManagement.Application.Features.Pessoas.Queries.ObterTodasPessoas;
using PersonManagement.Application.Features.Pessoas.Queries.ObterPessoaPorId;
using PersonManagement.Logging.Abstractions;
using PersonManagement.Domain.Common;
using System.Diagnostics;

namespace PersonManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PessoasController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAppLogger<PessoasController> _logger;

    public PessoasController(IMediator mediator, IAppLogger<PessoasController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obter todas as pessoas
    /// </summary>
    /// <param name="apenasAtivos">Filtrar apenas pessoas ativas</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de pessoas</returns>
    [HttpGet]
    public async Task<IActionResult> ObterTodas([FromQuery] bool apenasAtivos = false, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogWithContext(LogLevel.Information, "🔍 Iniciando busca de pessoas. ApenasAtivos: {ApenasAtivos}", apenasAtivos);

            var query = new ObterTodasPessoasQuery(apenasAtivos);
            var resultado = await _mediator.Send(query, cancellationToken);

            stopwatch.Stop();
            _logger.LogPerformance("ObterTodasPessoas", stopwatch.Elapsed, new { ApenasAtivos = apenasAtivos, Count = resultado?.Count() ?? 0 });

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Erro ao buscar pessoas. ApenasAtivos: {ApenasAtivos}", apenasAtivos);
            return StatusCode(500, new { mensagem = "Ocorreu um erro ao buscar as pessoas", erro = ex.Message });
        }
    }

    /// <summary>
    /// Obter pessoa por ID
    /// </summary>
    /// <param name="id">ID da pessoa</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Detalhes da pessoa</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogWithContext(LogLevel.Information, "🔍 Buscando pessoa por ID: {PessoaId}", id);

            var query = new ObterPessoaPorIdQuery(id);
            var resultado = await _mediator.Send(query, cancellationToken);

            stopwatch.Stop();

            if (resultado.IsFailure)
            {
                _logger.LogWarning("⚠️ Pessoa não encontrada. ID: {PessoaId}", id);
                return NotFound(new { mensagem = resultado.ErrorMessage });
            }

            _logger.LogPerformance("ObterPessoaPorId", stopwatch.Elapsed, new { PessoaId = id });
            return Ok(resultado.Value);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Erro ao buscar pessoa por ID: {PessoaId}", id);
            return StatusCode(500, new { mensagem = "Ocorreu um erro ao buscar a pessoa", erro = ex.Message });
        }
    }

    /// <summary>
    /// Criar uma nova pessoa
    /// </summary>
    /// <param name="command">Dados para criação da pessoa</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Pessoa criada</returns>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarPessoaCommand command, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogWithContext(LogLevel.Information, "➕ Criando nova pessoa: {Nome} {Sobrenome}", command.Nome, command.Sobrenome);

            var resultado = await _mediator.Send(command, cancellationToken);

            stopwatch.Stop();

            if (resultado.IsFailure)
            {
                _logger.LogWarning("⚠️ Falha na validação ao criar pessoa: {Erros}", string.Join(", ", resultado.Errors));
                return BadRequest(new { mensagem = resultado.ErrorMessage, erros = resultado.Errors });
            }

            _logger.LogPerformance("CriarPessoa", stopwatch.Elapsed, new { PessoaId = resultado.Value.Id, Nome = command.Nome });
            _logger.LogAudit("CriarPessoa", "Sistema", new { PessoaId = resultado.Value.Id, Nome = command.Nome, Sobrenome = command.Sobrenome });

            return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Value.Id }, resultado.Value);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Erro ao criar pessoa: {Nome} {Sobrenome}", command.Nome, command.Sobrenome);
            return StatusCode(500, new { mensagem = "Ocorreu um erro interno ao criar a pessoa", erro = ex.Message });
        }
    }

    /// <summary>
    /// Atualizar uma pessoa existente
    /// </summary>
    /// <param name="id">ID da pessoa</param>
    /// <param name="command">Dados para atualização da pessoa</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Pessoa atualizada</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarPessoaCommand command, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            command.Id = id;
            _logger.LogWithContext(LogLevel.Information, "✏️ Atualizando pessoa: {PessoaId}", id);

            var resultado = await _mediator.Send(command, cancellationToken);

            stopwatch.Stop();

            if (resultado.IsFailure)
            {
                _logger.LogWarning("⚠️ Falha ao atualizar pessoa: {Erros}", string.Join(", ", resultado.Errors));

                if (resultado.ErrorMessage == "Pessoa não encontrada")
                {
                    return NotFound(new { mensagem = resultado.ErrorMessage });
                }

                return BadRequest(new { mensagem = resultado.ErrorMessage, erros = resultado.Errors });
            }

            _logger.LogPerformance("AtualizarPessoa", stopwatch.Elapsed, new { PessoaId = id });
            _logger.LogAudit("AtualizarPessoa", "Sistema", new { PessoaId = id, Nome = command.Nome, Sobrenome = command.Sobrenome });

            return Ok(resultado.Value);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Erro ao atualizar pessoa: {PessoaId}", id);
            return StatusCode(500, new { mensagem = "Ocorreu um erro interno ao atualizar a pessoa", erro = ex.Message });
        }
    }
}