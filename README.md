<p align="center">
  <img src="logoQuizCraft.png" alt="QuizCraft Logo" width="400" />
</p>

<h1 align="center">QuizCraft</h1>

<p align="center">
  <strong>Plataforma de Estudos Inteligente</strong><br>
  Crie quizzes personalizados, estude com repeticao espacada e acompanhe seu desempenho.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-9.0-purple?logo=dotnet" alt=".NET 9" />
  <img src="https://img.shields.io/badge/WPF-Fluent%20Design-blue?logo=windows" alt="WPF" />
  <img src="https://img.shields.io/badge/SQLite-Offline-green?logo=sqlite" alt="SQLite" />
  <img src="https://img.shields.io/badge/License-Proprietary-red" alt="License" />
</p>

---

## Sobre

O **QuizCraft** e um aplicativo desktop para Windows voltado para estudantes que desejam criar bancos de questoes, estudar com repeticao espacada e acompanhar seu progresso por meio de dashboards visuais.

Ideal para quem esta se preparando para **ENEM, vestibulares, concursos e certificacoes**.

### Principais recursos

- **Gestao completa de conteudo** - Cadastre materias, topicos e questoes com suporte a multipla escolha, verdadeiro/falso e resposta curta
- **4 modos de quiz** - Treino, Prova, Revisao de Erros e Revisao Espacada (algoritmo SM-2)
- **Dashboard inteligente** - Estatisticas de desempenho, sequencia de estudos e graficos de progresso
- **Sistema de dominio (Mastery)** - Acompanhe seu nivel de aprendizado por questao com repeticao espacada
- **Importacao/Exportacao** - Importe e exporte seus bancos de questoes em JSON
- **Backup automatico** - Agendamento periodico com politica de retencao configuravel
- **Tema claro e escuro** - Interface Fluent Design adaptavel a sua preferencia
- **100% offline** - Todos os dados ficam no seu computador, sem depender de internet

---

## Screenshots

> *Em breve*

---

## Tecnologias

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 9 (net9.0-windows) |
| UI Framework | WPF + [WPF-UI](https://github.com/lepoco/wpfui) 3.x (Fluent Design) |
| Banco de Dados | SQLite via Entity Framework Core 9 |
| Arquitetura | MVVM com [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) 8.x |
| Logging | Serilog com rotacao diaria |
| Testes | xUnit + NSubstitute (75 testes) |
| Instalador | Inno Setup 6 |

---

## Arquitetura

O projeto segue uma arquitetura em camadas com separacao clara de responsabilidades:

```
QuizCraft/
├── src/
│   ├── QuizCraft.Domain/           # Entidades, enums, interfaces
│   ├── QuizCraft.Application/      # Servicos de aplicacao (Quiz, Backup, Estatisticas)
│   ├── QuizCraft.Infrastructure/   # EF Core, repositorios, licenciamento
│   └── QuizCraft.Presentation/     # WPF Views, ViewModels, Converters
├── tests/
│   └── QuizCraft.Tests/            # Testes unitarios (xUnit)
├── docs/                           # Documentacao tecnica
├── installer/                      # Script Inno Setup
└── QuizCraft.sln
```

---

## Pre-requisitos

- **Windows 10/11** (x64)
- [.NET 9 SDK](https://dotnet.microsoft.com/pt-br/download/dotnet/9.0)

---

## Como compilar

```bash
# Clonar o repositorio
git clone https://github.com/CodeCraftgenz/QuizCraft.git
cd QuizCraft

# Restaurar dependencias e compilar
dotnet build

# Rodar os testes
dotnet test

# Executar o app
dotnet run --project src/QuizCraft.Presentation
```

---

## Como gerar o instalador

1. Instale o [Inno Setup 6](https://jrsoftware.org/isinfo.php)
2. Publique o projeto:

```bash
dotnet publish src/QuizCraft.Presentation -c Release -r win-x64 --self-contained
```

3. Compile o instalador:

```bash
iscc installer/QuizCraft.iss
```

O instalador sera gerado em `installer/Output/`.

---

## Estrutura do banco de dados

O QuizCraft utiliza SQLite com as seguintes tabelas principais:

- **Subjects** - Materias (Matematica, Portugues, etc.)
- **Topics** - Topicos hierarquicos por materia
- **Questions** - Questoes com enunciado, tipo e dificuldade
- **Choices** - Alternativas de resposta
- **Tags** - Etiquetas para categorizar questoes
- **QuizSessions** - Sessoes de quiz com modo e status
- **Masteries** - Nivel de dominio por questao (repeticao espacada)
- **StudyStreaks** - Registro diario de atividade
- **AppSettings** - Configuracoes persistentes (tema, preferencias)

---

## Licenciamento

O QuizCraft utiliza um sistema de licenciamento vinculado ao hardware do dispositivo. Para utilizar o app, e necessario ativar com o email cadastrado na compra.

Adquira sua licenca em: [codecraftgenz.com.br](https://codecraftgenz.com.br)

---

## Documentacao

- [Arquitetura](docs/ARCHITECTURE.md) - Documentacao detalhada da arquitetura
- [Build e Instalador](docs/BUILD.md) - Instrucoes de compilacao e geracao do instalador
- [Plano de Testes](docs/TESTPLAN.md) - Estrategia e cenarios de teste
- [Backlog](docs/BACKLOG.md) - Funcionalidades planejadas

---

## Desenvolvido por

**[CodeCraft GenZ](https://codecraftgenz.com.br)**

---
