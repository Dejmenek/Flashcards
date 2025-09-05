using Flashcards.Models;

using Spectre.Console;

namespace Flashcards.Helpers;

public static class DataVisualizer
{
    private static readonly string[] months = {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    };

    public static void ShowCards(List<BaseCardDto> cards)
    {
        if (cards is [] || cards.Count == 0)
        {
            AnsiConsole.MarkupLine("[red]No cards found.[/]");
            return;
        }

        var table = new Table().Title("[bold yellow]CARDS[/]");
        table.AddColumn("Id");
        table.AddColumn("Type");
        table.AddColumn("Details");

        int index = 1;
        foreach (var card in cards)
        {
            string details = card switch
            {
                FlashcardDto f => $"[green]Front:[/] {Markup.Escape(f.Front)}\n[blue]Back:[/] {Markup.Escape(f.Back)}",
                ClozeCardDto c => $"[green]Cloze:[/] {Markup.Escape(c.ClozeText)}",
                FillInCardDto fi => $"[green]FillIn:[/] {Markup.Escape(fi.FillInText)}\n[blue]Answers:[/] {Markup.Escape(string.Join(", ", fi.Answer))}",
                MultipleChoiceCardDto mc => $"[green]Question:[/] {Markup.Escape(mc.Question)}\n[blue]Choices:[/] {Markup.Escape(string.Join(", ", mc.Choices))}\n[magenta]Answer:[/] {Markup.Escape(string.Join(", ", mc.Answer))}",
                _ => "[red]Unknown card type[/]"
            };

            table.AddRow(
                index.ToString(),
                card.CardType.ToString(),
                details
            );
            index++;
        }

        AnsiConsole.Write(table);
    }

    public static void ShowStacks(List<StackDto> stacks)
    {
        if (stacks is [])
        {
            AnsiConsole.MarkupLine("No stacks found.");
            return;
        }
        var table = new Table().Title("STACKS");

        table.AddColumn("Name");

        foreach (StackDto stack in stacks)
        {
            table.AddRow(stack.Name);
        }

        AnsiConsole.Write(table);
    }

    public static void ShowStacksSummary(List<StackSummaryDto> stackSummaries)
    {
        if (stackSummaries is [])
        {
            AnsiConsole.MarkupLine("No stacks found.");
            return;
        }
        var table = new Table().Title("STACKS SUMMARY");

        table.AddColumn("Name");
        table.AddColumn("Due Cards");
        table.AddColumn("Total Cards");

        foreach (StackSummaryDto stackSummary in stackSummaries)
        {
            table.AddRow(stackSummary.Name, stackSummary.DueCards.ToString(), stackSummary.TotalCards.ToString());
        }

        AnsiConsole.Write(table);
    }

    public static void ShowStudySessions(List<StudySessionDto> studySessions)
    {
        if (studySessions is [])
        {
            AnsiConsole.MarkupLine("No study sessions found.");
            return;
        }

        var table = new Table().Title("STUDY SESSIONS");
        int index = 1;

        table.AddColumn("Id");
        table.AddColumn("Date");
        table.AddColumn("Score");

        foreach (StudySessionDto studySession in studySessions)
        {
            table.AddRow(index.ToString(), studySession.Date.ToString(), studySession.Score.ToString());
            index++;
        }

        AnsiConsole.Write(table);
    }

    public static void ShowMonthlyStudySessionReport(IEnumerable<MonthlyStudySessionsNumberData> monthlyStudySessionsNumbers)
    {
        if (!monthlyStudySessionsNumbers.Any())
        {
            AnsiConsole.MarkupLine("No study sessions found.");
            return;
        }

        var table = new Table().Title("MONTHLY STUDY SESSION");

        table.AddColumn("StackName");

        foreach (string month in months)
        {
            table.AddColumn(month);
        }

        foreach (var monthlyStudySessionNumberRecord in monthlyStudySessionsNumbers)
        {
            table.AddRow(monthlyStudySessionNumberRecord.StackName,
                monthlyStudySessionNumberRecord.JanuaryNumber.ToString(), monthlyStudySessionNumberRecord.FebruaryNumber.ToString(),
                monthlyStudySessionNumberRecord.MarchNumber.ToString(), monthlyStudySessionNumberRecord.AprilNumber.ToString(),
                monthlyStudySessionNumberRecord.MayNumber.ToString(), monthlyStudySessionNumberRecord.JuneNumber.ToString(),
                monthlyStudySessionNumberRecord.JulyNumber.ToString(), monthlyStudySessionNumberRecord.AugustNumber.ToString(),
                monthlyStudySessionNumberRecord.SeptemberNumber.ToString(), monthlyStudySessionNumberRecord.OctoberNumber.ToString(),
                monthlyStudySessionNumberRecord.NovemberNumber.ToString(), monthlyStudySessionNumberRecord.DecemberNumber.ToString()
                );
        }

        AnsiConsole.Write(table);
    }

    public static void ShowMonthlyStudySessionAverageScoreReport(IEnumerable<MonthlyStudySessionsAverageScoreData> monthlyStudySessionsAverageScores)
    {
        if (!monthlyStudySessionsAverageScores.Any())
        {
            AnsiConsole.MarkupLine("No study sessions found.");
            return;
        }

        var table = new Table().Title("MONTHLY STUDY SESSION AVERAGE SCORES");

        table.AddColumn("StackName");

        foreach (string month in months)
        {
            table.AddColumn(month);
        }

        foreach (var monthlyStudySessionAverageScoreRecord in monthlyStudySessionsAverageScores)
        {
            table.AddRow(monthlyStudySessionAverageScoreRecord.StackName,
                monthlyStudySessionAverageScoreRecord.JanuaryAverageScore.ToString(), monthlyStudySessionAverageScoreRecord.FebruaryAverageScore.ToString(),
                monthlyStudySessionAverageScoreRecord.MarchAverageScore.ToString(), monthlyStudySessionAverageScoreRecord.AprilAverageScore.ToString(),
                monthlyStudySessionAverageScoreRecord.MayAverageScore.ToString(), monthlyStudySessionAverageScoreRecord.JuneAverageScore.ToString(),
                monthlyStudySessionAverageScoreRecord.JulyAverageScore.ToString(), monthlyStudySessionAverageScoreRecord.AugustAverageScore.ToString(),
                monthlyStudySessionAverageScoreRecord.SeptemberAverageScore.ToString(), monthlyStudySessionAverageScoreRecord.OctoberAverageScore.ToString(),
                monthlyStudySessionAverageScoreRecord.NovemberAverageScore.ToString(), monthlyStudySessionAverageScoreRecord.DecemberAverageScore.ToString()
                );
        }

        AnsiConsole.Write(table);
    }

    public static void ShowFlashcardFront(FlashcardDto flashcard)
    {
        var table = new Table();

        table.AddColumn("Front");
        table.AddRow(flashcard.Front);

        AnsiConsole.Write(table);
    }

    public static void ShowMultipleChoiceCard(string question)
    {
        var questionPanel = new Panel(question)
            .Header("Question")
            .Border(BoxBorder.Rounded)
            .Padding(2, 2, 2, 2);

        AnsiConsole.Write(new Align(questionPanel, HorizontalAlignment.Center));
    }
}