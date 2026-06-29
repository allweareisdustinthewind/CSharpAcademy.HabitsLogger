using HabitsLogger.UI.Menu;
using Spectre.Console;

namespace HabitsLogger.UI;

internal class Gui
{
   public static void ShowMainTitel ()
   {
      AnsiConsole.Clear ();
      AnsiConsole.Write ("\n       ");

      var titel = new Text ("Habits Logger", new Style (foreground: Color.Yellow, decoration: Decoration.Underline));
      AnsiConsole.Write (titel);
      AnsiConsole.WriteLine ();
      AnsiConsole.WriteLine ();
   }

   public static bool ShowMainMenu ()
   {
      ShowMainTitel ();
      return new MainMenu ().Show ();
   }

   public static Spectre.Console.Table CreateTable (params string [] columns)
   {
      var table = new Spectre.Console.Table ().RoundedBorder ().BorderColor (Color.Aqua);

      foreach (string col in columns)
         table.AddColumn ($"[aqua]{col}[/]", col => col.Centered ());

      return table;
   }

   public static bool Ask (string question)
   {
      return AnsiConsole.Confirm (question);
   }

   public static string GetUserInput (string hint)
   {
      bool curVisibility = false;
      if (OperatingSystem.IsWindows ())
         curVisibility = Console.CursorVisible;

      Console.CursorVisible = true;
      AnsiConsole.Markup (hint);
      string ?value = Console.ReadLine ();
      Console.CursorVisible = curVisibility;

      return value ?? "";
   }

   public static void Info (string text)
   {
      var (x, y) = Console.GetCursorPosition ();
      Console.CursorVisible = false;
      AnsiConsole.MarkupLine (text);
      System.Threading.Thread.Sleep (2500);
      Console.CursorVisible = true;
      Console.SetCursorPosition (x, y);
   }

   public static void Exception (string text)
   {
      Info ($"[white on red] {text} [/]");
   }
}
