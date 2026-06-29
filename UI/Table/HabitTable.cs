using HabitsLogger.Database;
using Spectre.Console;

namespace HabitsLogger.UI.Table;

internal class HabitTable : DataTable
{
   public HabitTable (ProcesMode mode = ProcesMode.View)
      : base (mode)
   { 
   }

   protected override void Init ()
   {
      HabitEntry entry = new ();
      
      string [] names = new string [3];
      for (int i = 0; i < 3; ++i)
         names [i] = entry.GetProperty (i).Item1;

      _table = Gui.CreateTable (names);
      _entries = DbManager.GetAllHabits ();
   }

   protected override void InformNoData ()
   {
      Gui.Info ($"There are no habits in database");
   }

   protected override void ShowTableTitle ()
   {
      AnsiConsole.MarkupLine ($" All registered habits");
   }
}
