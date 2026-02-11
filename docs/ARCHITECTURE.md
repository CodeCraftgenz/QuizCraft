# QuizCraft - Documentacao Tecnica Completa

> Guia tecnico para manutencao e desenvolvimento do QuizCraft.
> Atualizado em: Fevereiro 2026.

---

## Indice

1. [Visao Geral](#1-visao-geral)
2. [Stack Tecnologica](#2-stack-tecnologica)
3. [Estrutura do Projeto](#3-estrutura-do-projeto)
4. [Arquitetura em Camadas](#4-arquitetura-em-camadas)
5. [Domain Layer](#5-domain-layer)
6. [Infrastructure Layer](#6-infrastructure-layer)
7. [Application Layer](#7-application-layer)
8. [Presentation Layer](#8-presentation-layer)
9. [Fluxo de Inicializacao do App](#9-fluxo-de-inicializacao-do-app)
10. [Sistema de Navegacao](#10-sistema-de-navegacao)
11. [Injecao de Dependencias](#11-injecao-de-dependencias)
12. [Sistema de Licenciamento](#12-sistema-de-licenciamento)
13. [Algoritmo de Repeticao Espacada](#13-algoritmo-de-repeticao-espacada)
14. [Ciclo de Vida de um Quiz](#14-ciclo-de-vida-de-um-quiz)
15. [Sistema de Backup](#15-sistema-de-backup)
16. [Import/Export](#16-importexport)
17. [Armazenamento Local](#17-armazenamento-local)
18. [Converters e UI](#18-converters-e-ui)
19. [Testes](#19-testes)
20. [Instalador](#20-instalador)
21. [Guia Rapido: Como Fazer X](#21-guia-rapido-como-fazer-x)

---

## 1. Visao Geral

O **QuizCraft** e um app desktop Windows para estudantes criarem bancos de questoes, estudarem com repeticao espacada e acompanharem desempenho via dashboards.

- **Plataforma:** Windows 10/11 (x64)
- **Runtime:** .NET 9 (self-contained, nao requer .NET instalado)
- **Banco:** SQLite local (100% offline)
- **Licenciamento:** Ativacao online vinculada ao hardware, com grace period de 7 dias offline
- **Publicacao:** Instalador Inno Setup (~50 MB)

---

## 2. Stack Tecnologica

| Camada | Tecnologia | Versao |
|---|---|---|
| Runtime | .NET | 9.0 (net9.0-windows) |
| UI Framework | WPF + [WPF-UI](https://github.com/lepoco/wpfui) | 3.x (Fluent Design) |
| MVVM | [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) | 8.x |
| ORM | Entity Framework Core | 9.x |
| Banco de Dados | SQLite | via EF Core |
| Logging | Serilog | 4.x |
| Criptografia | DPAPI + SHA-256 | System.Security.Cryptography |
| Hardware ID | WMI | System.Management |
| Testes | xUnit + NSubstitute | - |
| Instalador | Inno Setup | 6.x |

---

## 3. Estrutura do Projeto

```
QuizCraft/
├── src/
│   ├── QuizCraft.Domain/            # Entidades, enums, interfaces
│   │   ├── Entities/                # Subject, Topic, Question, Choice, Tag, etc.
│   │   ├── Enums/                   # QuizMode, QuestionType, SessionStatus
│   │   ├── Interfaces/              # IRepository<T>, IQuestionRepository, etc.
│   │   └── Models/                  # LicenseModels (LicenseRecord, LicenseState)
│   │
│   ├── QuizCraft.Application/       # Servicos de logica de negocio
│   │   └── Services/                # QuizService, SpacedRepetition, Statistics, etc.
│   │
│   ├── QuizCraft.Infrastructure/    # Implementacoes, banco, APIs externas
│   │   ├── Data/                    # DbContext, DatabaseInitializer, Seeder
│   │   ├── Repositories/            # Repository<T>, QuestionRepo, SessionRepo
│   │   └── Services/                # BackupService, Licensing (5 arquivos)
│   │
│   └── QuizCraft.Presentation/      # WPF - Views, ViewModels, Converters
│       ├── Assets/                  # logoQuizCraft.png, quizcraft.ico
│       ├── Converters/              # 15 value converters
│       ├── ViewModels/              # 10 ViewModels (Base + 9 features)
│       ├── Views/                   # 10 UserControls (paginas)
│       ├── App.xaml(.cs)            # Startup, DI, temas
│       ├── MainWindow.xaml(.cs)     # Shell principal + navegacao
│       └── LicenseWindow.xaml(.cs)  # Janela de ativacao
│
├── tests/
│   └── QuizCraft.Tests/            # Testes unitarios (xUnit)
│
├── docs/                           # Documentacao
├── installer/                      # Script Inno Setup + icone
├── tools/                          # Gerador de icone
└── QuizCraft.sln
```

### Dependencias entre projetos

```
Presentation ──> Application ──> Domain
     │                │
     └──> Infrastructure ──> Domain
     │         │
     └──> Domain
```

> **Regra:** Camadas superiores dependem das inferiores. Domain nao depende de ninguem.

---

## 4. Arquitetura em Camadas

```
┌─────────────────────────────────────────────────────────────┐
│  PRESENTATION (WPF)                                         │
│  Views (XAML) <──Binding──> ViewModels (MVVM Toolkit)       │
│  Converters, MainWindow (navegacao), LicenseWindow          │
├─────────────────────────────────────────────────────────────┤
│  APPLICATION                                                │
│  QuizService, SpacedRepetitionService, StatisticsService    │
│  ImportExportService, BackupSchedulerService                │
├─────────────────────────────────────────────────────────────┤
│  INFRASTRUCTURE                                             │
│  QuizCraftDbContext (EF Core + SQLite)                      │
│  Repository<T>, QuestionRepository, QuizSessionRepository   │
│  BackupService, LicensingService, CryptoHelper              │
├─────────────────────────────────────────────────────────────┤
│  DOMAIN                                                     │
│  Entities, Enums, Interfaces, Models                        │
│  (nenhuma dependencia externa)                              │
└─────────────────────────────────────────────────────────────┘
```

---

## 5. Domain Layer

### 5.1 Entidades

| Entidade | Descricao | Arquivo |
|---|---|---|
| **Subject** | Materia (Matematica, Portugues...) | `Entities/Subject.cs` |
| **Topic** | Topico dentro da materia (hierarquico via ParentTopicId) | `Entities/Topic.cs` |
| **Question** | Questao com enunciado, tipo, dificuldade, explicacao | `Entities/Question.cs` |
| **Choice** | Alternativa de resposta (ligada a Question) | `Entities/Choice.cs` |
| **Tag** | Etiqueta (ENEM, Vestibular...) | `Entities/Tag.cs` |
| **QuestionTag** | Tabela N:M entre Question e Tag | `Entities/QuestionTag.cs` |
| **QuizSession** | Sessao de quiz (modo, status, duracao, acertos) | `Entities/QuizSession.cs` |
| **QuizSessionItem** | Resposta individual dentro da sessao | `Entities/QuizSessionItem.cs` |
| **Mastery** | Nivel de dominio por questao (repeticao espacada) | `Entities/Mastery.cs` |
| **StudySet** | Preset de filtros salvos | `Entities/StudySet.cs` |
| **StudyStreak** | Registro diario de atividade | `Entities/StudyStreak.cs` |
| **AppSettings** | Configuracoes chave-valor | `Entities/AppSettings.cs` |

### 5.2 Diagrama de Relacionamentos

```
Subject ──1:N──> Topic ──1:N──> Question ──1:N──> Choice
                   │                │
                   │ (self-ref)     ├──1:1──> Mastery
                   └── ParentTopic  │
                                    └──N:M──> Tag (via QuestionTag)

QuizSession ──1:N──> QuizSessionItem ──N:1──> Question

StudyStreak (standalone - registro por dia)
AppSettings (standalone - chave/valor)
StudySet    (standalone - filtros salvos em JSON)
```

### 5.3 Enums

```csharp
// QuizMode - Modos de quiz
Training = 0        // Feedback imediato apos cada questao
Exam = 1            // Resultado so no final
ErrorReview = 2     // Revisar apenas questoes erradas
SpacedReview = 3    // Questoes pela repeticao espacada

// QuestionType - Tipos de questao
MultipleChoice = 0      // Multipla escolha (1 correta)
TrueFalse = 1           // Verdadeiro/Falso
ShortAnswer = 2         // Resposta digitada
MultipleSelection = 3   // Multipla selecao (varias corretas)

// SessionStatus - Estado da sessao
InProgress = 0    // Em andamento
Completed = 1     // Finalizada
Abandoned = 2     // Abandonada
Paused = 3        // Pausada

// LicenseState - Estado da licenca
NotFound = 0      // Nenhuma licenca local
Valid = 1         // Licenca valida
Invalid = 2       // Licenca invalida
Error = 3         // Erro de validacao
```

### 5.4 Interfaces

| Interface | Metodos Principais | Implementacao |
|---|---|---|
| `IRepository<T>` | GetByIdAsync, GetAllAsync, FindAsync, AddAsync, UpdateAsync, DeleteAsync, CountAsync | `Repository<T>` |
| `IQuestionRepository` | SearchAsync (com filtros e paginacao), GetForQuizAsync, GetDueForReviewAsync, GetWithDetailsAsync | `QuestionRepository` |
| `IQuizSessionRepository` | GetWithItemsAsync, GetRecentSessionsAsync, GetSessionsByDateRangeAsync | `QuizSessionRepository` |
| `IBackupService` | CreateBackupAsync, RestoreBackupAsync, GetBackupsAsync, DeleteOldBackupsAsync | `BackupService` |
| `IStatisticsService` | GetDashboardStatsAsync, GetWeakestTopicsAsync, GetDailyPerformanceAsync, RecordStudyDayAsync | `StatisticsService` |
| `ISpacedRepetitionService` | UpdateMasteryAsync, GetReviewQueueAsync, GetDueCountAsync, CalculateNextReview | `SpacedRepetitionService` |
| `ILicensingService` | CheckLicenseAsync, ActivateAsync, RemoveLicense, GetStoredLicense | `LicensingService` |

---

## 6. Infrastructure Layer

### 6.1 QuizCraftDbContext

**Arquivo:** `Data/QuizCraftDbContext.cs`

11 DbSets. Configuracoes principais no `OnModelCreating`:
- Subject: MaxLength(200), Index em Name
- Topic: FK→Subject (Cascade), Self-reference→ParentTopic (Restrict)
- Question: Type convertido para int, FK→Topic (Cascade), Indexes em TopicId, Difficulty, Statement
- Tag: MaxLength(100), Unique Index em Name
- QuestionTag: Composite PK (QuestionId, TagId)
- Mastery: 1:1 com Question, Unique Index em QuestionId, Index em NextReviewAt
- QuizSession: Mode e Status convertidos para int, Index em StartedAt
- StudyStreak: Unique Index em Date
- AppSettings: Unique Index em Key

### 6.2 DatabaseInitializer

**Arquivo:** `Data/DatabaseInitializer.cs`

Metodos estaticos que retornam caminhos em `%AppData%\QuizCraft\`:

| Metodo | Retorno |
|---|---|
| `GetDatabasePath()` | `%AppData%\QuizCraft\quizcraft.db` |
| `GetLogsPath()` | `%AppData%\QuizCraft\logs\` |
| `GetBackupsPath()` | `%AppData%\QuizCraft\backups\` |
| `GetAttachmentsPath()` | `%AppData%\QuizCraft\attachments\` |

> Todos criam o diretorio se nao existir.

### 6.3 DatabaseSeeder

**Arquivo:** `Data/DatabaseSeeder.cs`

Popula o banco com dados de exemplo (so se estiver vazio):
- **5 Materias:** Matematica, Portugues, Historia, Ciencias, Geografia
- **15 Topicos:** distribuidos entre as materias
- **5 Tags:** ENEM, Vestibular, Basico, Avancado, Revisao
- **41 Questoes:** varios tipos e dificuldades

### 6.4 Repositorios

| Classe | Herda de | Metodos Especiais |
|---|---|---|
| `Repository<T>` | - | CRUD generico (7 metodos) |
| `QuestionRepository` | Repository\<Question\> | SearchAsync (filtros + paginacao), GetForQuizAsync (monta quiz), GetDueForReviewAsync (repeticao espacada) |
| `QuizSessionRepository` | Repository\<QuizSession\> | GetWithItemsAsync (carrega tudo), GetRecentSessionsAsync (ultimas 20) |

### 6.5 BackupService

**Arquivo:** `Services/BackupService.cs`

- **CreateBackupAsync():** Fecha conexao → copia .db/.db-wal/.db-shm + attachments → compacta em ZIP → reabre conexao
- **RestoreBackupAsync():** Cria backup de seguranca → fecha conexao → extrai ZIP → reabre conexao
- **DeleteOldBackupsAsync(count):** Mantem apenas os N mais recentes

### 6.6 Sistema de Licenciamento (5 arquivos)

Ver [Secao 12](#12-sistema-de-licenciamento) para detalhes completos.

---

## 7. Application Layer

### 7.1 QuizService (Orquestrador Principal)

**Arquivo:** `Services/QuizService.cs`

O QuizService e o servico central que orquestra todo o fluxo de quiz.

| Metodo | O que faz |
|---|---|
| `BuildQuizQuestionsAsync(...)` | Monta lista de questoes com base no modo e filtros |
| `StartSessionAsync(...)` | Cria nova QuizSession no banco (status InProgress) |
| `RecordAnswerAsync(...)` | Registra resposta + atualiza Mastery via SpacedRepetitionService |
| `FinishSessionAsync(...)` | Finaliza sessao (status Completed), calcula acertos, registra streak |

**Logica de selecao por modo:**
- **Training/Exam:** Busca questoes com filtros normais (materia, topico, tags, dificuldade)
- **ErrorReview:** Questoes com Mastery.Level < 3 (nao dominadas)
- **SpacedReview:** Questoes com NextReviewAt <= agora

### 7.2 SpacedRepetitionService

**Arquivo:** `Services/SpacedRepetitionService.cs`

Ver [Secao 13](#13-algoritmo-de-repeticao-espacada) para detalhes do algoritmo.

### 7.3 StatisticsService

**Arquivo:** `Services/StatisticsService.cs`

| Metodo | Retorno |
|---|---|
| `GetDashboardStatsAsync()` | DashboardStats (8 metricas: total questoes, estudadas, acuracia 7/30 dias, streak, tempo medio, sessoes, pendentes de revisao) |
| `GetWeakestTopicsAsync(5)` | Top 5 topicos com menor acuracia (minimo 3 tentativas) |
| `GetDailyPerformanceAsync(30)` | Acuracia e questoes por dia nos ultimos 30 dias |
| `GetPerformanceByTopicAsync()` | Performance agrupada por topico |
| `RecordStudyDayAsync(...)` | Cria/acumula registro diario no StudyStreak |

**Calculo de streak:** Conta dias consecutivos a partir de hoje, tolerando gap de 1 dia (se hoje ainda nao estudou).

### 7.4 ImportExportService

**Arquivo:** `Services/ImportExportService.cs`

| Metodo | Formato | Descricao |
|---|---|---|
| `ExportQuestionsJsonAsync()` | JSON | Exporta questoes com materia, topico, tags, alternativas |
| `ImportQuestionsJsonAsync()` | JSON | Importa questoes, cria materias/topicos/tags automaticamente |
| `ExportQuestionsCsvAsync()` | CSV (;) | Tabular com ate 5 alternativas por questao |
| `ExportStatsCsvAsync()` | CSV (;) | Historico de sessoes com acuracia |

### 7.5 BackupSchedulerService

**Arquivo:** `Services/BackupSchedulerService.cs`

- Timer periodico (padrao: 15 dias)
- Ao disparar: `IBackupService.CreateBackupAsync()` + `DeleteOldBackupsAsync(retentionCount)`
- Configuravel via `Configure(intervalDays, retentionCount)`

---

## 8. Presentation Layer

### 8.1 Paginas (Views)

| Pagina | Arquivo | ViewModel | Funcao |
|---|---|---|---|
| Dashboard | `DashboardView.xaml` | DashboardViewModel | Estatisticas, graficos de acuracia, topicos fracos |
| Biblioteca | `SubjectsView.xaml` | SubjectsViewModel | CRUD de materias e topicos |
| Questoes | `QuestionsView.xaml` | QuestionsViewModel | Busca, filtros, CRUD de questoes com editor |
| Criar Quiz | `CreateQuizView.xaml` | CreateQuizViewModel | Configurar e iniciar quiz |
| Executar Quiz | `ExecuteQuizView.xaml` | ExecuteQuizViewModel | Responder questoes, timer, progresso |
| Resultados | `ResultsView.xaml` | ResultsViewModel | Resultado da sessao, por topico, por questao |
| Revisao | `ReviewView.xaml` | ReviewViewModel | Fila de revisao espacada |
| Historico | `HistoryView.xaml` | HistoryViewModel | Sessoes passadas |
| Configuracoes | `SettingsView.xaml` | SettingsViewModel | Tema, backup, export |
| Ajuda | `HelpView.xaml` | HelpViewModel | Guia de uso em portugues |

### 8.2 Janelas

| Janela | Funcao |
|---|---|
| `MainWindow.xaml` | Shell principal: sidebar (220px) + area de conteudo. Gerencia navegacao entre paginas |
| `LicenseWindow.xaml` | Dialog de ativacao de licenca (email + botao ativar) |

### 8.3 ViewModels

Todos herdam de `BaseViewModel` (exceto LicenseViewModel) que fornece:
- `IsLoading` (bool) - estado de carregamento
- `ErrorMessage` (string?) - mensagem de erro
- `ExecuteWithLoadingAsync()` - wrapper para operacoes async com loading
- `InitializeAsync()` - metodo virtual para inicializacao

**Padrao MVVM usado:**
- `[ObservableProperty]` para propriedades reativas (gera OnPropertyChanged automaticamente)
- `[RelayCommand]` para comandos (gera ICommand automaticamente)
- `ObservableCollection<T>` para listas bindaveis

### 8.4 Converters (15 total)

| Converter | Entrada → Saida |
|---|---|
| BoolToVisibilityConverter | bool → Visible/Collapsed (param "Invert") |
| InverseBoolConverter | bool → !bool |
| PercentageToColorConverter | double → Verde(>80)/Amarelo(>60)/Laranja(>40)/Vermelho |
| BoolToCorrectTextConverter | bool → "Correto"/"Incorreto" |
| BoolToCorrectColorConverter | bool → Verde/Vermelho |
| MasteryLevelToTextConverter | int(0-5) → Novo/Iniciante/Aprendiz/Intermediario/Avancado/Dominado |
| NullToVisibilityConverter | null → Collapsed (param "Invert") |
| DoubleToPercentageConverter | double → "X.X%" |
| PercentageToHeightConverter | double → altura em pixels (max 160px) |
| PercentageToBarWidthConverter | double → largura em pixels (max 200px) |
| EqualityConverter | MultiBinding → bool (comparacao) |
| QuizModeToTextConverter | QuizMode → Treino/Prova/Revisao de Erros/Revisao Espacada |
| QuestionTypeToTextConverter | QuestionType → Multipla Escolha/V ou F/Resposta Curta/Multipla Selecao |
| NullOrEmptyToVisibilityConverter | string → Collapsed se vazio |

---

## 9. Fluxo de Inicializacao do App

**Arquivo:** `App.xaml.cs` → `OnStartup()`

```
1. Configura Serilog (log em arquivo com rotacao diaria, 14 dias)
        │
2. Configura DI (ServiceCollection)
        │
3. Inicializa banco SQLite
   ├── context.Database.EnsureCreated()
   └── DatabaseSeeder.SeedAsync() (apenas se banco vazio)
        │
4. Verifica licenca (CheckLicenseOnStartup)
   ├── CheckLicenseAsync() → LicenseState
   ├── Se Valid → continua
   └── Se nao → mostra LicenseWindow (dialog modal)
       ├── Usuario ativa → continua
       └── Usuario fecha sem ativar → Shutdown(0)
        │
5. Aplica tema salvo (le AppSettings["Theme"])
   └── Se "Dark" → SettingsViewModel.ApplyTheme(true)
        │
6. Inicia BackupSchedulerService.Start()
        │
7. Cria e exibe MainWindow
   └── mainWindow.Closed += Shutdown()
```

**Shutdown:** `OnExit()` → Dispose services → Log.CloseAndFlush()

**ShutdownMode:** `OnExplicitShutdown` (necessario para nao fechar ao fechar LicenseWindow)

---

## 10. Sistema de Navegacao

**Arquivo:** `MainWindow.xaml.cs`

A navegacao usa **ListBox** no sidebar com tags de string. Ao selecionar um item:

```
ListBox_SelectionChanged
    └── NavigateToAsync(tag)
            ├── Cria ViewModel (via DI ou new)
            ├── Chama InitializeAsync() no ViewModel
            ├── Cria View com DataContext = ViewModel
            └── ContentArea.Content = view
```

### Rotas

| Tag (sidebar) | ViewModel | View |
|---|---|---|
| "Dashboard" | DashboardViewModel | DashboardView |
| "Subjects" | SubjectsViewModel | SubjectsView |
| "Questions" | QuestionsViewModel | QuestionsView |
| "CreateQuiz" | CreateQuizViewModel | CreateQuizView |
| "Review" | ReviewViewModel | ReviewView |
| "History" | HistoryViewModel | HistoryView |
| "Settings" | SettingsViewModel | SettingsView |
| "Help" | HelpViewModel | HelpView |

### Navegacao especial (via callbacks)

Algumas paginas nao sao acessiveis pelo sidebar, mas sim por callbacks:

```
CreateQuizView ──(OnStartQuiz)──> ExecuteQuizView
ExecuteQuizView ──(OnFinishQuiz)──> ResultsView
HistoryView ──(OnViewSession)──> ResultsView
```

**Guarda de navegacao:** Flag `_isNavigating` previne navegacao concorrente.

---

## 11. Injecao de Dependencias

**Arquivo:** `App.xaml.cs` → `ConfigureServices()`

```csharp
// Banco de dados
QuizCraftDbContext           → Transient (SQLite)

// Repositorios
IRepository<T>               → Repository<T>           (Transient)
IQuestionRepository          → QuestionRepository      (Transient)
IQuizSessionRepository       → QuizSessionRepository   (Transient)

// Servicos
IBackupService               → BackupService           (Transient)
IStatisticsService           → StatisticsService       (Transient)
ISpacedRepetitionService     → SpacedRepetitionService (Transient)
QuizService                  → QuizService             (Transient)
ImportExportService          → ImportExportService     (Transient)
BackupSchedulerService       → BackupSchedulerService  (Singleton)
ILicensingService            → LicensingService        (Singleton)

// ViewModels                                          (Transient)
DashboardViewModel, SubjectsViewModel, QuestionsViewModel,
ReviewViewModel, SettingsViewModel, HelpViewModel, LicenseViewModel

// Janela principal                                    (Singleton)
MainWindow
```

**Acesso global:** `App.Services.GetRequiredService<T>()`

---

## 12. Sistema de Licenciamento

### 12.1 Arquitetura

```
┌──────────────────────────────────┐     ┌─────────────────────────────────┐
│  LOCAL (Infrastructure/Services) │     │  SERVIDOR REMOTO                │
├──────────────────────────────────┤     ├─────────────────────────────────┤
│                                  │     │                                 │
│  HardwareHelper                  │     │  API Base URL:                  │
│  └─ GetHardwareId()              │     │  codecraftgenz-monorepo         │
│     ├─ WMI: ProcessorId          │     │    .onrender.com/api            │
│     ├─ WMI: MotherboardSerial    │     │                                 │
│     └─ SHA-256(concat)           │     │  POST /public/license/          │
│                                  │     │       activate-device           │
│  LicenseApiClient                │     │  Body: {app_id, email,          │
│  ├─ AppId = 10                   │     │         hardware_id}            │
│  ├─ ActivateDeviceAsync() ───────┼────>│                                 │
│  └─ VerifyLicenseAsync()  ───────┼────>│  POST /verify-license           │
│                                  │     │  Body: {app_id, email,          │
│  LicensingStorage                │     │         hardware_id}            │
│  ├─ Save() → DPAPI Encrypt      │     │                                 │
│  │     → license.dat             │     └─────────────────────────────────┘
│  ├─ Load() → DPAPI Decrypt      │
│  └─ Delete()                     │
│                                  │
│  CryptoHelper                    │
│  ├─ Protect() [DPAPI]            │
│  ├─ Unprotect() [DPAPI]          │
│  └─ Sha256()                     │
│                                  │
│  LicensingService                │
│  ├─ CheckLicenseAsync()          │
│  ├─ ActivateAsync(email)         │
│  ├─ RemoveLicense()              │
│  └─ GetStoredLicense()           │
└──────────────────────────────────┘
```

### 12.2 Fluxo de Ativacao

```
1. Usuario digita email na LicenseWindow
2. LicenseViewModel.ActivateCommand
3. LicensingService.ActivateAsync(email)
   ├── HardwareHelper.GetHardwareId() → hardwareId
   ├── LicenseApiClient.ActivateDeviceAsync(email, hardwareId)
   │   └── POST /public/license/activate-device
   │       Body: { app_id: 10, email, hardware_id }
   ├── Se sucesso:
   │   ├── Cria LicenseRecord (email, key, hardwareId, timestamps)
   │   ├── LicensingStorage.Save(record) → DPAPI encrypt → license.dat
   │   └── Retorna DeviceActivationResult { Success: true }
   └── Se falha:
       └── Retorna DeviceActivationResult { Success: false, Message: "..." }
```

### 12.3 Fluxo de Verificacao (a cada startup)

```
1. LicensingService.CheckLicenseAsync()
2. LicensingStorage.Load() → LicenseRecord
   ├── Se null → retorna NotFound
   ├── Se hardwareId != local → retorna Invalid
   └── Se ok, continua:
3. LicenseApiClient.VerifyLicenseAsync(email, hardwareId)
   ├── Se online + valid → atualiza LastValidatedAt → retorna Valid
   ├── Se online + invalid → retorna Invalid
   └── Se offline (timeout/erro de rede):
       ├── Se LastValidatedAt < 7 dias atras → retorna Valid (grace period)
       └── Se LastValidatedAt >= 7 dias → retorna Error
```

### 12.4 Seguranca

- **Hardware binding:** Licenca vinculada a CPU + Placa Mae (SHA-256)
- **DPAPI:** Criptografia vinculada ao usuario Windows (impossivel copiar license.dat para outro PC/usuario)
- **Grace period:** 7 dias offline antes de bloquear
- **Arquivo:** `%AppData%\QuizCraft\license.dat`

---

## 13. Algoritmo de Repeticao Espacada

**Arquivo:** `Application/Services/SpacedRepetitionService.cs`

### 13.1 Niveis e Intervalos

| Nivel | Nome | Intervalo |
|---|---|---|
| 0 | Novo | 0 dias (imediato) |
| 1 | Iniciante | 1 dia |
| 2 | Aprendiz | 3 dias |
| 3 | Intermediario | 7 dias |
| 4 | Avancado | 14 dias |
| 5 | Dominado | 30 dias |

### 13.2 Regras de Transicao

```
Acertou:  nivel = min(nivel + 1, 5)
Errou:    nivel = max(nivel - 2, 0)
          intervalo = intervalo * 0.5 (revisa mais cedo)
```

### 13.3 Calculo

```
proxima_revisao = agora + intervalos[novo_nivel]
Se errou: proxima_revisao = agora + (intervalos[novo_nivel] / 2)
```

### 13.4 Fila de Revisao

`GetReviewQueueAsync()` retorna questoes onde `Mastery.NextReviewAt <= DateTime.UtcNow`, ordenadas pela data mais antiga primeiro.

### 13.5 Exemplo

Questao no nivel 3 (intervalo 7 dias):
- **Acerta:** sobe para nivel 4 → proxima revisao em 14 dias
- **Erra:** desce para nivel 1 → proxima revisao em ~0.5 dia

---

## 14. Ciclo de Vida de um Quiz

```
┌─────────────────────────────────────────────────────┐
│  1. CONFIGURACAO (CreateQuizView)                   │
│     - Selecionar materia, topico, tags, dificuldade │
│     - Escolher modo (Treino/Prova/Erros/Espacada)   │
│     - Definir quantidade e opcoes (timer, shuffle)  │
│                                                     │
│  QuizService.BuildQuizQuestionsAsync()              │
│  QuizService.StartSessionAsync()                    │
├─────────────────────────────────────────────────────┤
│  2. EXECUCAO (ExecuteQuizView)                      │
│     - Exibe questao por questao                     │
│     - Timer e progresso visual                      │
│     - Modo Treino: feedback imediato + explicacao   │
│     - Modo Prova: sem feedback ate o final          │
│                                                     │
│  Para cada resposta:                                │
│  QuizService.RecordAnswerAsync()                    │
│    └── SpacedRepetitionService.UpdateMasteryAsync() │
├─────────────────────────────────────────────────────┤
│  3. FINALIZACAO                                     │
│     QuizService.FinishSessionAsync()                │
│       ├── Status = Completed                        │
│       ├── Calcula acertos e duracao                 │
│       └── StatisticsService.RecordStudyDayAsync()   │
├─────────────────────────────────────────────────────┤
│  4. RESULTADOS (ResultsView)                        │
│     - Acuracia geral com cor                        │
│     - Breakdown por topico                          │
│     - Lista questao por questao                     │
│     - Botao "Revisar Erros"                         │
└─────────────────────────────────────────────────────┘
```

---

## 15. Sistema de Backup

### 15.1 Backup Automatico

- Timer via `BackupSchedulerService` (padrao: 15 dias, retencao: 10 backups)
- Configuravel pelo usuario em Configuracoes
- Inicia automaticamente no startup

### 15.2 Formato do Backup

```
QuizCraft_Backup_YYYY-MM-DD_HH-MM-SS.zip
  ├── quizcraft.db          # Banco SQLite
  ├── quizcraft.db-wal      # WAL (se existir)
  ├── quizcraft.db-shm      # Shared memory (se existir)
  └── attachments/           # Imagens anexadas
```

### 15.3 Restauracao

1. Cria backup de seguranca do estado atual (automatico)
2. Fecha conexao com o banco
3. Extrai ZIP sobre os arquivos existentes
4. Reabre conexao

### 15.4 Retencao

Apos cada backup, remove os mais antigos que excedem o limite configurado.

---

## 16. Import/Export

### 16.1 Formato JSON de Questoes

```json
[
  {
    "Subject": "Matematica",
    "Topic": "Algebra",
    "Type": "MultipleChoice",
    "Statement": "Quanto e 2+2?",
    "Explanation": "Soma basica",
    "Difficulty": 1,
    "Source": "Manual de Matematica",
    "Tags": ["ENEM", "Basico"],
    "Choices": [
      { "Text": "3", "IsCorrect": false },
      { "Text": "4", "IsCorrect": true },
      { "Text": "5", "IsCorrect": false }
    ]
  }
]
```

### 16.2 Import JSON

- Cria materias/topicos/tags automaticamente se nao existirem
- Valida dificuldade (clamp 1-5)
- Tipo padrao: MultipleChoice se invalido

### 16.3 CSV Export

- Delimitador: `;`
- Ate 5 alternativas por questao (colunas ChoiceA-E)
- Resposta correta como letra (A-E)

---

## 17. Armazenamento Local

Todos os dados ficam em `%AppData%\QuizCraft\`:

```
%AppData%\QuizCraft\
├── quizcraft.db         # Banco SQLite principal
├── license.dat          # Licenca criptografada (DPAPI)
├── logs/                # Logs do Serilog
│   ├── quizcraft-20260210.log
│   └── quizcraft-20260209.log
├── backups/             # Backups em ZIP
│   └── QuizCraft_Backup_2026-02-10_14-30-00.zip
└── attachments/         # Imagens de questoes
    └── {guid}.png
```

**Politicas:**
- Logs: rotacao diaria, retencao 14 dias
- Backups: configuravel (padrao 10)

---

## 18. Converters e UI

### 18.1 Temas

O app suporta tema **Claro** e **Escuro** via WPF-UI:

```csharp
// SettingsViewModel.cs
public static void ApplyTheme(bool isDark)
{
    ApplicationThemeManager.Apply(isDark ? ApplicationTheme.Dark : ApplicationTheme.Light);
}
```

A preferencia e salva em `AppSettings` com Key="Theme", Value="Dark" ou "Light".

### 18.2 Estilos Globais (App.xaml)

| Estilo | Tipo | Descricao |
|---|---|---|
| CardBorder | Border | Fundo com tema, borda 1px, radius 8, padding 16 |
| StatValue | TextBlock | Font 28, SemiBold, cor primaria |
| StatLabel | TextBlock | Font 12, cor secundaria |
| SectionTitle | TextBlock | Font 18, SemiBold |
| PageTitle | TextBlock | Font 24, Bold |

### 18.3 Graficos do Dashboard

O Dashboard usa barras customizadas (sem biblioteca de graficos):
- `PercentageToHeightConverter` → barras verticais (acuracia diaria)
- `PercentageToBarWidthConverter` → barras horizontais (performance por topico)
- `PercentageToColorConverter` → cores por faixa de acuracia

---

## 19. Testes

**Projeto:** `tests/QuizCraft.Tests/`

- **Framework:** xUnit
- **Mocking:** NSubstitute
- **75 testes** cobrindo:
  - QuizService (fluxo completo)
  - SpacedRepetitionService (algoritmo SM-2)
  - StatisticsService (calculos)
  - ImportExportService (JSON/CSV)
  - Repository queries

**Como rodar:**
```bash
dotnet test
```

---

## 20. Instalador

**Arquivo:** `installer/QuizCraft.iss` (Inno Setup 6)

### 20.1 Build do instalador

```bash
# 1. Publicar self-contained
dotnet publish src/QuizCraft.Presentation -c Release -r win-x64 --self-contained

# 2. Compilar instalador
iscc installer/QuizCraft.iss

# Output: installer/Output/QuizCraft_Setup_v1.0.0.exe (~50 MB)
```

### 20.2 Caracteristicas

- Self-contained (inclui .NET runtime, roda em qualquer Windows 10+ x64)
- Compressao lzma2/ultra64
- Cria diretorios AppData pos-instalacao
- Pergunta se quer remover dados na desinstalacao
- Icone personalizado (Q roxo)
- Suporte PT-BR e EN

---

## 21. Guia Rapido: Como Fazer X

### Adicionar uma nova pagina/tela

1. Criar `Views/MinhaView.xaml` (UserControl)
2. Criar `ViewModels/MinhaViewModel.cs` (herda de BaseViewModel)
3. Registrar ViewModel no DI em `App.xaml.cs` → `ConfigureServices()`
4. Adicionar rota em `MainWindow.xaml.cs` → `NavigateToAsync()`
5. Adicionar item no ListBox do `MainWindow.xaml`

### Adicionar uma nova entidade ao banco

1. Criar classe em `Domain/Entities/NovaEntidade.cs`
2. Adicionar `DbSet<NovaEntidade>` no `QuizCraftDbContext`
3. Configurar no `OnModelCreating()` (indices, FKs, constraints)
4. Se precisar de repositorio especifico:
   - Criar interface em `Domain/Interfaces/`
   - Implementar em `Infrastructure/Repositories/`
   - Registrar no DI

### Adicionar um novo servico

1. Criar interface em `Domain/Interfaces/INovoService.cs`
2. Implementar em `Application/Services/` ou `Infrastructure/Services/`
3. Registrar no DI em `App.xaml.cs`
4. Injetar no ViewModel via construtor

### Alterar o algoritmo de repeticao espacada

Editar `Application/Services/SpacedRepetitionService.cs`:
- Array `_intervals` para mudar intervalos
- `CalculateNewLevel()` para mudar progressao
- `CalculateNextReview()` para mudar calculo da proxima data

### Adicionar novo tipo de questao

1. Adicionar valor no enum `QuestionType` em `Domain/Enums/`
2. Atualizar `QuestionTypeToTextConverter` em `Converters/Converters.cs`
3. Atualizar `ExecuteQuizView.xaml` para renderizar o novo tipo
4. Atualizar `ExecuteQuizViewModel.SubmitAnswerAsync()` para validar resposta
5. Atualizar `ImportExportService` para suportar import/export

### Alterar a API de licenciamento

Editar `Infrastructure/Services/LicenseApiClient.cs`:
- `BaseUrl` para mudar o servidor
- `AppId` para mudar o ID do app
- Metodos `ActivateDeviceAsync`/`VerifyLicenseAsync` para mudar payloads

### Mudar os dados de seed (exemplo)

Editar `Infrastructure/Data/DatabaseSeeder.cs`:
- Adicionar/remover materias, topicos, tags, questoes
- So roda se o banco estiver completamente vazio

### Gerar novo instalador

```bash
dotnet publish src/QuizCraft.Presentation -c Release -r win-x64 --self-contained
iscc installer/QuizCraft.iss
```

### Mudar versao do app

Atualizar em:
1. `installer/QuizCraft.iss` → `#define MyAppVersion`
2. (Opcional) `.csproj` → `<Version>`

---

## Mapa de Arquivos Principais

| O que preciso mexer? | Arquivo |
|---|---|
| Startup / DI / Inicializacao | `Presentation/App.xaml.cs` |
| Navegacao / Menu lateral | `Presentation/MainWindow.xaml(.cs)` |
| Tela de licenca | `Presentation/LicenseWindow.xaml(.cs)` |
| Qualquer pagina | `Presentation/Views/{Pagina}View.xaml` |
| Logica de qualquer pagina | `Presentation/ViewModels/{Pagina}ViewModel.cs` |
| Converters de valor | `Presentation/Converters/Converters.cs` |
| Estilos globais / Tema | `Presentation/App.xaml` |
| Logica de quiz | `Application/Services/QuizService.cs` |
| Repeticao espacada | `Application/Services/SpacedRepetitionService.cs` |
| Estatisticas / Dashboard | `Application/Services/StatisticsService.cs` |
| Import/Export | `Application/Services/ImportExportService.cs` |
| Backup agendado | `Application/Services/BackupSchedulerService.cs` |
| Banco de dados (schema) | `Infrastructure/Data/QuizCraftDbContext.cs` |
| Caminhos de arquivo | `Infrastructure/Data/DatabaseInitializer.cs` |
| Dados iniciais | `Infrastructure/Data/DatabaseSeeder.cs` |
| Queries de questoes | `Infrastructure/Repositories/QuestionRepository.cs` |
| Queries de sessoes | `Infrastructure/Repositories/QuizSessionRepository.cs` |
| Backup (criar/restaurar) | `Infrastructure/Services/BackupService.cs` |
| Licenciamento (orquestrador) | `Infrastructure/Services/LicensingService.cs` |
| API de licenca (HTTP) | `Infrastructure/Services/LicenseApiClient.cs` |
| Criptografia local | `Infrastructure/Services/CryptoHelper.cs` |
| ID do hardware | `Infrastructure/Services/HardwareHelper.cs` |
| Armazenamento da licenca | `Infrastructure/Services/LicensingStorage.cs` |
| Entidades do banco | `Domain/Entities/*.cs` |
| Enums | `Domain/Enums/*.cs` |
| Interfaces de servico | `Domain/Interfaces/*.cs` |
| Modelos de licenca | `Domain/Models/LicenseModels.cs` |
| Instalador | `installer/QuizCraft.iss` |
