using HabitsLogger.Database;
using Spectre.Console;

namespace HabitsLogger.UI.Table;

internal class LogTable : DataTable
{
   string _habit;

   public LogTable (string habit, ProcesMode mode = ProcesMode.View)
      : base (mode)
   { 
      _habit = habit;
   }

   protected override void Init ()
   {
      LogEntry entry = new ();

      string [] names = new string [3];
      for (int i = 0; i < 3; ++i)
         names [i] = entry.GetProperty (i).Item1;

      _table = Gui.CreateTable (names);
      _entries = DbManager.GetLogsOfHabit (_habit);
   }

   protected override void InformNoData ()
   {
      Gui.Info ($"There are no logs for habit [green]'{_habit}'[/] in database");
   }

   protected override void ShowTableTitle ()
   {
      AnsiConsole.MarkupLine ($" Logs of habit [green]'{_habit}'[/]");
   }
}
