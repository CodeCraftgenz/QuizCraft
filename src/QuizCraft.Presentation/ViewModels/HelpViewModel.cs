using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace QuizCraft.Presentation.ViewModels;

/// <summary>
/// ViewModel da tela de ajuda. Exibe seções com instruções de uso, atalhos, FAQ e formatos de importação.
/// </summary>
public partial class HelpViewModel : BaseViewModel
{
    /// <summary>Seções de ajuda exibidas na interface.</summary>
    public ObservableCollection<HelpSection> Sections { get; } = new();

    /// <summary>Popula as seções de ajuda com conteúdo estático em português.</summary>
    public override Task InitializeAsync()
    {
        Sections.Clear();

        Sections.Add(new HelpSection("Primeiros Passos",
            """
            Bem-vindo ao QuizCraft! Siga estes 3 passos para começar:

            1. Crie uma Matéria: Vá em "Biblioteca" e clique em "Nova Matéria".
               Exemplos: Matemática, Biologia, Direito Constitucional.

            2. Adicione Tópicos: Dentro da matéria, crie tópicos como "Funções", "Citologia", etc.

            3. Crie Questões: Vá em "Questões" e clique em "Nova Questão". Escolha o tópico,
               escreva o enunciado, adicione as alternativas e marque a correta.

            Pronto! Agora você pode criar quizzes e começar a estudar.
            """));

        Sections.Add(new HelpSection("Criando um Quiz",
            """
            Para criar um quiz de revisão:

            1. Vá em "Criar Quiz" no menu lateral.
            2. Selecione os filtros: matéria, tópico, dificuldade.
            3. Escolha o modo:
               • Treino: mostra a resposta correta após cada questão.
               • Prova: mostra o resultado apenas no final.
               • Revisão de Erros: prioriza questões que você errou.
               • Revisão Espaçada: traz questões baseadas no algoritmo de repetição.
            4. Configure quantidade de questões e timer (opcional).
            5. Clique em "Iniciar Quiz".
            """));

        Sections.Add(new HelpSection("Repetição Espaçada",
            """
            O QuizCraft usa um sistema de repetição espaçada para otimizar seus estudos:

            • Cada questão tem um nível de domínio de 0 a 5.
            • Quando você acerta, o nível sobe e a próxima revisão é agendada
              para mais longe (1, 3, 7, 14 ou 30 dias).
            • Quando erra, o nível desce 2 pontos e a revisão é agendada mais cedo.
            • A tela "Revisão" mostra todas as questões pendentes para revisar hoje.
            """));

        Sections.Add(new HelpSection("Backup e Restauração",
            """
            Seus dados são preciosos! O QuizCraft faz backup automático:

            • Backup automático: a cada 15 dias (configurável em Configurações).
            • Backup manual: vá em Configurações → "Criar Backup Agora".
            • Os backups ficam em: %AppData%\QuizCraft\backups\
            • São arquivos .zip contendo o banco de dados e anexos.
            • Para restaurar: selecione o backup e clique em "Restaurar".
            • Antes de restaurar, um backup de segurança é criado automaticamente.
            • Política de retenção: mantém os últimos 10 backups (configurável).
            """));

        Sections.Add(new HelpSection("Importar e Exportar",
            """
            Você pode importar e exportar questões:

            • Exportar JSON: Configurações → "Exportar Questões (JSON)".
              Gera um arquivo na Área de Trabalho.
            • Exportar CSV: útil para abrir no Excel.
            • Importar JSON: prepare o arquivo no formato correto e importe
              em Configurações.

            Formato JSON para importação:
            [
              {
                "Subject": "Matemática",
                "Topic": "Funções",
                "Type": "MultipleChoice",
                "Statement": "Qual é f(2) se f(x) = 2x + 1?",
                "Difficulty": 2,
                "Tags": ["funções", "cálculo"],
                "Choices": [
                  { "Text": "5", "IsCorrect": true },
                  { "Text": "3", "IsCorrect": false },
                  { "Text": "4", "IsCorrect": false },
                  { "Text": "6", "IsCorrect": false }
                ]
              }
            ]
            """));

        Sections.Add(new HelpSection("Atalhos de Teclado",
            """
            • Enter: confirmar resposta / avançar
            • Setas ←→: navegar entre questões
            • 1-5: selecionar alternativa (A-E)
            • M: marcar questão para revisão
            • P: pausar/retomar timer
            • Ctrl+F: busca global
            • Ctrl+N: nova questão
            • Escape: fechar editor / voltar
            """));

        Sections.Add(new HelpSection("FAQ - Perguntas Frequentes",
            """
            P: Preciso de internet para usar o QuizCraft?
            R: Não! O QuizCraft funciona 100% offline.

            P: Onde ficam meus dados?
            R: Em %AppData%\QuizCraft\ (banco de dados, logs e backups).

            P: Posso usar em mais de um computador?
            R: Sim! Exporte seus dados (backup ou JSON) e importe no outro computador.

            P: O tema escuro funciona?
            R: Sim! Vá em Configurações e ative o "Tema Escuro".

            P: Quantas questões posso cadastrar?
            R: Sem limite! O SQLite suporta milhares de questões com performance.

            P: Como desinstalar?
            R: Use o desinstalador no Menu Iniciar ou Painel de Controle.
               Seus dados em AppData são preservados (remova manualmente se quiser).
            """));

        return Task.CompletedTask;
    }
}

/// <summary>Seção de ajuda com título e conteúdo textual.</summary>
public record HelpSection(string Title, string Content);
