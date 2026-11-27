# FCG – FIAP Cloud Games

API REST (.NET 8, monolito) para cadastro de usuários e biblioteca de jogos adquiridos – Fase 1 do Tech Challenge.

## Objetivos
- Cadastro de usuários (nome, e-mail, senha segura: mín. 8 caracteres com letras, números e especiais).
- Autenticação JWT com dois perfis: `User` (plataforma/biblioteca) e `Admin` (cadastrar jogos, administrar usuários, criar promoções).
- Persistência com Entity Framework Core (SQLite) e migrations.
- Middleware de tratamento de erros e logs estruturados (JSON), Swagger, testes unitários e de integração.

## Estrutura (`src/`)
- `FCG.Api/Domain` – entidades e regras de negócio (DDD): `User`, `Game`, `Promotion`, `UserGame` (biblioteca), validações.
- `FCG.Api/Infrastructure` – `FcgDbContext` e migrations.
- `FCG.Api/Services` – hash de senha (PBKDF2) e geração de JWT.
- `FCG.Api/Middleware` – tratamento de erros e logs de requisição.
- `FCG.Api/Controllers` – endpoints.
- `FCG.Tests` – testes xUnit (unitários + integração via `WebApplicationFactory`).

## Executando
```bash
cd src
dotnet run --project FCG.Api      # aplica migrations automaticamente e cria o banco fcg.db
```
Swagger em `/swagger`. Um administrador inicial é criado a partir de `Admin:Email` / `Admin:Password`
(padrão de desenvolvimento: `admin@fcg.com` / `Admin@1234`). **Altere `Jwt:Key` e as credenciais em produção**
(ex.: variáveis de ambiente `Jwt__Key`, `Admin__Password`).

## Testes
```bash
cd src && dotnet test
```

## Endpoints principais
| Método | Rota | Acesso |
|---|---|---|
| POST | `/api/auth/register`, `/api/auth/login` | público |
| GET | `/api/games`, `/api/games/{id}` | autenticado |
| POST/PUT/DELETE | `/api/games` | Admin |
| GET/POST/DELETE | `/api/promotions` | Admin |
| GET/PUT/DELETE | `/api/users`, `PUT /api/users/{id}/role` | Admin |
| GET | `/api/library` | autenticado |
| POST | `/api/library/{gameId}` (adquirir, aplica promoção ativa) | autenticado |

## Migrations
```bash
cd src && dotnet ef migrations add <Nome> -p FCG.Api -o Infrastructure/Migrations
```

## Documentação DDD
Event Storming (criação de usuários e de jogos) e diagramas devem ser mantidos em Miro ou equivalente e linkados aqui.
Eventos modelados no código: *UsuárioCadastrado*, *JogoCadastrado*, *PromoçãoCriada*, *JogoAdquirido*.
