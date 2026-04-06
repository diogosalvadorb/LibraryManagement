# LibraryManagement

API REST para gerenciamento de biblioteca, com controle de livros, usuários e empréstimos. Construída em **.NET 9** seguindo os princípios de **Clean Architecture**, com autenticação JWT, validações via FluentValidation e processamento de jobs em background com Hangfire.

---

## Features

O sistema permite que bibliotecas gerenciem seu acervo de livros e o ciclo de vida dos empréstimos, com regras de negócio baseadas na role de cada usuário. As principais funcionalidades são:

- Cadastro e autenticação de usuários com perfis de acesso (Common, Premium, VIP, Admin)
- Gerenciamento completo do catálogo de livros (CRUD)
- Criação e devolução de empréstimos com regras de limite por role
- Marcação automática de empréstimos vencidos via job recorrente (Hangfire)
- Autorização baseada em roles (somente Admin pode criar/atualizar/deletar livros)

---

## Arquitetura

O projeto segue **Clean Architecture**, separando as responsabilidades em camadas concêntricas onde as dependências apontam sempre para o centro (domínio).

```
┌─────────────────────────────────────────┐
│           LibraryManagement.API         │  ← Apresentação
│    Controllers · Program · Hangfire     │
├─────────────────────────────────────────┤
│       LibraryManagement.Application     │  ← Casos de uso
│   Services · DTOs · Validators · Jobs   │
├─────────────────────────────────────────┤
│         LibraryManagement.Domain        │  ← Regras de negócio
│      Entities · Enums · Interfaces      │
├─────────────────────────────────────────┤
│     LibraryManagement.Infrastructure    │  ← Acesso a dados
│     EF Core · Repositories · Context    │
├─────────────────────────────────────────┤
│     LibraryManagement.Tests             │  ← Testes
│     xUnit · NSubstitute · Moq           │
└─────────────────────────────────────────┘

```

### Descrição das Camadas

**Domain** — núcleo da aplicação, sem dependências externas. Define as entidades (`Book`, `Loan`, `User`), os enumeradores (`LoanStatus`, `UserRole`) e as interfaces dos repositórios. As regras de negócio vivem aqui: limites de empréstimo por role, cálculo da data de devolução esperada e as transições de estado de `Loan` e `Book`.

**Application** — orquestra os casos de uso consumindo apenas as interfaces do domínio. Contém os services (`BookService`, `LoanService`, `UserService`, `AuthService`), os DTOs de entrada e saída, os validators (FluentValidation), o mapeamento entre entidades e DTOs, e o job recorrente de detecção de empréstimos vencidos.

**Infrastructure** — implementa as interfaces do domínio. Contém o `DataBaseContext` (EF Core), as configurações de mapeamento de entidades e os repositórios concretos que executam as queries no SQL Server.

**API** — ponto de entrada HTTP. Define os controllers REST, registra todas as dependências no container de DI, configura autenticação JWT, Hangfire e a documentação via Scalar.

**Tests** — testes unitários cobrindo services, validators e o job de atraso, usando xUnit com NSubstitute e Moq como frameworks de mock.

---

## Tecnologias

| Tecnologia | Versão | Uso |
|-----------|--------|-----|
| .NET | 9.0 | Plataforma principal |
| ASP.NET Core | 9.0 | API REST |
| Entity Framework Core | 9.0 | ORM (SQL Server) |
| SQL Server | — | Banco de dados relacional |
| JWT Bearer | 9.0 | Autenticação e autorização |
| FluentValidation | 11.3 | Validação de DTOs |
| Hangfire | 1.8 | Jobs em background / scheduler |
| Scalar | — | Documentação interativa da API |
| xUnit | 2.9 | Framework de testes |

---

## Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (local ou via Docker)

### SQL Server via Docker (opcional)

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Senha@123" \
  -p 1433:1433 --name sqlserver \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

---

## Como Rodar

### 1. Clonar e restaurar dependências

```bash
git clone <url-do-repositorio>
cd LibraryManagement
dotnet restore
```

### 2. Aplicar as migrations

```bash
dotnet ef database update \
  --project LibraryManagement.Infrastructure \
  --startup-project LibraryManagement.API
```

### 3. Executar a API

```bash
dotnet run --project LibraryManagement.API
```

A API estará disponível em:
- HTTP: `http://localhost:5134`
- HTTPS: `https://localhost:7016`
- Documentação (Scalar): `https://localhost:7016/scalar/v1`
- Dashboard Hangfire: `https://localhost:7016/hangfire`

---

## Endpoints

### Usuários

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `GET` | `/api/users` | — | Lista todos os usuários |
| `GET` | `/api/users/{id}` | — | Busca usuário por ID |
| `POST` | `/api/users` | — | Cadastra novo usuário |
| `POST` | `/api/users/login` | — | Realiza login e retorna JWT |

---

### Livros

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `GET` | `/api/books` | — | Lista todos os livros ativos |
| `GET` | `/api/books/{id}` | — | Busca livro por ID |
| `POST` | `/api/books` | Admin | Cadastra novo livro |
| `PUT` | `/api/books/{id}` | Admin | Atualiza livro |
| `DELETE` | `/api/books/{id}` | Admin | Desativa livro (soft delete) |

---

### Empréstimos

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| `GET` | `/api/loans` | Admin | Lista todos os empréstimos |
| `GET` | `/api/loans/{id}` | Autenticado | Busca empréstimo por ID |
| `GET` | `/api/loans/user/{userId}` | Autenticado | Lista empréstimos ativos do usuário |
| `POST` | `/api/loans` | Autenticado | Cria um novo empréstimo |
| `PATCH` | `/api/loans/{id}/return` | Autenticado | Registra a devolução do livro |
| `DELETE` | `/api/loans/{id}` | Admin | Desativa empréstimo |

---

## Autenticação

A API usa **JWT Bearer Token**. Para acessar endpoints protegidos:

1. Faça login em `POST /api/users/login`
2. Copie o token retornado
3. Envie o header `Authorization: Bearer <token>` em todas as requisições protegidas

O token expira em **2 horas**.

---

## Testes

```bash
# Rodar todos os testes
dotnet test

# Com relatório detalhado
dotnet test --verbosity normal

# Com cobertura de código
dotnet test --collect:"XPlat Code Coverage"
```

A suíte cobre:

- `AuthService` — geração e validação de hash SHA-256 e tokens JWT
- `BookService` — CRUD completo com cenários de erro (not found, já inativo, ISBN duplicado)
- `LoanService` — criação, devolução e exclusão com todas as regras de negócio (limites por role, livro indisponível, empréstimo já devolvido)
- `OverdueLoanJob` — marcação automática de empréstimos vencidos com casos de borda (inativos, já devolvidos, mistos)
- `Validators` — validações de campos obrigatórios, formatos e enumerações

---

## Configuração

### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "Connection": "Server=localhost;Database=libarary;User Id=sa;Password=Senha@123;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "<chave-secreta-base64>",
    "Issuer": "LibraryManagement",
    "Audience": "LibraryManagementClient"
  }
}
```

### Job Recorrente

O `OverdueLoanJob` é registrado no Hangfire e executado **a cada 5 minutos**. Ele verifica todos os empréstimos com status `Borrowed` cuja `ExpectedReturnDate` já passou e os marca como `Overdue`.

```csharp
RecurringJob.AddOrUpdate<OverdueLoanJob>(
    recurringJobId: "check-overdue-loans",
    methodCall: job => job.CheckAndMarkOverdueLoansAsync(),
    cronExpression: "*/5 * * * *");
```

O progresso dos jobs pode ser acompanhado em tempo real pelo dashboard do Hangfire em `/hangfire`.
