using HabitsLogger.Database;
using HabitsLogger.UI.Table;
using Spectre.Console;
namespace HabitsLogger.UI.Menu;

internal class HabitsMenu : ConsoleMenu
{
   public HabitsMenu (string title, DataTable.ProcesMode mode)
      : base (title)
   {
      var habits = DbManager.GetAllHabits ();

      MenuAction action = mode switch
      {
         DataTable.ProcesMode.View => ViewLogs,
         DataTable.ProcesMode.Add => AddLog,
         DataTable.ProcesMode.Delete => DeleteLog,
         DataTable.ProcesMode.Update => UpdateLog,
         _ => throw new NotSupportedException ()
      };

      foreach (var habit in habits)
         AddItem (((HabitEntry) habit).Description, action);

      AddItem ("<Back to previous menu>", (item) => { return true; });
   }

   bool ViewLogs (string item)
   {
      LogTable table = new (item);
      table.Show ();
      return false;
   }

   bool AddLog (string item)
   {
      HashSet<string> names = new ();

      var habits = DbManager.GetAllHabits ();
      string habitName = item.Trim ().ToLower ();

      LogEntry? newLog = null;
      foreach (var habit in habits)
      {
         HabitEntry entry = (HabitEntry) habit;
         if (entry.Description.Trim ().ToLower () == habitName)
         {
            newLog = new ();
            newLog.HabitId = entry.Id;
            newLog.QuantityDesc = entry.QuantityDesc;
            break;
         }
      }

      if (newLog == null)
         return false;


      string prevAnswer = string.Empty;

      for (; ; )
      {
         Console.Clear ();
         Gui.ShowMainTitel ();
         AnsiConsole.MarkupLine ($"Add new log to habit [green]'{item}'[/]\n");

         string input = Gui.GetUserInput ("Please enter a date of new log entry in format 'dd-mm-yy' or type 'now' to use actual date: ");
         string date = LogEntry.ParseDate (input);
         if (string.IsNullOrEmpty (date))
         {
            Gui.Exception ($"The parameter was entered in incorrect format. Expected format: dd-mm-yy");
            continue;
         }

         newLog.Date = date;

         prevAnswer = "Please enter a date of new log entry in format 'dd-mm-yy' or type 'now' to use actual date: " + input;

         break;
      }

      for (; ; )
      {
         int quantity = LogEntry.ParseQuantity (Gui.GetUserInput ($"Please enter quantity in {newLog.QuantityDesc} (not negative number): "));
         if (quantity < 0)
         {
            Gui.Exception ($"The entered parameter is incorrect. Expected not negative number");
            
            Console.Clear ();
            Gui.ShowMainTitel ();
            AnsiConsole.MarkupLine ($"Add new log to habit [green]'{item}'[/]\n");
            AnsiConsole.WriteLine (prevAnswer);

            continue;
         }

         newLog.Quantity = quantity;
         break;
      }

      DbManager.ExecuteCommand (newLog.cmdAdd);
      
      Gui.Info ($"\n[green]The new log was appened to the database[/]");

      return false;
   }

   bool DeleteLog (string item)
   {
      LogTable table = new LogTable (item, DataTable.ProcesMode.Delete);
      table.Show ();

      return false;
   }

   bool UpdateLog (string item)
   {
      LogTable table = new LogTable (item, DataTable.ProcesMode.Update);
      table.Show ();

      return false;
   }
}
