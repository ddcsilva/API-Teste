# Template API .NET 8 - Gerenciamento de Pessoas

Este é um template completo de API .NET 8 seguindo **Clean Architecture**, **CQRS** e **boas práticas** de desenvolvimento. O projeto implementa um sistema simples de cadastro de pessoas como exemplo, mas pode ser facilmente adaptado para outros domínios.

## 🚀 Novidades Implementadas

### ✨ **Serilog** - Logging Profissional
- **Substituiu** o logging padrão do ASP.NET Core
- **Logs estruturados** com propriedades contextuais
- **Arquivos de log** com rotação diária (30 dias de retenção)
- **Console colorido** para desenvolvimento
- **Request logging** automático com métricas de performance
- **Níveis de log** configuráveis por ambiente

### 🗃️ **SQLite** - Banco Leve e Funcional
- **Substituiu** InMemory Database por padrão
- **Persistência real** dos dados
- **Migrations automáticas** na inicialização
- **Suporte multi-banco**: SQLite, SQL Server, InMemory
- **Configuração flexível** por arquivo de configuração

### 🏗️ **Estrutura Organizada**
- **src/**: Projetos principais organizados por camada
- **tests/**: Projetos de teste separados por camada
- **Testes unitários** individuais para cada projeto
- **Arquivo .gitignore** completo para .NET

## 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture** com as seguintes camadas:

- **PersonManagement.Domain**: Entidades, interfaces e regras de negócio
- **PersonManagement.Application**: Casos de uso, CQRS handlers, DTOs e validações
- **PersonManagement.Infrastructure**: Implementação de repositórios, Entity Framework e acesso a dados
- **PersonManagement.API**: Controllers, configurações e ponto de entrada da aplicação
- **PersonManagement.Logging**: Abstrações e implementações de logging

## 🚀 Tecnologias Utilizadas

- **.NET 8**
- **ASP.NET Core Web API**
- **Entity Framework Core** (com suporte a SQLite, SQL Server e InMemory)
- **Serilog** (logging estruturado profissional)
- **SQLite** (banco de dados leve e persistente)
- **MediatR** (para implementação do padrão CQRS)
- **AutoMapper** (para mapeamento entre objetos)
- **FluentValidation** (para validação de dados)
- **xUnit** (para testes unitários)
- **Moq** (para mocking em testes)
- **Swagger/OpenAPI** (para documentação da API)

## 📁 Estrutura do Projeto

```
PersonManagement.Template/
├── src/                                    # 📂 Projetos principais
│   ├── PersonManagement.Domain/
│   │   ├── Common/
│   │   │   └── BaseEntity.cs
│   │   ├── Entities/
│   │   │   └── Pessoa.cs
│   │   └── Interfaces/
│   │       ├── IPessoaRepository.cs
│   │       └── IUnitOfWork.cs
│   ├── PersonManagement.Application/
│   │   ├── DTOs/
│   │   │   └── PessoaDto.cs
│   │   ├── Features/
│   │   │   └── Pessoas/
│   │   │       ├── Commands/
│   │   │       │   ├── CriarPessoa/
│   │   │       │   └── AtualizarPessoa/
│   │   │       └── Queries/
│   │   │           ├── ObterPessoaPorId/
│   │   │           └── ObterTodasPessoas/
│   │   └── Mappings/
│   │       └── PessoaMappingProfile.cs
│   ├── PersonManagement.Infrastructure/
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── UnitOfWork.cs
│   │   │   └── Migrations/
│   │   └── Repositories/
│   │       └── PessoaRepository.cs
│   ├── PersonManagement.API/
│   │   ├── Controllers/
│   │   │   └── PessoasController.cs
│   │   ├── Extensions/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   ├── WebApplicationExtensions.cs
│   │   │   └── LoggingExtensions.cs
│   │   ├── Properties/
│   │   │   └── launchSettings.json
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   └── appsettings.Development.json
│   └── PersonManagement.Logging/
│       ├── Abstractions/
│       │   └── IAppLogger.cs
│       ├── Configuration/
│       │   └── LoggingConfiguration.cs
│       └── Implementation/
│           └── SerilogAppLogger.cs
├── tests/                                  # 🧪 Projetos de teste
│   ├── PersonManagement.Domain.Tests/
│   │   └── Domain/
│   │       └── Entities/
│   ├── PersonManagement.Application.Tests/
│   │   └── Application/
│   │       └── Commands/
│   ├── PersonManagement.Infrastructure.Tests/
│   │   └── Infrastructure/
│   │       └── Repositories/
│   └── PersonManagement.API.Tests/
│       └── API/
│           └── Controllers/
├── .gitignore                              # 🚫 Arquivo gitignore completo
├── PersonManagement.Template.sln           # 📋 Solução organizada
├── README.md
├── logs/ (criado automaticamente)
│   └── app-YYYY-MM-DD.log
└── PersonManagement.db (SQLite database)
```

## 🧪 Testes

O projeto possui **testes unitários separados por camada**:

- **PersonManagement.Domain.Tests**: Testes das entidades e regras de domínio
- **PersonManagement.Application.Tests**: Testes dos handlers CQRS e validações
- **PersonManagement.Infrastructure.Tests**: Testes dos repositórios e acesso a dados
- **PersonManagement.API.Tests**: Testes de integração dos controllers

### Executar todos os testes
```bash
dotnet test
```

### Executar testes de uma camada específica
```bash
dotnet test tests/PersonManagement.Domain.Tests
dotnet test tests/PersonManagement.Application.Tests
dotnet test tests/PersonManagement.Infrastructure.Tests
dotnet test tests/PersonManagement.API.Tests
```

## ✨ Destaque: Program.cs Refatorado + Serilog

O `Program.cs` foi **completamente refatorado** usando **métodos de extensão** e **Serilog**:

### ✅ Versão Atual (com Serilog e SQLite)
```csharp
using PersonManagement.API.Extensions;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ===== SERILOG CONFIGURAÇÃO =====
    builder.AddSerilog();

    // ===== CONFIGURAÇÃO DE SERVIÇOS =====
    builder.Services
        .AddApiConfiguration()
        .AddSwaggerDocumentation()
        .AddCorsPolicy()
        .AddApplicationServices()
        .AddInfrastructureServices(builder.Configuration);

    var app = builder.Build();

    // ===== CONFIGURAÇÃO DO PIPELINE =====
    app.UseSerilogMiddleware()
       .ConfigurePipeline()
       .SeedDatabase();

    app.Run();
}
catch (Exception ex)
{
    Serilog.Log.Fatal(ex, "💥 Aplicação falhou ao inicializar");
}
finally
{
    PersonManagement.API.Extensions.LoggingExtensions.LogApplicationShutdown();
}
```

### 🎯 Benefícios das Melhorias

#### 📋 **Serilog**
- **📈 Performance**: Logs estruturados mais eficientes
- **🔍 Debugging**: Contexto rico com propriedades
- **📊 Monitoramento**: Métricas automáticas de requisições
- **📁 Organização**: Arquivos rotacionados automaticamente
- **🎨 Experiência**: Console colorido e formatado

#### 🗃️ **SQLite**
- **💾 Persistência**: Dados salvos entre execuções
- **🚀 Performance**: Acesso rápido sem rede
- **📦 Portabilidade**: Arquivo único, fácil backup
- **🔧 Flexibilidade**: Troca fácil entre bancos
- **🧪 Testes**: Isolamento perfeito para desenvolvimento

#### 🏗️ **Estrutura Organizada**
- **📂 Separação clara**: src/ para código, tests/ para testes
- **🧪 Testes isolados**: Um projeto de teste por camada
- **🚫 .gitignore completo**: Ignora arquivos desnecessários
- **📋 Solução limpa**: Organizada com folders virtuais

## 🔧 Como Executar

### Pré-requisitos
- .NET 8 SDK
- Visual Studio 2022 ou VS Code

### Passos para execução

1. **Clone o repositório**
   ```bash
   git clone <url-do-repositorio>
   cd PersonManagement.Template
   ```

2. **Restaure as dependências**
   ```bash
   dotnet restore
   ```

3. **Execute os testes**
   ```bash
   dotnet test
   ```

4. **Execute a aplicação**
   ```bash
   dotnet run --project src/PersonManagement.API
   ```

5. **Acesse a documentação da API**
   - Swagger UI: `https://localhost:5001` ou `http://localhost:5000`
   - A aplicação automaticamente redireciona para o Swagger

## 📊 Banco de Dados

### Configuração Flexível
O projeto suporta múltiplos bancos através do `appsettings.json`:

```json
{
  "DatabaseType": "Sqlite", // "Sqlite", "SqlServer", "InMemory"
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=PersonManagement.db",
    "SqlServerConnection": "Server=localhost;Database=PersonManagement;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### Migrations
```bash
# Adicionar nova migration
dotnet ef migrations add NomeDaMigration --project src/PersonManagement.Infrastructure --startup-project src/PersonManagement.API

# Atualizar banco
dotnet ef database update --project src/PersonManagement.Infrastructure --startup-project src/PersonManagement.API
```

## 🔄 Como Adaptar para Outro Domínio

Este template pode ser facilmente adaptado para outros domínios:

1. **Renomeie as entidades**: `Pessoa` → `SuaEntidade`
2. **Atualize os namespaces**: `PersonManagement` → `SeuDominio`
3. **Modifique os DTOs e Commands**: Adapte para suas necessidades
4. **Ajuste as validações**: Configure as regras do seu domínio
5. **Atualize os testes**: Adapte os cenários de teste

## 🤝 Contribuição

Contribuições são bem-vindas! Sinta-se à vontade para:

- Reportar bugs
- Sugerir melhorias
- Enviar pull requests
- Compartilhar feedback

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

**Template criado com ❤️ para acelerar o desenvolvimento de APIs .NET 8 com Clean Architecture e CQRS**