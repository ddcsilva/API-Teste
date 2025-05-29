using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PersonManagement.Application.Features.Pessoas.Commands.CriarPessoa;
using PersonManagement.Infrastructure.Data;
using PersonManagement.API;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PersonManagement.API.Tests.API.Controllers;

public class PessoasControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly IServiceScope _scope;
    private readonly ApplicationDbContext _context;

    public PessoasControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // Remove o contexto de banco existente
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Adiciona contexto InMemory para testes com nome fixo
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_API");
                });
            });
        });

        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Limpar banco antes de cada teste
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    private async Task LimparBanco()
    {
        _context.Pessoas.RemoveRange(_context.Pessoas);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GET_ObterTodas_DeveRetornarListaVaziaQuandoNaoHaPessoas()
    {
        // Arrange
        await LimparBanco();

        // Act
        var response = await _client.GetAsync("/api/Pessoas");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var pessoas = JsonSerializer.Deserialize<object[]>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(pessoas);
        Assert.Empty(pessoas);
    }

    [Fact]
    public async Task POST_Criar_DeveCriarPessoaComSucesso()
    {
        // Arrange
        await LimparBanco();

        var command = new CriarPessoaCommand
        {
            Nome = "João",
            Sobrenome = "Silva",
            Email = "joao.silva.teste@email.com",
            DataNascimento = new DateTime(1990, 5, 15),
            Documento = "12345678901"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Pessoas", command);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var location = response.Headers.Location;
        Assert.NotNull(location);
        Assert.Contains("/api/Pessoas/", location.ToString());

        var content = await response.Content.ReadAsStringAsync();
        var pessoaCriada = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.Equal("João", pessoaCriada.GetProperty("nome").GetString());
        Assert.Equal("Silva", pessoaCriada.GetProperty("sobrenome").GetString());
        Assert.Equal("joao.silva.teste@email.com", pessoaCriada.GetProperty("email").GetString());
    }

    [Fact]
    public async Task POST_Criar_DeveRetornarBadRequestParaDadosInvalidos()
    {
        // Arrange
        await LimparBanco();

        var command = new CriarPessoaCommand
        {
            Nome = "", // Nome vazio
            Sobrenome = "Silva",
            Email = "email-invalido", // Email inválido
            DataNascimento = DateTime.Today.AddDays(1), // Data futura
            Documento = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Pessoas", command);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GET_ObterPorId_DeveRetornarPessoaQuandoExiste()
    {
        // Arrange
        await LimparBanco();

        // Criar pessoa diretamente no banco
        var pessoa = new PersonManagement.Domain.Entities.Pessoa(
            "Maria", "Santos", "maria.santos.teste@email.com",
            new DateTime(1985, 8, 22), "98765432109");

        _context.Pessoas.Add(pessoa);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/Pessoas/{pessoa.Id}");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var pessoaRetornada = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.Equal("Maria", pessoaRetornada.GetProperty("nome").GetString());
        Assert.Equal("Santos", pessoaRetornada.GetProperty("sobrenome").GetString());
        Assert.Equal("maria.santos.teste@email.com", pessoaRetornada.GetProperty("email").GetString());
        Assert.Equal("Maria Santos", pessoaRetornada.GetProperty("nomeCompleto").GetString());
    }

    [Fact]
    public async Task GET_ObterPorId_DeveRetornarNotFoundParaIdInexistente()
    {
        // Arrange
        await LimparBanco();
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/Pessoas/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PUT_Atualizar_DeveAtualizarPessoaComSucesso()
    {
        // Arrange
        await LimparBanco();

        // Criar pessoa diretamente no banco
        var pessoa = new PersonManagement.Domain.Entities.Pessoa(
            "Pedro", "Oliveira", "pedro.oliveira.teste@email.com",
            new DateTime(1992, 12, 3), "11122233344");

        _context.Pessoas.Add(pessoa);
        await _context.SaveChangesAsync();

        var updateCommand = new
        {
            Nome = "Pedro Atualizado",
            Sobrenome = "Oliveira Santos",
            Email = "pedro.atualizado@email.com",
            DataNascimento = new DateTime(1992, 12, 3)
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Pessoas/{pessoa.Id}", updateCommand);

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var pessoaAtualizada = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.Equal("Pedro Atualizado", pessoaAtualizada.GetProperty("nome").GetString());
        Assert.Equal("Oliveira Santos", pessoaAtualizada.GetProperty("sobrenome").GetString());
        Assert.Equal("pedro.atualizado@email.com", pessoaAtualizada.GetProperty("email").GetString());
        Assert.Equal("Pedro Atualizado Oliveira Santos", pessoaAtualizada.GetProperty("nomeCompleto").GetString());
    }

    [Fact]
    public async Task PUT_Atualizar_DeveRetornarNotFoundParaIdInexistente()
    {
        // Arrange
        await LimparBanco();
        var idInexistente = Guid.NewGuid();
        var updateCommand = new
        {
            Nome = "Nome",
            Sobrenome = "Sobrenome",
            Email = "email@test.com",
            DataNascimento = new DateTime(1990, 1, 1)
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Pessoas/{idInexistente}", updateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PUT_Atualizar_DeveRetornarBadRequestParaEmailDuplicado()
    {
        // Arrange
        await LimparBanco();

        // Criar duas pessoas diretamente no banco
        var pessoa1 = new PersonManagement.Domain.Entities.Pessoa(
            "João", "Silva", "joao@email.com",
            new DateTime(1990, 5, 15), "12345678901");

        var pessoa2 = new PersonManagement.Domain.Entities.Pessoa(
            "Maria", "Santos", "maria@email.com",
            new DateTime(1985, 8, 22), "98765432109");

        _context.Pessoas.AddRange(pessoa1, pessoa2);
        await _context.SaveChangesAsync();

        // Tentar atualizar pessoa2 com o email da pessoa1
        var updateCommand = new
        {
            Nome = "Maria",
            Sobrenome = "Santos",
            Email = "joao@email.com", // Email que já existe na pessoa1
            DataNascimento = new DateTime(1985, 8, 22)
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Pessoas/{pessoa2.Id}", updateCommand);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.Contains("Email já existe", error.GetProperty("mensagem").GetString());
        Assert.True(error.TryGetProperty("erros", out var erros));
        Assert.Contains("Email já existe", erros.EnumerateArray().First().GetString());
    }

    [Fact]
    public async Task GET_ObterTodas_DeveRetornarTodasAsPessoas()
    {
        // Arrange
        await LimparBanco();

        // Criar pessoas diretamente no banco
        var pessoas = new[]
        {
            new PersonManagement.Domain.Entities.Pessoa("Ana", "Costa", "ana.costa.teste@email.com", new DateTime(1988, 3, 10), "55566677788"),
            new PersonManagement.Domain.Entities.Pessoa("Carlos", "Lima", "carlos.lima.teste@email.com", new DateTime(1995, 11, 25), "99988877766")
        };

        _context.Pessoas.AddRange(pessoas);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/Pessoas");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<JsonElement[]>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.Length);
        Assert.Contains(resultado, p => p.GetProperty("nome").GetString() == "Ana");
        Assert.Contains(resultado, p => p.GetProperty("nome").GetString() == "Carlos");
    }

    [Fact]
    public async Task GET_ObterTodas_ComApenasAtivos_DeveRetornarApenasPessoasAtivas()
    {
        // Arrange
        await LimparBanco();

        var pessoa1 = new PersonManagement.Domain.Entities.Pessoa("Ativo", "Teste", "ativo@test.com", new DateTime(1990, 1, 1), "11111111111");
        var pessoa2 = new PersonManagement.Domain.Entities.Pessoa("Inativo", "Teste", "inativo@test.com", new DateTime(1990, 1, 1), "22222222222");
        pessoa2.Desativar();

        _context.Pessoas.AddRange(pessoa1, pessoa2);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync("/api/Pessoas?apenasAtivos=true");

        // Assert
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<JsonElement[]>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(resultado);
        Assert.Single(resultado);
        Assert.Equal("Ativo", resultado[0].GetProperty("nome").GetString());
        Assert.True(resultado[0].GetProperty("estaAtivo").GetBoolean());
    }

    [Fact]
    public async Task POST_Criar_DeveRetornarBadRequestParaEmailDuplicado()
    {
        // Arrange
        await LimparBanco();

        // Criar primeira pessoa diretamente no banco
        var pessoaExistente = new PersonManagement.Domain.Entities.Pessoa(
            "João", "Silva", "duplicado@email.com",
            new DateTime(1990, 5, 15), "12345678901");

        _context.Pessoas.Add(pessoaExistente);
        await _context.SaveChangesAsync();

        // Tentar criar outra pessoa com o mesmo email
        var command2 = new CriarPessoaCommand
        {
            Nome = "Maria",
            Sobrenome = "Santos",
            Email = "duplicado@email.com", // Email duplicado
            DataNascimento = new DateTime(1985, 8, 22),
            Documento = "98765432109"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Pessoas", command2);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.Contains("Email já existe", error.GetProperty("mensagem").GetString());
        Assert.True(error.TryGetProperty("erros", out var erros));
        Assert.Contains("Email já existe", erros.EnumerateArray().First().GetString());
    }

    [Fact]
    public async Task POST_Criar_DeveRetornarBadRequestParaDocumentoDuplicado()
    {
        // Arrange
        await LimparBanco();

        // Criar primeira pessoa diretamente no banco
        var pessoaExistente = new PersonManagement.Domain.Entities.Pessoa(
            "João", "Silva", "joao@email.com",
            new DateTime(1990, 5, 15), "12345678901");

        _context.Pessoas.Add(pessoaExistente);
        await _context.SaveChangesAsync();

        // Tentar criar outra pessoa com o mesmo documento
        var command2 = new CriarPessoaCommand
        {
            Nome = "Maria",
            Sobrenome = "Santos",
            Email = "maria@email.com",
            DataNascimento = new DateTime(1985, 8, 22),
            Documento = "12345678901" // Documento duplicado
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Pessoas", command2);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.Contains("Documento já existe", error.GetProperty("mensagem").GetString());
        Assert.True(error.TryGetProperty("erros", out var erros));
        Assert.Contains("Documento já existe", erros.EnumerateArray().First().GetString());
    }

    [Fact]
    public async Task POST_Criar_DeveRetornarBadRequestComMultiplosErrosParaEmailEDocumentoDuplicados()
    {
        // Arrange
        await LimparBanco();

        // Criar primeira pessoa diretamente no banco
        var pessoaExistente = new PersonManagement.Domain.Entities.Pessoa(
            "João", "Silva", "duplicado@email.com",
            new DateTime(1990, 5, 15), "12345678901");

        _context.Pessoas.Add(pessoaExistente);
        await _context.SaveChangesAsync();

        // Tentar criar outra pessoa com email E documento duplicados
        var command2 = new CriarPessoaCommand
        {
            Nome = "Maria",
            Sobrenome = "Santos",
            Email = "duplicado@email.com", // Email duplicado
            DataNascimento = new DateTime(1985, 8, 22),
            Documento = "12345678901" // Documento também duplicado
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Pessoas", command2);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Verificar mensagem combinada
        var mensagem = error.GetProperty("mensagem").GetString();
        Assert.Contains("Email já existe", mensagem);
        Assert.Contains("Documento já existe", mensagem);

        // Verificar array de erros
        Assert.True(error.TryGetProperty("erros", out var erros));
        var errosArray = erros.EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Equal(2, errosArray.Length);
        Assert.Contains("Email já existe", errosArray);
        Assert.Contains("Documento já existe", errosArray);
    }

    [Fact]
    public async Task DELETE_Excluir_DeveExcluirPessoaComSucesso()
    {
        // Arrange
        await LimparBanco();

        // Criar pessoa diretamente no banco
        var pessoa = new PersonManagement.Domain.Entities.Pessoa(
            "João", "Silva", "joao.exclusao@email.com",
            new DateTime(1990, 5, 15), "12345678901");

        _context.Pessoas.Add(pessoa);
        await _context.SaveChangesAsync();

        // Act
        var response = await _client.DeleteAsync($"/api/Pessoas/{pessoa.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verificar se a pessoa foi realmente excluída - usar nova consulta ao invés de cache
        var pessoaExcluida = await _context.Pessoas.FirstOrDefaultAsync(p => p.Id == pessoa.Id);
        Assert.Null(pessoaExcluida);
    }

    [Fact]
    public async Task DELETE_Excluir_DeveRetornarNotFoundParaIdInexistente()
    {
        // Arrange
        await LimparBanco();
        var idInexistente = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/Pessoas/{idInexistente}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<JsonElement>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.Equal("Pessoa não encontrada", error.GetProperty("mensagem").GetString());
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        _scope.Dispose();
        _client.Dispose();
    }
}