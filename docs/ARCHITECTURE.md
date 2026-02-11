# Arquitetura do QuizCraft

## 1. Visao Geral do Produto

O **QuizCraft** e um aplicativo desktop para Windows, **100% offline**, desenvolvido para estudantes que desejam criar bancos de questoes, estudar com repeticao espacada e acompanhar seu desempenho por meio de dashboards visuais.

O aplicativo nao requer conexao com a internet em nenhum momento. Todos os dados sao armazenados localmente no computador do usuario, garantindo privacidade total e funcionamento independente de servidores externos.

**Publico-alvo:** Estudantes brasileiros preparando-se para concursos, vestibulares, provas de certificacao e demais avaliacoes.

**Plataforma:** Windows 10/11 (x64).

---

## 2. Stack Tecnologica

| Camada | Tecnologia | Versao | Justificativa |
|---|---|---|---|
| Runtime | .NET | 9 | LTS, desempenho nativo, AOT futuro |
| UI Framework | WPF | - | Framework desktop maduro, XAML poderoso |
| Design System | WPF-UI | 3.x | Fluent Design, visual Windows 11 nativo |
| Banco de Dados | SQLite | - | Zero-config, arquivo unico, offline |
| ORM | Entity Framework Core | 9.x | Migrations, LINQ, produtividade |
| MVVM Toolkit | CommunityToolkit.Mvvm | 8.x | Source generators, menos boilerplate |
| Graficos | LiveCharts2 | 2.x | Graficos interativos, WPF nativo |
| Logging | Serilog | 4.x | Structured logging, sinks flexiveis |
| Testes | xUnit + FluentAssertions | - | Padrao da comunidade .NET |
| Instalador | Inno Setup | 6.x | Leve, customizavel, gratuito |

---

## 3. Arquitetura em Camadas

O projeto segue uma **arquitetura em camadas (Layered Architecture)** com separacao clara de responsabilidades. A comunicacao e sempre de cima para baixo: camadas superiores dependem das inferiores, nunca o contrario.

```
+============================================================+
|                    PRESENTATION LAYER                       |
|                                                             |
|   WPF Views (XAML)          ViewModels                      |
|   +------------------+      +---------------------------+   |
|   | DashboardPage    |<---->| DashboardViewModel        |   |
|   | QuizPage         |<---->| QuizViewModel             |   |
|   | QuestionsPage    |<---->| QuestionsViewModel        |   |
|   | ReviewPage       |<---->| ReviewViewModel           |   |
|   | HistoryPage      |<---->| HistoryViewModel          |   |
|   | SettingsPage     |<---->| SettingsViewModel         |   |
|   | SubjectsPage     |<---->| SubjectsViewModel         |   |
|   | BackupPage       |<---->| BackupViewModel           |   |
|   +------------------+      +---------------------------+   |
|                                                             |
|   Padrao: MVVM com CommunityToolkit.Mvvm                    |
|   Data Binding, Commands, ObservableProperties              |
+============================================================+
                             |
                             | Dependency Injection
                             v
+============================================================+
|                    APPLICATION LAYER                        |
|                                                             |
|   +---------------------+  +----------------------------+  |
|   | QuizService         |  | StatisticsService          |  |
|   | - CreateSession()   |  | - GetDashboardStats()      |  |
|   | - SubmitAnswer()    |  | - GetTopicPerformance()    |  |
|   | - FinishSession()   |  | - GetAccuracyOverTime()    |  |
|   +---------------------+  +----------------------------+  |
|                                                             |
|   +---------------------+  +----------------------------+  |
|   | SpacedRepetition    |  | ImportExportService         |  |
|   |   Service           |  | - ExportToJson()           |  |
|   | - CalculateNext()   |  | - ImportFromJson()         |  |
|   | - GetReviewQueue()  |  | - ExportToCsv()            |  |
|   | - UpdateMastery()   |  | - ImportFromCsv()          |  |
|   +---------------------+  +----------------------------+  |
|                                                             |
|   +---------------------+                                   |
|   | BackupScheduler     |                                   |
|   |   Service           |                                   |
|   | - CheckSchedule()   |                                   |
|   | - RunAutoBackup()   |                                   |
|   +---------------------+                                   |
+============================================================+
                             |
                             | Interfaces (Domain)
                             v
+============================================================+
|                      DOMAIN LAYER                          |
|                                                             |
|   Entities              Enums            Interfaces         |
|   +---------------+     +------------+   +---------------+  |
|   | Subject       |     | Difficulty |   | ISubjectRepo  |  |
|   | Topic         |     | QuestionTy |   | ITopicRepo    |  |
|   | Question      |     | QuizMode   |   | IQuestionRepo |  |
|   | Choice        |     | MasteryLvl |   | IMasteryRepo  |  |
|   | Tag           |     |            |   | IQuizSession  |  |
|   | Mastery       |     |            |   |   Repo        |  |
|   | QuizSession   |     |            |   | ITagRepo      |  |
|   | QuizSession   |     |            |   | IBackupSvc    |  |
|   |   Item        |     |            |   | IAppSettings  |  |
|   | StudyStreak   |     |            |   |   Repo        |  |
|   | AppSettings   |     |            |   +---------------+  |
|   +---------------+     +------------+                      |
+============================================================+
                             |
                             | Implementacoes
                             v
+============================================================+
|                   INFRASTRUCTURE LAYER                      |
|                                                             |
|   +---------------------+  +----------------------------+  |
|   | QuizCraftDbContext   |  | Repositories               |  |
|   | - DbSet<Subject>    |  | - SubjectRepository        |  |
|   | - DbSet<Topic>      |  | - TopicRepository          |  |
|   | - DbSet<Question>   |  | - QuestionRepository       |  |
|   | - DbSet<Choice>     |  | - MasteryRepository        |  |
|   | - DbSet<Tag>        |  | - QuizSessionRepository    |  |
|   | - DbSet<Mastery>    |  | - TagRepository            |  |
|   | - DbSet<QuizSession>|  | - AppSettingsRepository    |  |
|   | - DbSet<StudyStreak>|  +----------------------------+  |
|   | - DbSet<AppSettings>|                                   |
|   +---------------------+  +----------------------------+  |
|                             | BackupService              |  |
|   SQLite Database           | - CreateBackup()           |  |
|   quizcraft.db              | - RestoreBackup()          |  |
|                             | - ApplyRetention()         |  |
|   Serilog File Sink         +----------------------------+  |
|   logs/quizcraft-YYYYMMDD                                   |
|     .log                                                    |
+============================================================+
```

---

## 4. Modelo de Dados

### 4.1 Diagrama Entidade-Relacionamento (Simplificado)

```
Subject (Disciplina)
  |
  +-- 1:N -- Topic (Assunto)
                |
                +-- 1:N -- Question (Questao)
                              |
                              +-- 1:N -- Choice (Alternativa)
                              |
                              +-- N:M -- Tag (via QuestionTag)
                              |
                              +-- 1:1 -- Mastery (Dominio / Repeticao Espacada)

QuizSession (Sessao de Quiz)
  |
  +-- 1:N -- QuizSessionItem (Resposta Individual)
                |
                +-- N:1 -- Question

StudyStreak (Sequencia de Estudo)
  - Registro diario de atividade

AppSettings (Configuracoes do App)
  - Tema, intervalo de backup, preferencias
```

### 4.2 Detalhamento das Entidades

#### Subject (Disciplina)
| Campo | Tipo | Descricao |
|---|---|---|
| Id | int (PK) | Identificador unico |
| Name | string | Nome da disciplina |
| Description | string? | Descricao opcional |
| Color | string? | Cor para identificacao visual |
| CreatedAt | DateTime | Data de criacao |
| UpdatedAt | DateTime | Data da ultima atualizacao |
| IsActive | bool | Soft delete |

#### Topic (Assunto)
| Campo | Tipo | Descricao |
|---|---|---|
| Id | int (PK) | Identificador unico |
| SubjectId | int (FK) | Referencia a disciplina |
| ParentTopicId | int? (FK) | Assunto pai (hierarquia) |
| Name | string | Nome do assunto |
| Description | string? | Descricao opcional |
| CreatedAt | DateTime | Data de criacao |
| IsActive | bool | Soft delete |

> **Nota:** A relacao `ParentTopicId` permite criar uma hierarquia de assuntos (ex.: Matematica > Algebra > Equacoes de 2o grau).

#### Question (Questao)
| Campo | Tipo | Descricao |
|---|---|---|
| Id | int (PK) | Identificador unico |
| TopicId | int (FK) | Referencia ao assunto |
| Text | string | Enunciado da questao |
| Explanation | string? | Explicacao da resposta |
| Difficulty | enum | Facil, Medio, Dificil |
| QuestionType | enum | MultipleChoice, TrueFalse |
| ImagePath | string? | Caminho para imagem anexa |
| CreatedAt | DateTime | Data de criacao |
| UpdatedAt | DateTime | Data da ultima atualizacao |
| IsActive | bool | Soft delete |

#### Choice (Alternativa)
| Campo | Tipo | Descricao |
|---|---|---|
| Id | int (PK) | Identificador unico |
| QuestionId | int (FK) | Referencia a questao |
| Text | string | Texto da alternativa |
| IsCorrect | bool | Se e a resposta correta |
| Order | int | Ordem de exibicao |

#### Tag (Etiqueta)
| Campo | Tipo | Descricao |
|---|---|---|
| Id | int (PK) | Identificador unico |
| Name | string | Nome da tag (unico) |

> Relacao N:M com Question via tabela intermediaria `QuestionTag`.

#### Mastery (Dominio - Repeticao Espacada)
| Campo | Tipo | Descricao |
|---|---|---|
| Id | int (PK) | Identificador unico |
| QuestionId | int (FK, unique) | Referencia a questao (1:1) |
| Level | int | Nivel atual (0-5) |
| NextReviewDate | DateTime | Data da proxima revisao |
| LastReviewedAt | DateTime? | Data da ultima revisao |
| TimesCorrect | int | Total de acertos |
| TimesIncorrect | int | Total de erros |

#### QuizSession (Sessao de Quiz)
| Campo | Tipo | Descricao |
|---|---|---|
| Id | int (PK) | Identificador unico |
| Mode | enum | Training, Exam |
| StartedAt | DateTime | Inicio da sessao |
| FinishedAt | DateTime? | Fim da sessao |
| TotalQuestions | int | Numero total de questoes |
| CorrectAnswers | int | Respostas corretas |
| TimeLimitSeconds | int? | Tempo limite (modo exame) |
| ElapsedSeconds | int | Tempo decorrido |
| IsCompleted | bool | Se a sessao foi finalizada |

#### QuizSessionItem (Item da Sessao)
| Campo | Tipo | Descricao |
|---|---|---|
| Id | int (PK) | Identificador unico |
| QuizSessionId | int (FK) | Referencia a sessao |
| QuestionId | int (FK) | Referencia a questao |
| SelectedChoiceId | int? (FK) | Alternativa selecionada |
| IsCorrect | bool | Se acertou |
| TimeSpentSeconds | int | Tempo gasto na questao |
| AnsweredAt | DateTime? | Quando respondeu |

#### StudyStreak (Sequencia de Estudo)
| Campo | Tipo | Descricao |
|---|---|---|
| Id | int (PK) | Identificador unico |
| Date | DateOnly | Data da atividade |
| QuestionsAnswered | int | Questoes respondidas no dia |
| CorrectAnswers | int | Acertos no dia |
| StudyTimeSeconds | int | Tempo de estudo no dia |

#### AppSettings (Configuracoes)
| Campo | Tipo | Descricao |
|---|---|---|
| Id | int (PK) | Identificador unico |
| Key | string | Chave da configuracao |
| Value | string | Valor da configuracao |

---

## 5. Decisoes de Design

### 5.1 Por que WPF-UI ao inves de MaterialDesignInXAML?

| Criterio | WPF-UI | MaterialDesignInXAML |
|---|---|---|
| Visual | Fluent Design, nativo Windows 11 | Material Design, visual Android/Web |
| Consistencia com SO | Alta - segue padroes do Windows | Baixa - visual de outra plataforma |
| Tema automatico | Segue tema do Windows automaticamente | Configuracao manual |
| Peso / Dependencias | Leve | Mais pesado |
| Manutencao | Ativa, comunidade crescente | Ativa, comunidade madura |

**Decisao:** WPF-UI foi escolhido por oferecer uma experiencia visual moderna e **nativa do Windows 11**, fazendo com que o QuizCraft pareca parte do sistema operacional. Para um app desktop voltado a estudantes brasileiros, essa familiaridade reduz a curva de aprendizado.

### 5.2 Por que SQLite ao inves de SQL Server LocalDB ou LiteDB?

| Criterio | SQLite | SQL Server LocalDB | LiteDB |
|---|---|---|---|
| Instalacao | Zero-config, arquivo unico | Requer instalacao separada | Zero-config |
| Tamanho | ~1 MB | ~50 MB | ~500 KB |
| Offline | Nativo | Nativo | Nativo |
| EF Core Support | Excelente | Excelente | Nao oficial |
| Portabilidade | Copiar arquivo | Complexa | Copiar arquivo |
| Maturidade | Decadas de uso, bilhoes de deploys | Madura | Relativamente nova |

**Decisao:** SQLite e o banco de dados mais implantado do mundo. Para um app offline, arquivo unico, zero-config e suporte completo ao EF Core tornam a escolha natural. A portabilidade do arquivo facilita backups e restauracoes.

### 5.3 Por que CommunityToolkit.Mvvm ao inves de Prism ou ReactiveUI?

| Criterio | CommunityToolkit.Mvvm | Prism | ReactiveUI |
|---|---|---|---|
| Source Generators | Sim - `[ObservableProperty]`, `[RelayCommand]` | Nao | Nao |
| Boilerplate | Minimo | Moderado | Moderado |
| Curva de aprendizado | Baixa | Media | Alta (Rx) |
| Microsoft oficial | Sim (Community Toolkit) | Nao | Nao |
| Peso | Leve | Pesado (modulos) | Medio |

**Decisao:** CommunityToolkit.Mvvm utiliza **source generators do C#** para eliminar boilerplate. Um `[ObservableProperty]` substitui dezenas de linhas de codigo. Sendo parte do toolkit oficial da Microsoft, garante longevidade e compatibilidade.

---

## 6. Armazenamento de Dados

Todos os dados do QuizCraft sao armazenados no diretorio local do usuario:

```
%AppData%\QuizCraft\
  |
  +-- quizcraft.db              # Banco de dados SQLite
  |
  +-- logs\                     # Logs estruturados (Serilog)
  |     +-- quizcraft-20260210.log
  |     +-- quizcraft-20260209.log
  |
  +-- backups\                  # Backups automaticos e manuais
  |     +-- quizcraft-backup-20260210-143000.zip
  |     +-- quizcraft-backup-20260125-080000.zip
  |
  +-- attachments\              # Imagens anexadas as questoes
        +-- {guid}.png
        +-- {guid}.jpg
```

### Politica de Armazenamento

- **Banco de dados:** Arquivo unico SQLite, sem limite de tamanho definido (na pratica, dezenas de milhares de questoes cabem em poucos MB).
- **Logs:** Rotacao diaria, retencao de 30 dias.
- **Backups:** Retencao configuravel (padrao: 10 backups mais recentes).
- **Anexos:** Imagens copiadas para a pasta `attachments` com nome GUID para evitar conflitos.

---

## 7. Estrategia de Backup

### 7.1 Backup Automatico

- **Intervalo padrao:** A cada 15 dias (configuravel nas Settings).
- **Verificacao:** Ao iniciar o app, o `BackupSchedulerService` verifica se o ultimo backup tem mais dias do que o intervalo configurado.
- **Execucao:** Se necessario, cria o backup automaticamente em segundo plano.
- **Notificacao:** O usuario e informado via snackbar/toast apos a conclusao.

### 7.2 Backup Manual

- O usuario pode criar um backup a qualquer momento pela tela de Backup.
- O backup manual segue o mesmo formato do automatico.

### 7.3 Formato do Backup

Cada backup e um arquivo **ZIP** contendo:

```
quizcraft-backup-YYYYMMDD-HHmmss.zip
  |
  +-- quizcraft.db              # Copia do banco de dados
  +-- attachments/              # Copia de todas as imagens anexas
        +-- {guid}.png
        +-- ...
```

### 7.4 Restauracao

1. O usuario seleciona um arquivo de backup (.zip).
2. **Antes de restaurar**, o sistema cria automaticamente um **backup de seguranca** do estado atual.
3. O banco de dados e os anexos sao substituidos pelos do backup.
4. O app reinicia para carregar os novos dados.

### 7.5 Politica de Retencao

- **Padrao:** Manter os 10 backups mais recentes.
- **Configuravel:** O usuario pode alterar o numero nas Settings.
- **Execucao:** Apos cada novo backup, os backups mais antigos que excedem o limite sao removidos automaticamente.

---

## 8. Algoritmo de Repeticao Espacada

O QuizCraft implementa um algoritmo de repeticao espacada simplificado, inspirado no sistema Leitner, com 6 niveis (0 a 5).

### 8.1 Niveis e Intervalos

| Nivel | Intervalo (dias) | Descricao |
|---|---|---|
| 0 | 0 (imediato) | Questao nova ou com muitos erros |
| 1 | 1 | Revisao no dia seguinte |
| 2 | 3 | Revisao em 3 dias |
| 3 | 7 | Revisao em 1 semana |
| 4 | 14 | Revisao em 2 semanas |
| 5 | 30 | Revisao em 1 mes (dominada) |

### 8.2 Regras de Transicao

```
Resposta CORRETA:
  novo_nivel = min(nivel_atual + 1, 5)

Resposta INCORRETA:
  novo_nivel = max(nivel_atual - 2, 0)
```

### 8.3 Calculo da Proxima Revisao

```
proxima_revisao = data_atual + intervalos[novo_nivel]
```

**Exemplo:**
- Questao no Nivel 3 (intervalo 7 dias).
- Estudante **acerta**: sobe para Nivel 4, proxima revisao em 14 dias.
- Estudante **erra**: desce para Nivel 1, proxima revisao em 1 dia.

### 8.4 Fila de Revisao

A tela de **Revisao Diaria** exibe todas as questoes cuja `NextReviewDate <= hoje`, ordenadas por:
1. Nivel mais baixo primeiro (prioridade para questoes mais dificeis).
2. Data de ultima revisao mais antiga.

---

## 9. Estrutura de Projetos na Solution

```
QuizCraft.sln
  |
  +-- src/
  |     +-- QuizCraft/                    # Projeto principal (WPF App)
  |     |     +-- App.xaml                # Ponto de entrada, DI container
  |     |     +-- Views/                  # Paginas XAML
  |     |     +-- ViewModels/             # ViewModels (MVVM)
  |     |     +-- Models/                 # Entidades de dominio
  |     |     +-- Services/               # Logica de aplicacao
  |     |     +-- Data/                   # DbContext, Repositories
  |     |     +-- Converters/             # Value Converters WPF
  |     |     +-- Assets/                 # Icones, imagens, fontes
  |     |     +-- Helpers/                # Utilitarios
  |     |
  +-- tests/
  |     +-- QuizCraft.Tests/              # Testes unitarios e de integracao
  |
  +-- installer/
  |     +-- QuizCraftSetup.iss            # Script Inno Setup
  |
  +-- docs/
        +-- ARCHITECTURE.md               # Este documento
        +-- BACKLOG.md                    # Backlog de funcionalidades
        +-- TESTPLAN.md                   # Plano de testes
```

---

## 10. Injecao de Dependencias

O QuizCraft utiliza o container de DI nativo do .NET (`Microsoft.Extensions.DependencyInjection`) configurado no `App.xaml.cs`:

```
Registros:
  - DbContext          -> QuizCraftDbContext (Scoped)
  - ISubjectRepository -> SubjectRepository (Scoped)
  - ITopicRepository   -> TopicRepository (Scoped)
  - IQuestionRepository-> QuestionRepository (Scoped)
  - ...demais repos    -> Implementacoes (Scoped)
  - QuizService        -> QuizService (Transient)
  - StatisticsService  -> StatisticsService (Transient)
  - SpacedRepetitionService -> SpacedRepetitionService (Transient)
  - ImportExportService -> ImportExportService (Transient)
  - BackupService      -> BackupService (Singleton)
  - BackupSchedulerService -> BackupSchedulerService (Singleton)
  - ViewModels         -> Transient (um por navegacao)
```

---

## 11. Navegacao

A navegacao entre paginas utiliza o sistema de navegacao do WPF-UI (`INavigationService`), com um `NavigationView` como menu lateral principal:

- Dashboard (pagina inicial)
- Disciplinas / Assuntos
- Questoes
- Novo Quiz
- Revisao Diaria
- Historico
- Backup
- Configuracoes

---

## 12. Performance

- **Lazy loading** de relacoes no EF Core para evitar carregamento excessivo.
- **Paginacao** em listas de questoes e historico de sessoes.
- **Virtualizacao** de listas no WPF (`VirtualizingStackPanel`).
- **Async/await** em todas as operacoes de I/O (banco, arquivo, backup).
- **Background threads** para backup automatico e calculos pesados de estatisticas.
