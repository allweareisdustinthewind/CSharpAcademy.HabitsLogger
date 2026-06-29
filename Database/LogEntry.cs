
using Microsoft.Data.Sqlite;
using Spectre.Console;
using System.Globalization;

namespace HabitsLogger.Database;

internal class LogEntry : DataEntry
{
   public int HabitId { get; set; }
   public string Date { get; set; } = string.Empty;
   public int Quantity { get; set; }
   public string QuantityDesc { get; set; } = string.Empty;

   public LogEntry ()
   {
      cmdAdd = (SqliteCommand cmd) =>
      {
         cmd.CommandText = "INSERT INTO log (HabitId, Date, Quantity) VALUES (@habitId, @date, @quantity)";
         cmd.Parameters.AddWithValue ("@habitId", HabitId);
         cmd.Parameters.AddWithValue ("@date", ConvertDateToDbFormat (Date));
         cmd.Parameters.AddWithValue ("@quantity", Quantity);
         cmd.ExecuteNonQuery ();
      };

      cmdDelete = (SqliteCommand cmd) =>
      {
         cmd.CommandText = "DELETE FROM log WHERE Id = @Id";
         cmd.Parameters.AddWithValue ("@Id", Id);
         cmd.ExecuteNonQuery ();
      };

      cmdUpdate = (SqliteCommand cmd) =>
      {
         cmd.CommandText = "UPDATE log SET Date = @date, Quantity = @quantity WHERE Id = @Id";
         cmd.Parameters.AddWithValue ("@date", ConvertDateToDbFormat (Date));
         cmd.Parameters.AddWithValue ("@quantity", Quantity);
         cmd.Parameters.AddWithValue ("@Id", Id);
         cmd.ExecuteNonQuery ();
      };
   }

   public override void GetData (SqliteDataReader reader)
   {
      Id = reader.GetInt32 (0);

      Date = ConvertDateFromDbFormat (reader.GetString (1));

      Quantity = reader.GetInt32 (2);
      QuantityDesc = reader.GetString (3);
   }

   static string ConvertDateFromDbFormat (string date)
   {
      return date.Substring (6, 2) + "-" + date.Substring (4, 2) + "-" + date.Substring (0, 4);
   }
   
   static string ConvertDateToDbFormat (string date, bool fullYear = true)
   {
      string year = !fullYear ? "20" + date.Substring (6, 2) : date.Substring (6, 4);
      return year + date.Substring (3, 2) + date.Substring (0, 2);
   }

   public static string ParseDate (string date)
   {
      if (date.Trim ().ToLower () == "now")
         return DateTime.Now.ToString ("dd-MM-yyyy");

      if (!DateTime.TryParseExact (date, "dd-MM-yy", new CultureInfo ("en-US"), DateTimeStyles.None, out var _))
         return string.Empty;

      return ConvertDateFromDbFormat (ConvertDateToDbFormat (date, false));
   }

   public static int ParseQuantity (string value)
   {
      if (!Int32.TryParse (value, out int res))
         return -1;

      return res;
   }

   public override void AppendToTable (int id, Table table)
   {
      table.AddRow (id.ToString (), Date, $"{Quantity} {QuantityDesc}");
   }

   public override void UpdateLine (int currentLine, Table? table, bool showSelected)
   {
      if (table == null)
         return;

      string [] texts = { ConvertText ((currentLine + 1).ToString (), showSelected),
                          ConvertText (Date, showSelected),
                          ConvertText ($"{Quantity} {QuantityDesc}", showSelected)};

      for (int i = 0; i < texts.Length; ++i)
         table.UpdateCell (currentLine, i, new Markup (texts [i]));
   }

   public override void UpdateCell (int currentLine, int currentColumn, Table? table, bool showSelected)
   {
      if (table == null)
         return;

      string text = currentColumn switch
      {
         0 => (currentLine + 1).ToString (),
         1 => Date,
         2 => $"{Quantity} {QuantityDesc}",
         _ => string.Empty
      };

      if (showSelected)
         text = $"[black on yellow]{text}[/]";

      table.UpdateCell (currentLine, currentColumn, new Markup (text));
   }

   public override Tuple<string, string> GetProperty (int index)
   {
      return index switch
      {
         0 => Tuple.Create ("Nr", Id.ToString ()),
         1 => Tuple.Create ("Date", Date),
         2 => Tuple.Create ("Quantity", Quantity.ToString ()),
         _ => Tuple.Create (string.Empty, string.Empty)
      };
   }

   public override string SetProperty (int index, string value)
   {
      switch (index)
      {
         case 1:
            string date = ParseDate (value);
            if (string.IsNullOrEmpty (date))
               return "The parameter 'Date' is incorrect. Expecting date in format 'dd-mm-yy'";

            Date = date;
            break;

         case 2:
            int res = ParseQuantity (value);
            if (res < 0)
               return "The parameter 'Quantity' is incorrect (not negative number expected)";

            Quantity = res;
            break;
      }

      return string.Empty;
   }

   public override bool CheckIfUnique (int index, string value, List<DataEntry> entries)
   {
      return true;
   }
}