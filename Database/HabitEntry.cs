
using Microsoft.Data.Sqlite;
using Spectre.Console;

namespace HabitsLogger.Database;

internal class HabitEntry : DataEntry
{
   public string Description { get; set; } = string.Empty;
   public string QuantityDesc { get; set; } = string.Empty;

   public HabitEntry ()
   {
      cmdAdd = (SqliteCommand cmd) =>
      {
         cmd.CommandText = "INSERT INTO habits (Description, QuantityDesc) VALUES (@desc, @quantityDesc)";
         cmd.Parameters.AddWithValue ("@desc", Description);
         cmd.Parameters.AddWithValue ("@quantityDesc", QuantityDesc);
         cmd.ExecuteNonQuery ();
      };

      cmdDelete = (SqliteCommand cmd) =>
      {
         cmd.CommandText = "DELETE FROM habits WHERE Id = @Id";
         cmd.Parameters.AddWithValue ("@Id", Id);
         cmd.ExecuteNonQuery ();
      };

      cmdUpdate = (SqliteCommand cmd) =>
      {
         cmd.CommandText = "UPDATE habits SET Description = @desc, QuantityDesc = @quantityDesc WHERE Id = @Id";
         cmd.Parameters.AddWithValue ("@desc", Description);
         cmd.Parameters.AddWithValue ("@quantityDesc", QuantityDesc);
         cmd.Parameters.AddWithValue ("@Id", Id);
         cmd.ExecuteNonQuery ();
      };
   }

   public override void GetData (SqliteDataReader reader)
   {
      Id = reader.GetInt32 (0);
      Description = reader.GetString (1);
      QuantityDesc = reader.GetString (2);
   }

   public override void AppendToTable (int id, Table table)
   {
      table.AddRow (id.ToString (), Description, QuantityDesc);
   }

   public override void UpdateLine (int currentLine, Table? table, bool showSelected)
   {
      if (table == null)
         return;

      string [] texts = { ConvertText ((currentLine + 1).ToString (), showSelected),
                          ConvertText (Description, showSelected),
                          ConvertText (QuantityDesc, showSelected)};

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
         1 => Description,
         2 => QuantityDesc,
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
         1 => Tuple.Create ("Habit", Description),
         2 => Tuple.Create ("Quantity description", QuantityDesc),
         _ => Tuple.Create (string.Empty, string.Empty)
      };
   }

   public override string SetProperty (int index, string value)
   {
      switch (index)
      {
         case 1:
            Description = value;
            break;

         case 2:
            QuantityDesc = value;
            break;
      }

      return string.Empty;
   }

   public override bool CheckIfUnique (int index, string value, List<DataEntry> entries)
   {
      if (index != 1 || entries == null || entries.Count <= 0)
         return true;

      string valueToCheck = value.Trim ().ToLower ();
      foreach (var entry in entries)
      {
         string desc = ((HabitEntry) entry).Description.Trim ().ToLower ();
         if (desc == valueToCheck)
            return false;
      }

      return true;
   }
}
