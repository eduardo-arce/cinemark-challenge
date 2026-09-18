# Cinemark Movie Catalog API

API REST para gerenciamento de catálogo de filmes, com notificações assíncronas via fila. Construída em .NET 9, persistência em MongoDB, cache distribuído em Redis, mensageria via Amazon SQS (LocalStack) e ambiente containerizado via Docker Compose.

## Sumário

- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Pré-requisitos](#pré-requisitos)
- [Executando o projeto](#executando-o-projeto)
- [Executando localmente (sem Docker para a API)](#executando-localmente-sem-docker-para-a-api)
- [Endpoints da API](#endpoints-da-api)
- [Tratamento de erros](#tratamento-de-erros)
- [Seed de dados](#seed-de-dados)
- [Testes unitários](#testes-unitários)
- [Cobertura de testes](#cobertura-de-testes)
- [Comandos úteis](#comandos-úteis)

## Tecnologias

### Stack principal

| Tecnologia | Versão | Finalidade |
|---|---|---|
| .NET | 9.0 | Plataforma de execução |
| ASP.NET Core | 9.0 | Web API |
| MongoDB | 7 | Banco de dados NoSQL |
| Redis | 7 (alpine) | Cache distribuído |
| Amazon SQS (LocalStack) | 3 | Mensageria assíncrona |
| Serilog | 9.0 | Logging estruturado (JSON) |
| FluentValidation | 12.1 | Validação de entrada |
| Swashbuckle | — | Swagger/OpenAPI |
| Docker / Docker Compose | — | Containerização e orquestração |

### Testes

| Tecnologia | Versão | Finalidade |
|---|---|---|
| NUnit | 4.2 | Framework de testes unitários |
| Moq | 4.20 | Mocks e stubs de dependências |
| FluentAssertions | 8.4 | Asserções expressivas |
| Coverlet | 6.0 | Coleta de cobertura de código |

## Arquitetura

Projeto organizado em camadas seguindo princípios de Clean Architecture:

```
cinemark-challenge/
├── MovieCatalog.Api            → Controllers, middlewares, Program.cs (composition root)
├── MovieCatalog.Application    → Services (use cases), validators, mappers
├── MovieCatalog.Domain         → Entidades, DTOs, enums, eventos, interfaces de use case
├── MovieCatalog.Infra          → Repositórios (MongoDB), cache (Redis), mensageria (SQS)
├── MovieCatalog.Shared         → Utilitários compartilhados (Result<T>, PaginatedResult<T>)
├── MovieCatalog.Test           → Testes unitários da API
├── Notification.Worker         → Worker que consome eventos da fila SQS
├── Notification.Test           → Testes unitários do Worker
└── docker-compose.yml          → Orquestração de todos os serviços
```

### Fluxo de dados

```
Cliente → API (CRUD de filmes) → MongoDB (persistência)
                                → Redis (cache)
                                → SQS (evento publicado)
                                       ↓
                              Notification.Worker (consome e loga o evento)
```

### Padrões utilizados

- **Use Case Pattern** — cada ação de negócio é uma classe/interface (`IFilmCreate`, `IFilmRead`, etc.)
- **Repository Pattern** — abstração de acesso a dados (`IFilmRepository`)
- **Result Pattern** — `Result<T>` para fluxo de sucesso/erro sem exceções desnecessárias
- **Soft Delete** — filmes não são removidos fisicamente, apenas marcados como deletados
- **Middleware de exceções** — tratamento centralizado de erros com correlation ID

## Pré-requisitos

- [Docker](https://www.docker.com/get-started/) (versão 20+) e Docker Compose v2
- [.NET SDK 9.0](https://dotnet.microsoft.com/download/dotnet/9.0) (para rodar localmente ou executar testes)

## Executando o projeto

Na raiz do projeto:

```bash
docker compose up -d --build
```

O comando irá subir:

| Serviço | Container | Porta |
|---|---|---|
| MongoDB | `cinemark-mongodb` | `27017` |
| Redis | `cinemark-redis` | `6379` |
| LocalStack (SQS) | `cinemark-localstack` | `4566` |
| Movie Catalog API | `cinemark-movie-catalog-api` | `5000` |
| Notification Worker | `cinemark-notification-worker` | `5001` |

### URLs disponíveis

| Recurso | URL |
|---|---|
| **Movie Catalog Api** | http://localhost:5000/swagger |
| **Worker Health Check** | http://localhost:5001/health |

Para verificar se os containers estão rodando:

```bash
docker ps
```

## Endpoints da API

Base URL: `http://localhost:5000/api/v1/films`

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/v1/films` | Criar um filme |
| `GET` | `/api/v1/films` | Listar filmes (paginado, com filtros) |
| `GET` | `/api/v1/films/{id}` | Buscar filme por ID |
| `PUT` | `/api/v1/films/{id}` | Atualizar um filme |
| `DELETE` | `/api/v1/films/{id}` | Soft delete de um filme |

### Filtros disponíveis (GET /api/v1/films)

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `genre` | enum | Filtrar por gênero (`Action`, `Comedy`, `Drama`, `SciFi`, `Horror`, `Animation`) |
| `active` | bool | Filtrar por status ativo/inativo |
| `page` | int | Número da página (padrão: 1) |
| `pageSize` | int | Itens por página (padrão: 10) |

### Exemplo de corpo (POST/PUT)

```json
{
  "title": "The Dark Knight",
  "synopsis": "When the menace known as the Joker wreaks havoc...",
  "genre": "Action",
  "releaseDate": "2008-07-18",
  "durationMinutes": 152,
  "rating": 9.0,
  "active": true
}
```

## Tratamento de erros

Todas as respostas de erro incluem um **Correlation ID** para rastreamento, retornado tanto no header `X-Correlation-Id` quanto no corpo da resposta.

### 400 — Validação de entrada

```json
{
  "status": 400,
  "title": "Validation Error",
  "errors": [
    { "propertyName": "Title", "errorMessage": "Título é obrigatório" },
    { "propertyName": "DurationMinutes", "errorMessage": "Duração deve ser maior que 0" }
  ],
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

Mensagens de validação disponíveis:

| Campo | Regra | Mensagem |
|---|---|---|
| Title | Obrigatório | `Título é obrigatório` |
| Title | Máx. 200 caracteres | `Título deve ter no máximo 200 caracteres` |
| Title | Duplicado | `Já existe um filme com este título` |
| Genre | Valor válido do enum | `Gênero inválido` |
| ReleaseDate | Obrigatório | `Data de lançamento é obrigatória` |
| DurationMinutes | Maior que 0 | `Duração deve ser maior que 0` |
| Rating | Entre 0 e 10 | `Avaliação deve estar entre 0 e 10` |

### 404 — Recurso não encontrado

```json
{
  "status": 404,
  "title": "Not Found",
  "detail": "Filme com ID 'abc123' não encontrado",
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

### 500 — Erro interno

```json
{
  "status": 500,
  "title": "Internal Server Error",
  "detail": "An unexpected error occurred. Please try again later.",
  "correlationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

## Seed de dados

Na primeira execução, a API insere automaticamente **10 filmes** no MongoDB caso a coleção `films` esteja vazia:

| Filme | Gênero | Ano | Duração | Rating |
|---|---|---|---|---|
| The Shawshank Redemption | Drama | 1994 | 142 min | 9.3 |
| The Dark Knight | Action | 2008 | 152 min | 9.0 |
| Interstellar | SciFi | 2014 | 169 min | 8.7 |
| The Hangover | Comedy | 2009 | 100 min | 7.7 |
| The Conjuring | Horror | 2013 | 112 min | 7.5 |
| Spider-Man: Into the Spider-Verse | Animation | 2018 | 117 min | 8.4 |
| Gladiator | Action | 2000 | 155 min | 8.5 |
| Blade Runner 2049 | SciFi | 2017 | 164 min | 8.0 |
| Forrest Gump | Drama | 1994 | 142 min | 8.8 |
| Toy Story | Animation | 1995 | 81 min | 8.3 |

## Testes unitários

### Executando todos os testes

```bash
dotnet test cinemark-challenge.sln
```

Para output detalhado:

```bash
dotnet test cinemark-challenge.sln --verbosity normal
```

Para rodar um projeto de teste específico:

```bash
dotnet test MovieCatalog.Test/MovieCatalog.Test.csproj
```

Para rodar uma classe específica:

```bash
dotnet test --filter "FullyQualifiedName~FilmCreateValidatorTests"
```

### Resultado: 49 testes, todos aprovados

#### MovieCatalog.Test (49 testes)

| Categoria | Classe | Testes |
|---|---|---|
| Validators | `FilmCreateValidatorTests` | 11 |
| Validators | `FilmUpdateValidatorTests` | 7 |
| Services | `FilmCreateServiceTests` | 5 |
| Services | `FilmReadServiceTests` | 5 |
| Services | `FilmUpdateServiceTests` | 6 |
| Services | `FilmDeleteServiceTests` | 4 |
| Domain | `FilmEntityTests` | 5 |
| Mappers | `FilmMapperExtensionsTests` | 6 |

## Cobertura de testes

### Gerando o relatório de cobertura

```bash
dotnet test cinemark-challenge.sln --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

```bash
reportgenerator "-reports:TestResults/*/coverage.cobertura.xml" "-targetdir:TestResults/CoverageReport" "-reporttypes:Html;TextSummary"
```

> Requer a ferramenta global: `dotnet tool install -g dotnet-reportgenerator-globaltool`

O relatório HTML será gerado em `TestResults/CoverageReport/index.html`.

### Resumo da cobertura

| Camada | Cobertura | Detalhe |
|---|---|---|
| **MovieCatalog.Application** | **94.9%** | Services 100%, Validators 100%, Mapper 100% |
| **MovieCatalog.Domain** | **90.7%** | Entidades 100%, DTOs 100%, Events ~67% |
| **Notification.Worker** | **68.2%** | SqsConsumerWorker 94.4%, HealthCheck 100% |
| MovieCatalog.Infra | 0% | Infraestrutura (requer integração com MongoDB/Redis) |
| MovieCatalog.Api | 0% | Controllers/Middlewares (requer testes de integração) |

## Comandos úteis

### Docker

```bash
# Subir tudo
docker compose up -d --build

# Parar tudo (preserva dados)
docker compose down

# Parar tudo e apagar volumes (zera bancos)
docker compose down -v

# Ver logs da API em tempo real
docker compose logs -f movie-catalog-api

# Ver logs do Worker em tempo real
docker compose logs -f notification-worker

# Rebuild e restart de um serviço específico
docker compose up -d --build movie-catalog-api
```

### Acessando os serviços externos

#### MongoDB (via MongoDB Compass ou mongosh)

| Campo | Valor |
|---|---|
| Connection String | `mongodb://admin:admin123@localhost:27017` |
| Database | `cinemark` |
| Collection | `films` |

#### Redis (via RedisInsight)

| Campo | Valor |
|---|---|
| Host | `localhost` |
| Porta | `6379` |

As chaves de cache seguem o padrão `cinemark:films:*`.
