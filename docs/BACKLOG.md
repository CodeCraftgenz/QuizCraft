# Backlog do QuizCraft

## Legenda

- **MVP** - Funcionalidades essenciais para a primeira versao funcional do produto.
- **PRO** - Funcionalidades avancadas planejadas para versoes futuras.
- **Prioridade:** Alta / Media / Baixa
- **Status:** Pendente / Em Progresso / Concluido

---

## Visao Geral

O MVP cobre aproximadamente 70% das funcionalidades listadas, entregando um produto completo e utilizavel para estudo. As funcionalidades PRO adicionam gamificacao, recursos avancados de conteudo e integracao estendida.

---

## Backlog Completo

### Epic 1: Gerenciamento de Conteudo

| Epic | Feature | Prioridade | MVP/PRO | Status |
|---|---|---|---|---|
| Gerenciamento de Conteudo | CRUD de Disciplinas (criar, editar, excluir, listar) | Alta | MVP | Pendente |
| Gerenciamento de Conteudo | CRUD de Assuntos com hierarquia (assunto pai/filho) | Alta | MVP | Pendente |
| Gerenciamento de Conteudo | CRUD de Questoes (multipla escolha, V/F) | Alta | MVP | Pendente |
| Gerenciamento de Conteudo | Cadastro de alternativas com marcacao de correta | Alta | MVP | Pendente |
| Gerenciamento de Conteudo | Campo de explicacao da resposta na questao | Alta | MVP | Pendente |
| Gerenciamento de Conteudo | Definir nivel de dificuldade (Facil, Medio, Dificil) | Media | MVP | Pendente |
| Gerenciamento de Conteudo | Sistema de tags para categorizar questoes | Media | MVP | Pendente |
| Gerenciamento de Conteudo | Busca e filtros por disciplina, assunto, dificuldade, tags | Media | MVP | Pendente |
| Gerenciamento de Conteudo | Importacao de questoes via JSON | Media | MVP | Pendente |
| Gerenciamento de Conteudo | Exportacao de questoes via JSON | Media | MVP | Pendente |
| Gerenciamento de Conteudo | Importacao de questoes via CSV | Baixa | PRO | Pendente |
| Gerenciamento de Conteudo | Exportacao de questoes via CSV | Baixa | PRO | Pendente |
| Gerenciamento de Conteudo | Soft delete com possibilidade de reativar registros | Media | MVP | Pendente |
| Gerenciamento de Conteudo | Contador de questoes por disciplina e assunto | Baixa | MVP | Pendente |

### Epic 2: Motor de Quiz

| Epic | Feature | Prioridade | MVP/PRO | Status |
|---|---|---|---|---|
| Motor de Quiz | Criar quiz com filtros (disciplina, assunto, dificuldade, tags) | Alta | MVP | Pendente |
| Motor de Quiz | Definir quantidade de questoes no quiz | Alta | MVP | Pendente |
| Motor de Quiz | Modo Treino (feedback imediato apos cada resposta) | Alta | MVP | Pendente |
| Motor de Quiz | Modo Exame (feedback somente no final) | Alta | MVP | Pendente |
| Motor de Quiz | Embaralhamento de questoes e alternativas | Alta | MVP | Pendente |
| Motor de Quiz | Temporizador configuravel (modo exame) | Media | MVP | Pendente |
| Motor de Quiz | Pausar e retomar quiz em andamento | Media | MVP | Pendente |
| Motor de Quiz | Exibir explicacao apos resposta (modo treino) | Alta | MVP | Pendente |
| Motor de Quiz | Tela de resultado com resumo de desempenho | Alta | MVP | Pendente |
| Motor de Quiz | Revisar respostas no final da sessao | Media | MVP | Pendente |
| Motor de Quiz | Filtrar quiz por questoes nunca respondidas | Baixa | PRO | Pendente |
| Motor de Quiz | Filtrar quiz por questoes mais erradas | Baixa | PRO | Pendente |

### Epic 3: Repeticao Espacada

| Epic | Feature | Prioridade | MVP/PRO | Status |
|---|---|---|---|---|
| Repeticao Espacada | Rastreamento de nivel de dominio por questao (0-5) | Alta | MVP | Pendente |
| Repeticao Espacada | Calculo automatico da proxima data de revisao | Alta | MVP | Pendente |
| Repeticao Espacada | Fila de revisao diaria (questoes pendentes) | Alta | MVP | Pendente |
| Repeticao Espacada | Tela dedicada de Revisao Diaria | Alta | MVP | Pendente |
| Repeticao Espacada | Indicador visual do nivel de dominio da questao | Media | MVP | Pendente |
| Repeticao Espacada | Progressao: acerto sobe nivel, erro desce 2 niveis | Alta | MVP | Pendente |
| Repeticao Espacada | Intervalos configurados: [0, 1, 3, 7, 14, 30] dias | Alta | MVP | Pendente |
| Repeticao Espacada | Badge/contador de revisoes pendentes na navegacao | Media | MVP | Pendente |

### Epic 4: Estatisticas e Dashboard

| Epic | Feature | Prioridade | MVP/PRO | Status |
|---|---|---|---|---|
| Estatisticas e Dashboard | Cards resumo: total questoes, acertos, sequencia, revisoes pendentes | Alta | MVP | Pendente |
| Estatisticas e Dashboard | Grafico de linha: taxa de acerto ao longo do tempo | Alta | MVP | Pendente |
| Estatisticas e Dashboard | Grafico de barras: desempenho por disciplina | Media | MVP | Pendente |
| Estatisticas e Dashboard | Tabela de desempenho por assunto | Media | MVP | Pendente |
| Estatisticas e Dashboard | Identificacao dos assuntos mais fracos | Media | MVP | Pendente |
| Estatisticas e Dashboard | Total de tempo estudado | Baixa | MVP | Pendente |
| Estatisticas e Dashboard | Grafico de distribuicao de dificuldade | Baixa | PRO | Pendente |
| Estatisticas e Dashboard | Mapa de calor de dias estudados | Baixa | PRO | Pendente |

### Epic 5: Historico de Sessoes

| Epic | Feature | Prioridade | MVP/PRO | Status |
|---|---|---|---|---|
| Historico de Sessoes | Lista de sessoes realizadas com data, modo e resultado | Alta | MVP | Pendente |
| Historico de Sessoes | Detalhes da sessao (cada questao, resposta e resultado) | Alta | MVP | Pendente |
| Historico de Sessoes | Reabrir resultado de sessao anterior | Media | MVP | Pendente |
| Historico de Sessoes | Filtros por data, disciplina e modo | Media | MVP | Pendente |
| Historico de Sessoes | Exportar historico de sessao para CSV | Baixa | PRO | Pendente |
| Historico de Sessoes | Paginacao da lista de sessoes | Media | MVP | Pendente |

### Epic 6: Backup e Dados

| Epic | Feature | Prioridade | MVP/PRO | Status |
|---|---|---|---|---|
| Backup e Dados | Backup automatico a cada 15 dias (configuravel) | Alta | MVP | Pendente |
| Backup e Dados | Verificacao de necessidade de backup ao iniciar o app | Alta | MVP | Pendente |
| Backup e Dados | Backup manual sob demanda | Alta | MVP | Pendente |
| Backup e Dados | Formato ZIP contendo banco de dados e anexos | Alta | MVP | Pendente |
| Backup e Dados | Restauracao a partir de arquivo de backup | Alta | MVP | Pendente |
| Backup e Dados | Backup de seguranca automatico antes de restaurar | Alta | MVP | Pendente |
| Backup e Dados | Politica de retencao (manter N backups mais recentes) | Media | MVP | Pendente |
| Backup e Dados | Configuracao do intervalo de backup nas Settings | Media | MVP | Pendente |
| Backup e Dados | Lista de backups disponiveis com data e tamanho | Media | MVP | Pendente |
| Backup e Dados | Notificacao apos backup automatico concluido | Baixa | MVP | Pendente |

### Epic 7: UX e Polimento

| Epic | Feature | Prioridade | MVP/PRO | Status |
|---|---|---|---|---|
| UX e Polimento | Tema claro e escuro (seguir tema do Windows) | Alta | MVP | Pendente |
| UX e Polimento | Alternancia manual de tema nas configuracoes | Media | MVP | Pendente |
| UX e Polimento | Atalhos de teclado para acoes principais | Media | MVP | Pendente |
| UX e Polimento | Feedback visual em acoes (snackbar/toast) | Media | MVP | Pendente |
| UX e Polimento | Loading states e indicadores de progresso | Media | MVP | Pendente |
| UX e Polimento | Dialogo de confirmacao para acoes destrutivas | Alta | MVP | Pendente |
| UX e Polimento | Tela de boas-vindas / onboarding no primeiro uso | Baixa | PRO | Pendente |
| UX e Polimento | Pagina de ajuda com instrucoes de uso | Baixa | PRO | Pendente |
| UX e Polimento | Acessibilidade: navegacao por teclado completa | Baixa | PRO | Pendente |
| UX e Polimento | Acessibilidade: suporte a leitor de tela | Baixa | PRO | Pendente |
| UX e Polimento | Responsividade: layout adaptavel ao tamanho da janela | Media | MVP | Pendente |

### Epic 8: Gamificacao

| Epic | Feature | Prioridade | MVP/PRO | Status |
|---|---|---|---|---|
| Gamificacao | Sequencia de estudo (streak) com contagem de dias consecutivos | Media | MVP | Pendente |
| Gamificacao | Meta semanal de questoes configuravel | Baixa | PRO | Pendente |
| Gamificacao | Progresso visual da meta semanal | Baixa | PRO | Pendente |
| Gamificacao | Sistema de badges por conquistas | Baixa | PRO | Pendente |
| Gamificacao | Badge: primeira sessao concluida | Baixa | PRO | Pendente |
| Gamificacao | Badge: 7 dias consecutivos de estudo | Baixa | PRO | Pendente |
| Gamificacao | Badge: 100 questoes respondidas | Baixa | PRO | Pendente |
| Gamificacao | Badge: todas as questoes de uma disciplina dominadas | Baixa | PRO | Pendente |
| Gamificacao | Tela de conquistas e progresso | Baixa | PRO | Pendente |

### Epic 9: Recursos Avancados

| Epic | Feature | Prioridade | MVP/PRO | Status |
|---|---|---|---|---|
| Recursos Avancados | Questoes de multipla selecao (mais de uma correta) | Media | PRO | Pendente |
| Recursos Avancados | Senha de acesso ao aplicativo | Baixa | PRO | Pendente |
| Recursos Avancados | Exportar quiz/resultados em PDF | Baixa | PRO | Pendente |
| Recursos Avancados | Anexar imagens as questoes | Media | PRO | Pendente |
| Recursos Avancados | Visualizador de imagens na questao | Media | PRO | Pendente |
| Recursos Avancados | Duplicar questao existente | Baixa | PRO | Pendente |
| Recursos Avancados | Duplicar quiz com mesmos filtros | Baixa | PRO | Pendente |

### Epic 10: Instalador e Distribuicao

| Epic | Feature | Prioridade | MVP/PRO | Status |
|---|---|---|---|---|
| Instalador e Distribuicao | Script Inno Setup para gerar instalador .exe | Alta | MVP | Pendente |
| Instalador e Distribuicao | Atalho na area de trabalho e menu Iniciar | Alta | MVP | Pendente |
| Instalador e Distribuicao | Desinstalador funcional | Alta | MVP | Pendente |
| Instalador e Distribuicao | Versionamento do instalador (SemVer) | Media | MVP | Pendente |
| Instalador e Distribuicao | Icone personalizado do aplicativo | Media | MVP | Pendente |
| Instalador e Distribuicao | Tela de licenca no instalador (MIT) | Baixa | MVP | Pendente |
| Instalador e Distribuicao | Verificacao de versao do .NET instalada | Media | MVP | Pendente |
| Instalador e Distribuicao | Publicacao self-contained (sem dependencia de .NET no PC) | Media | PRO | Pendente |

---

## Resumo Quantitativo

| Categoria | MVP | PRO | Total |
|---|---|---|---|
| Gerenciamento de Conteudo | 12 | 2 | 14 |
| Motor de Quiz | 10 | 2 | 12 |
| Repeticao Espacada | 8 | 0 | 8 |
| Estatisticas e Dashboard | 6 | 2 | 8 |
| Historico de Sessoes | 5 | 1 | 6 |
| Backup e Dados | 10 | 0 | 10 |
| UX e Polimento | 7 | 4 | 11 |
| Gamificacao | 1 | 8 | 9 |
| Recursos Avancados | 0 | 7 | 7 |
| Instalador e Distribuicao | 7 | 1 | 8 |
| **Total** | **66** | **27** | **93** |

**Percentual MVP:** ~71% das funcionalidades.

---

## Ordem Sugerida de Implementacao (MVP)

1. **Infraestrutura** - Configurar projeto, DbContext, migrations, DI
2. **Gerenciamento de Conteudo** - CRUD completo de disciplinas, assuntos e questoes
3. **Motor de Quiz** - Criacao e execucao de quizzes
4. **Repeticao Espacada** - Algoritmo de niveis e tela de revisao
5. **Estatisticas e Dashboard** - Cards e graficos de desempenho
6. **Historico** - Lista e detalhes de sessoes
7. **Backup** - Automatico, manual, restauracao
8. **UX e Polimento** - Temas, atalhos, feedbacks
9. **Instalador** - Inno Setup, atalhos, desinstalador
10. **Testes** - Unitarios, integracao, testes manuais de UI
