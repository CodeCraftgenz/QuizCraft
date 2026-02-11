# Plano de Testes do QuizCraft

## 1. Estrategia de Testes

O QuizCraft adota uma estrategia de testes em duas camadas principais:

### 1.1 Testes Unitarios

Focam na **logica de dominio e servicos da aplicacao**, isolados de dependencias externas (banco de dados, sistema de arquivos). Utilizam mocks/stubs para repositorios e servicos externos.

**Escopo:**
- Algoritmo de repeticao espacada (`SpacedRepetitionService`)
- Calculos de estatisticas (`StatisticsService`)
- Logica de criacao e execucao de quiz (`QuizService`)
- Validacoes de entidades de dominio
- Logica de importacao/exportacao (`ImportExportService`)
- Logica de backup (`BackupService`)

### 1.2 Testes de Integracao

Focam na **interacao com o banco de dados SQLite** real, validando que as queries, migrations e repositorios funcionam corretamente em conjunto.

**Escopo:**
- Repositorios (CRUD completo contra SQLite em memoria)
- Migrations do EF Core
- Queries complexas (filtros, paginacao, relacionamentos)

### 1.3 Testes de UI (Manual)

A interface WPF e testada **manualmente** seguindo checklists estruturados, pois a automacao de UI em WPF tem custo/beneficio desfavoravel para o tamanho deste projeto.

---

## 2. Frameworks e Ferramentas

| Ferramenta | Uso | Justificativa |
|---|---|---|
| **xUnit** | Framework de testes | Padrao da comunidade .NET, suporte nativo no VS |
| **FluentAssertions** | Assercoes legíveis | Sintaxe fluente, mensagens de erro claras |
| **Moq** | Mocking de interfaces | Framework de mock mais utilizado em .NET |
| **Microsoft.EntityFrameworkCore.InMemory** | Banco em memoria para testes de integracao | Isolamento, velocidade, sem I/O de disco |
| **SQLite In-Memory** | Alternativa para testes EF Core | Mais fiel ao comportamento real do SQLite |

### Estrutura do Projeto de Testes

```
tests/
  +-- QuizCraft.Tests/
        +-- Unit/
        |     +-- Services/
        |     |     +-- SpacedRepetitionServiceTests.cs
        |     |     +-- StatisticsServiceTests.cs
        |     |     +-- QuizServiceTests.cs
        |     |     +-- ImportExportServiceTests.cs
        |     |     +-- BackupServiceTests.cs
        |     |
        |     +-- Domain/
        |           +-- QuestionValidationTests.cs
        |           +-- MasteryCalculationTests.cs
        |
        +-- Integration/
        |     +-- Repositories/
        |     |     +-- SubjectRepositoryTests.cs
        |     |     +-- TopicRepositoryTests.cs
        |     |     +-- QuestionRepositoryTests.cs
        |     |     +-- QuizSessionRepositoryTests.cs
        |     |     +-- MasteryRepositoryTests.cs
        |     |
        |     +-- Data/
        |           +-- MigrationTests.cs
        |           +-- DbContextTests.cs
        |
        +-- Helpers/
              +-- TestDataBuilder.cs
              +-- DbContextFactory.cs
```

---

## 3. Cenarios de Teste Detalhados

### 3.1 Repeticao Espacada (`SpacedRepetitionServiceTests`)

| # | Cenario | Entrada | Resultado Esperado |
|---|---|---|---|
| RE-01 | Acerto em nivel 0 sobe para nivel 1 | Nivel=0, Correto=true | Nivel=1, ProximaRevisao=amanha |
| RE-02 | Acerto em nivel 4 sobe para nivel 5 | Nivel=4, Correto=true | Nivel=5, ProximaRevisao=hoje+30d |
| RE-03 | Acerto em nivel 5 permanece em 5 | Nivel=5, Correto=true | Nivel=5, ProximaRevisao=hoje+30d |
| RE-04 | Erro em nivel 3 desce para nivel 1 | Nivel=3, Correto=false | Nivel=1, ProximaRevisao=amanha |
| RE-05 | Erro em nivel 1 desce para nivel 0 | Nivel=1, Correto=false | Nivel=0, ProximaRevisao=hoje |
| RE-06 | Erro em nivel 0 permanece em 0 | Nivel=0, Correto=false | Nivel=0, ProximaRevisao=hoje |
| RE-07 | Fila de revisao retorna questoes pendentes | Questoes com NextReview <= hoje | Lista filtrada e ordenada |
| RE-08 | Fila de revisao exclui questoes futuras | Questoes com NextReview > hoje | Lista vazia ou sem essas questoes |
| RE-09 | Fila de revisao ordena por nivel (menor primeiro) | Questoes em niveis diferentes | Nivel 0 antes de nivel 3 |
| RE-10 | Primeira resposta cria registro de mastery | Questao sem Mastery | Mastery criado com nivel correto |

### 3.2 Estatisticas (`StatisticsServiceTests`)

| # | Cenario | Entrada | Resultado Esperado |
|---|---|---|---|
| ST-01 | Taxa de acerto global correta | 70 acertos / 100 total | 70.0% |
| ST-02 | Taxa de acerto com zero questoes | Nenhuma sessao | 0% (sem erro de divisao por zero) |
| ST-03 | Contagem de streak consecutivo | 5 dias seguidos com atividade | Streak=5 |
| ST-04 | Streak reseta apos dia sem estudo | 3 dias, gap de 1, 2 dias | Streak=2 (contagem atual) |
| ST-05 | Streak conta o dia atual | Atividade hoje + 4 dias anteriores | Streak=5 |
| ST-06 | Desempenho por disciplina | Sessoes em 3 disciplinas | 3 registros com % de acerto correto |
| ST-07 | Assuntos mais fracos | 5 assuntos com % variada | Top 5 ordenados por menor acerto |
| ST-08 | Acerto ao longo do tempo (por semana) | Sessoes em 4 semanas | 4 pontos no grafico |
| ST-09 | Total de questoes respondidas | Multiplas sessoes | Soma correta de todas |
| ST-10 | Total de tempo estudado | Sessoes com tempos variados | Soma em minutos/horas |

### 3.3 Motor de Quiz (`QuizServiceTests`)

| # | Cenario | Entrada | Resultado Esperado |
|---|---|---|---|
| QZ-01 | Criar quiz com filtro de disciplina | SubjectId=1, Count=10 | 10 questoes da disciplina 1 |
| QZ-02 | Criar quiz com filtro de dificuldade | Difficulty=Hard, Count=5 | 5 questoes dificeis |
| QZ-03 | Criar quiz com menos questoes disponiveis que solicitado | Count=20, disponiveis=8 | 8 questoes (todas disponiveis) |
| QZ-04 | Embaralhamento de questoes | Mesmos filtros, multiplas execucoes | Ordens diferentes (aleatorio) |
| QZ-05 | Registrar resposta correta | ChoiceId correto | IsCorrect=true, atualiza Mastery |
| QZ-06 | Registrar resposta incorreta | ChoiceId incorreto | IsCorrect=false, atualiza Mastery |
| QZ-07 | Finalizar sessao calcula totais | Sessao com 10 perguntas respondidas | TotalQuestions=10, CorrectAnswers=N |
| QZ-08 | Sessao nao finalizada pode ser retomada | Sessao com IsCompleted=false | Pode retomar do ponto de parada |
| QZ-09 | Tempo limite encerra sessao automaticamente | TimeLimitSeconds atingido | IsCompleted=true, respostas pendentes ignoradas |
| QZ-10 | Quiz sem filtros usa todas as questoes | Sem filtros, Count=10 | 10 questoes de qualquer disciplina |
| QZ-11 | Filtro por tags funciona corretamente | Tags=["OAB", "Civil"] | Somente questoes com essas tags |

### 3.4 Importacao e Exportacao (`ImportExportServiceTests`)

| # | Cenario | Entrada | Resultado Esperado |
|---|---|---|---|
| IE-01 | Exportar para JSON gera formato valido | Lista de questoes | JSON valido com todos os campos |
| IE-02 | Importar JSON restaura questoes | JSON exportado | Questoes identicas criadas no banco |
| IE-03 | Round-trip JSON preserva dados | Exportar -> Importar -> Comparar | Dados identicos |
| IE-04 | JSON com campos opcionais vazios | Questao sem explicacao, sem tags | Importa sem erros |
| IE-05 | JSON invalido retorna erro descritivo | JSON malformado | Mensagem de erro clara |
| IE-06 | Exportar para CSV gera formato correto | Lista de questoes | CSV com delimitador correto |
| IE-07 | Importar CSV com headers validos | CSV bem formatado | Questoes criadas corretamente |
| IE-08 | CSV com encoding UTF-8 BOM | Arquivo com acentos | Caracteres preservados |
| IE-09 | Importacao nao duplica questoes existentes | Importar mesmo arquivo 2x | Sem duplicatas (ou estrategia definida) |
| IE-10 | Exportacao de disciplina completa | SubjectId com questoes e alternativas | Arquivo completo com hierarquia |

### 3.5 Backup (`BackupServiceTests`)

| # | Cenario | Entrada | Resultado Esperado |
|---|---|---|---|
| BK-01 | Criar backup gera arquivo ZIP | Banco de dados existente | Arquivo .zip criado no diretorio de backups |
| BK-02 | Backup contem banco de dados | Backup criado | ZIP contem quizcraft.db |
| BK-03 | Backup contem pasta de anexos | Backup com imagens | ZIP contem attachments/ |
| BK-04 | Restaurar backup substitui banco | Backup de estado anterior | Banco restaurado ao estado do backup |
| BK-05 | Restauracao cria backup de seguranca | Backup a restaurar | Backup do estado atual criado antes |
| BK-06 | Politica de retencao remove backups antigos | 12 backups, retencao=10 | 2 mais antigos removidos |
| BK-07 | Scheduler identifica necessidade de backup | Ultimo backup ha 16 dias, intervalo=15 | Retorna true (precisa backup) |
| BK-08 | Scheduler nao executa se recente | Ultimo backup ha 5 dias, intervalo=15 | Retorna false |
| BK-09 | Backup de banco vazio funciona | Banco sem dados | ZIP criado sem erros |
| BK-10 | Restaurar arquivo corrompido retorna erro | ZIP invalido | Erro tratado, banco original intacto |

---

## 4. Criterios de Aceitacao do MVP

Os criterios abaixo devem ser atendidos para que o MVP seja considerado pronto para lancamento.

### 4.1 Gerenciamento de Conteudo

- [ ] O usuario pode criar, editar e excluir disciplinas.
- [ ] O usuario pode criar, editar e excluir assuntos vinculados a disciplinas.
- [ ] O usuario pode criar assuntos hierarquicos (assunto pai/filho).
- [ ] O usuario pode criar questoes de multipla escolha e verdadeiro/falso.
- [ ] O usuario pode definir a alternativa correta e adicionar explicacao.
- [ ] O usuario pode atribuir nivel de dificuldade e tags as questoes.
- [ ] O usuario pode buscar e filtrar questoes por disciplina, assunto, dificuldade e tags.
- [ ] O usuario pode importar e exportar questoes em formato JSON.

### 4.2 Motor de Quiz

- [ ] O usuario pode criar um quiz selecionando filtros (disciplina, assunto, dificuldade, tags, quantidade).
- [ ] O quiz funciona em modo Treino (feedback imediato) e modo Exame (feedback no final).
- [ ] As questoes e alternativas sao embaralhadas aleatoriamente.
- [ ] O modo exame suporta temporizador configuravel.
- [ ] O usuario pode pausar e retomar um quiz em andamento.
- [ ] Ao finalizar, a tela de resultado exibe o resumo de desempenho.
- [ ] O usuario pode revisar cada questao com sua resposta e a correta.

### 4.3 Repeticao Espacada

- [ ] Cada questao respondida tem seu nivel de dominio rastreado (0-5).
- [ ] A proxima data de revisao e calculada automaticamente com os intervalos [0, 1, 3, 7, 14, 30] dias.
- [ ] Acerto incrementa o nivel em 1; erro decrementa em 2 (minimo 0, maximo 5).
- [ ] A tela de Revisao Diaria exibe todas as questoes com revisao pendente.
- [ ] A fila de revisao prioriza questoes de nivel mais baixo.
- [ ] O menu de navegacao exibe um badge com o numero de revisoes pendentes.

### 4.4 Estatisticas e Dashboard

- [ ] O dashboard exibe cards com: total de questoes, taxa de acerto, streak e revisoes pendentes.
- [ ] O grafico de linha mostra a evolucao da taxa de acerto ao longo do tempo.
- [ ] O grafico de barras mostra o desempenho por disciplina.
- [ ] O dashboard identifica os assuntos mais fracos do usuario.
- [ ] O tempo total de estudo e exibido.

### 4.5 Historico

- [ ] O usuario pode visualizar a lista de sessoes realizadas (data, modo, resultado).
- [ ] O usuario pode abrir os detalhes de qualquer sessao anterior.
- [ ] O usuario pode reabrir o resultado de uma sessao concluida.
- [ ] A lista de sessoes suporta filtros por data e disciplina.
- [ ] A lista de sessoes e paginada.

### 4.6 Backup e Dados

- [ ] O backup automatico e criado a cada 15 dias (configuravel).
- [ ] O usuario pode criar backup manual a qualquer momento.
- [ ] O backup e um ZIP contendo o banco de dados e anexos.
- [ ] O usuario pode restaurar um backup, com backup de seguranca automatico antes.
- [ ] A politica de retencao remove backups antigos conforme configurado.

### 4.7 UX e Polimento

- [ ] O app suporta tema claro e escuro, seguindo o tema do Windows.
- [ ] O usuario pode alternar o tema manualmente nas configuracoes.
- [ ] Atalhos de teclado funcionam para acoes principais.
- [ ] Acoes destrutivas (excluir) exigem confirmacao.
- [ ] O layout e responsivo a diferentes tamanhos de janela.

### 4.8 Instalador

- [ ] O instalador Inno Setup gera um .exe funcional.
- [ ] O instalador cria atalhos na area de trabalho e no menu Iniciar.
- [ ] O desinstalador remove o aplicativo corretamente.
- [ ] O instalador segue versionamento semantico (SemVer).

---

## 5. Abordagem de Testes de UI (Manual)

Como a automacao de UI em WPF possui alto custo de implementacao e manutencao, os testes de interface sao realizados manualmente seguindo checklists estruturados.

### 5.1 Checklist de Navegacao

- [ ] Todos os itens do menu lateral navegam para a pagina correta.
- [ ] O botao Voltar funciona em todas as paginas de detalhe.
- [ ] A navegacao por teclado (Tab, Enter, Esc) funciona em todas as paginas.
- [ ] A pagina inicial carrega o dashboard corretamente.

### 5.2 Checklist de Formularios

- [ ] Campos obrigatorios exibem validacao ao submeter vazio.
- [ ] Campos de texto aceitam caracteres especiais e acentos.
- [ ] Dropdowns carregam opcoes corretamente.
- [ ] Botoes de salvar desabilitam durante a operacao (evitar duplo clique).
- [ ] Mensagem de sucesso e exibida apos salvar.
- [ ] Mensagem de erro e exibida ao falhar.

### 5.3 Checklist de Quiz

- [ ] O quiz carrega as questoes conforme os filtros selecionados.
- [ ] As alternativas sao clicaveis e destacam a selecionada.
- [ ] No modo Treino, o feedback aparece apos cada resposta.
- [ ] No modo Exame, o feedback aparece apenas no final.
- [ ] O temporizador conta regressivamente e encerra a sessao ao zerar.
- [ ] O botao de pausar interrompe o quiz e o temporizador.
- [ ] A tela de resultado exibe os dados corretos.

### 5.4 Checklist de Temas

- [ ] O tema claro exibe todas as paginas sem problemas visuais.
- [ ] O tema escuro exibe todas as paginas sem problemas visuais.
- [ ] A alternancia de tema aplica imediatamente, sem reiniciar.
- [ ] Graficos e icones sao visiveis em ambos os temas.

### 5.5 Checklist de Performance

- [ ] O app inicia em menos de 3 segundos.
- [ ] A navegacao entre paginas e instantanea (< 500ms).
- [ ] Listas com 1000+ itens nao apresentam travamento.
- [ ] O backup nao congela a interface (execucao em background).

### 5.6 Checklist de Instalador

- [ ] O instalador executa sem erros no Windows 10.
- [ ] O instalador executa sem erros no Windows 11.
- [ ] O atalho na area de trabalho abre o aplicativo.
- [ ] O atalho no menu Iniciar abre o aplicativo.
- [ ] O desinstalador remove todos os arquivos do programa.
- [ ] O desinstalador preserva os dados do usuario (%AppData%).

---

## 6. Cobertura de Testes

### 6.1 Metas de Cobertura

| Camada | Meta | Justificativa |
|---|---|---|
| Domain (Entidades, Validacoes) | 90%+ | Logica critica, sem dependencias externas |
| Application (Services) | 80%+ | Logica de negocio principal |
| Infrastructure (Repositories) | 70%+ | Testes de integracao com SQLite |
| Presentation (ViewModels) | 50%+ | Logica de apresentacao, bindings testados via UI manual |

### 6.2 O Que NAO Testar Automaticamente

- Renderizacao visual de componentes WPF.
- Animacoes e transicoes.
- Layout pixel-perfect.
- Comportamento do instalador Inno Setup.

---

## 7. Convencoes de Teste

### 7.1 Nomenclatura

Os testes seguem o padrao **MetodoSobTeste_Cenario_ResultadoEsperado**:

```csharp
// Exemplo
public class SpacedRepetitionServiceTests
{
    [Fact]
    public void CalculateNextReview_CorrectAnswerAtLevel0_ReturnsLevel1WithTomorrow()
    {
        // Arrange
        var service = new SpacedRepetitionService();
        var mastery = new Mastery { Level = 0 };

        // Act
        var result = service.CalculateNextReview(mastery, isCorrect: true);

        // Assert
        result.Level.Should().Be(1);
        result.NextReviewDate.Should().Be(DateTime.Today.AddDays(1));
    }

    [Fact]
    public void CalculateNextReview_WrongAnswerAtLevel3_ReturnsLevel1WithTomorrow()
    {
        // Arrange
        var service = new SpacedRepetitionService();
        var mastery = new Mastery { Level = 3 };

        // Act
        var result = service.CalculateNextReview(mastery, isCorrect: false);

        // Assert
        result.Level.Should().Be(1);
        result.NextReviewDate.Should().Be(DateTime.Today.AddDays(1));
    }
}
```

### 7.2 Padrao AAA (Arrange-Act-Assert)

Todos os testes seguem o padrao **Arrange-Act-Assert**:

1. **Arrange:** Configurar o cenario (criar objetos, mocks, dados de entrada).
2. **Act:** Executar a acao sob teste.
3. **Assert:** Verificar o resultado esperado.

### 7.3 Test Data Builder

Utilizar um `TestDataBuilder` centralizado para criar entidades de teste, evitando duplicacao e facilitando manutencao:

```csharp
public static class TestDataBuilder
{
    public static Subject CreateSubject(string name = "Matematica")
        => new Subject { Name = name, IsActive = true, CreatedAt = DateTime.Now };

    public static Topic CreateTopic(int subjectId, string name = "Algebra")
        => new Topic { SubjectId = subjectId, Name = name, IsActive = true };

    public static Question CreateQuestion(int topicId, string text = "Quanto e 2+2?")
        => new Question
        {
            TopicId = topicId,
            Text = text,
            Difficulty = Difficulty.Easy,
            QuestionType = QuestionType.MultipleChoice,
            IsActive = true,
            Choices = new List<Choice>
            {
                new() { Text = "3", IsCorrect = false, Order = 1 },
                new() { Text = "4", IsCorrect = true, Order = 2 },
                new() { Text = "5", IsCorrect = false, Order = 3 },
                new() { Text = "6", IsCorrect = false, Order = 4 },
            }
        };

    public static Mastery CreateMastery(int questionId, int level = 0)
        => new Mastery
        {
            QuestionId = questionId,
            Level = level,
            NextReviewDate = DateTime.Today,
            TimesCorrect = 0,
            TimesIncorrect = 0
        };
}
```

---

## 8. Execucao dos Testes

### 8.1 Linha de Comando

```bash
# Executar todos os testes
dotnet test

# Executar com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Executar apenas testes unitarios
dotnet test --filter "Category=Unit"

# Executar apenas testes de integracao
dotnet test --filter "Category=Integration"
```

### 8.2 Integracao Continua (Futuro)

Quando um pipeline de CI for configurado, os testes devem:
1. Executar em cada push para a branch principal.
2. Bloquear merge se qualquer teste falhar.
3. Gerar relatorio de cobertura.
4. Notificar em caso de regressao de cobertura.

---

## 9. Riscos e Mitigacoes

| Risco | Probabilidade | Impacto | Mitigacao |
|---|---|---|---|
| SQLite InMemory comporta-se diferente do arquivo | Media | Medio | Testes de integracao criticos usam arquivo temporario |
| Testes de UI manuais podem ser esquecidos | Alta | Medio | Checklist obrigatorio antes de cada release |
| Cobertura baixa em ViewModels | Media | Baixo | Logica complexa fica nos Services, nao nos VMs |
| Testes lentos com banco real | Baixa | Baixo | Paralelizacao e banco em memoria |
| Falsos positivos em testes de aleatoriedade (shuffle) | Media | Baixo | Testar distribuicao estatistica, nao ordem exata |
