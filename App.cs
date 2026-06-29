
using HabitsLogger.Database;
using HabitsLogger.UI;

namespace HabitsLogger;

internal class App
{
   public void Run ()
   {
      if (DbManager.CreateEmptyDatabase ())
      {
         if (Gui.Ask ("\nThe database has no data. Would you like to populate it with some random records?"))
            DbManager.FillWithRandomData ();
      }

      while (!Gui.ShowMainMenu ())
      {
      }
   }
}
