using HabitsLogger.Database;
using HabitsLogger.UI.Table;
using Spectre.Console;

namespace HabitsLogger.UI.Menu;

internal class MainMenu : ConsoleMenu
{
   public MainMenu ()
   {
      AddItem ("View logs", ViewLogs);
      AddItem ("Add log", AddLog);
      AddItem ("Delete log", DeleteLog);
      AddItem ("Update log", UpdateLog);
      AddItem ("─────────", null);
      AddItem ("View habits", ViewHabits);
      AddItem ("Add habit", AddHabit);
      AddItem ("Delete habit", DeleteHabit);
      AddItem ("Update habit", UpdateHabit);
      AddItem ("Exit", (item) => { return true; });
   }

   bool ViewLogs (string item)
   {
      new HabitsMenu ("Please select a habit to view its logs:", DataTable.ProcesMode.View).Show ();
      return false;
   }

   bool AddLog (string item)
   {
      new HabitsMenu ("Please select a habit to append log to it:", DataTable.ProcesMode.Add).Show ();
      return false;
   }

   bool DeleteLog (string item)
   {
      new HabitsMenu ("Please select a habit to delete its logs:", DataTable.ProcesMode.Delete).Show ();
      return false;
   }

   bool UpdateLog (string item)
   {
      new HabitsMenu ("Please select a habit to update its logs: ", DataTable.ProcesMode.Update).Show ();
      return false;
   }

   bool ViewHabits (string item)
   {
      HabitTable table = new ();
      table.Show ();
      return false;
   }

   bool AddHabit (string item)
   {
      HashSet<string> names = new ();

      var habits = DbManager.GetAllHabits ();
      foreach (var habit in habits)
         names.Add (((HabitEntry) habit).Description.Trim ().ToLower ());

      HabitEntry newHabit = new ();

      for (; ; )
      {
         newHabit.Description = Gui.GetUserInput ("Please enter a new habit to add: ");
         if (names.Contains (newHabit.Description.Trim ().ToLower ()))
         {
            Gui.Exception ($"The habit '{newHabit.Description}' already exists and can't be append to the database");
            Console.Clear ();
            Gui.ShowMainTitel ();
            continue;
         }

         newHabit.QuantityDesc = Gui.GetUserInput ("Please enter a description of quantity (i.e. 'minutes'): ");

         DbManager.ExecuteCommand (newHabit.cmdAdd);

         Gui.Info ($"The habit [green]'{newHabit.Description}'[/] was appened to the database");

         break;
      }
      
      return false;
   }

   bool DeleteHabit (string item)
   {
      HabitTable table = new (DataTable.ProcesMode.Delete);
      table.Show ();
      return false;
   }

   bool UpdateHabit (string item)
   {
      HabitTable table = new (DataTable.ProcesMode.Update);
      table.Show ();
      return false;
   }
}
