using Microsoft.Data.Sqlite;

namespace HabitsLogger.Database;

/// <summary>
/// Class to manage all CRUD operations with database
/// </summary>
internal class DbManager
{
   readonly static string _connectionString = "Data Source=log.db";

   public delegate void SqlCmd (SqliteCommand cmd);

   public static void ExecuteCommand (SqlCmd? cmd)
   {
      if (cmd == null)
         return;

      using (var connection = new SqliteConnection (_connectionString))
      {
         connection.Open ();

         cmd.Invoke (connection.CreateCommand ());
         connection.Close ();
      }
   }

   public static bool CreateEmptyDatabase ()
   {
      bool newDatabase = false;
      ExecuteCommand ((cmd) =>
      {
         cmd.CommandText = @"CREATE TABLE IF NOT EXISTS habits
                            (
                               Id INTEGER PRIMARY KEY AUTOINCREMENT,
                               Description TEXT UNIQUE NOT NULL,
                               QuantityDesc TEXT NOT NULL
                            )";

         cmd.ExecuteNonQuery ();
         cmd.CommandText = @"CREATE TABLE IF NOT EXISTS log 
                             (
                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                HabitId INTEGER NOT NULL,
                                Date TEXT NOT NULL,
                                Quantity INTEGER,
                                FOREIGN KEY (HabitId) REFERENCES habits(Id)
                                ON DELETE CASCADE
                             )";
         cmd.ExecuteNonQuery ();

         cmd.CommandText = @"SELECT COUNT (*) cnt FROM habits";
         var res = cmd.ExecuteScalar ();
         newDatabase = res == null || res.ToString () == "0";
      });

      return newDatabase;
   }

   public static void FillWithRandomData ()
   {
      List<Tuple<string, string, int>> habits = new ()
      {
         Tuple.Create ("Run", "km", 10),
         Tuple.Create ("Read a book", "pages", 400),
         Tuple.Create ("Sport", "minutes", 60),
         Tuple.Create ("Learn foreign language", "minutes", 120),
         Tuple.Create ("Watch tv", "hours", 3),
         Tuple.Create ("Write poems", "quatrians", 10),
         Tuple.Create ("Stick to phone", "minutes", 30),
         Tuple.Create ("Play videogame", "hours", 3),
         Tuple.Create ("Drink beer", "bottles", 6),
         Tuple.Create ("Play with cat", "minutes", 30),
         Tuple.Create ("Learn quantum physics", "minutes", 30),
         Tuple.Create ("Try to understand 'Rust'", "hours", 2)
      };

      DateTime dt = DateTime.Now.AddDays (-30);

      int maxHabits = 4;
      int maxDays = 40;
      int maxLogs = 10;

      Random rand = new ();

      ExecuteCommand ((cmd) => 
      {
         HashSet<int> usedHabits = new ();

         string values = string.Empty;
         for (int i = 0; i < maxHabits; ++i)
         {
            int curHabit = -1;
            for (; ; )
            {
               curHabit = rand.Next (0, habits.Count);
               if (usedHabits.Contains (curHabit))
                  continue;

               usedHabits.Add (curHabit);
               break;
            }

            var habit = habits [curHabit];

            if (string.IsNullOrEmpty (values))
               values += $" ('{habit.Item1}', '{habit.Item2}')";
            else
               values += $", ('{habit.Item1}', '{habit.Item2}')";
         }

         cmd.CommandText = $"INSERT INTO habits (Description, QuantityDesc) VALUES {values}";
         cmd.ExecuteNonQuery ();

         cmd.CommandText = "SELECT Id, Description FROM habits";
         var reader = cmd.ExecuteReader ();

         List<Tuple<int, string>> habitsFromDb = new ();
         while (reader.Read ())
         {
            habitsFromDb.Add (Tuple.Create (reader.GetInt32 (0), reader.GetString (1)));
         }

         values = string.Empty;
         foreach (var (id, desc) in habitsFromDb)
         {
            int index = habits.FindIndex (x => x.Item1 == desc);
            if (index < 0)
               continue;

            int maxQuantity = habits [index].Item3;
            int logs = rand.Next (1, maxLogs + 1);
            
            for (int j = 0; j < logs; ++j)
            {
               int dayOffset = rand.Next (0, maxDays + 1);
               string date = dt.AddDays (dayOffset).ToString ("yyyyMMdd");

               int quantity = rand.Next (1, maxQuantity + 1);
               if (string.IsNullOrEmpty (values))
                  values = $" ({id}, '{date}', {quantity})";
               else
                  values += $", ({id}, '{date}', {quantity})";
            }
         }

         reader.Close ();
         cmd.CommandText = $"INSERT INTO log (HabitId, Date, Quantity) VALUES {values}";
         cmd.ExecuteNonQuery ();
      });
   }

   public static List<DataEntry> GetAllHabits ()
   {
      List<DataEntry> habits = new ();

      ExecuteCommand ((cmd) =>
      {
         cmd.CommandText = "SELECT * FROM habits ORDER BY Description";
         var reader = cmd.ExecuteReader ();

         while (reader.Read ())
         {
            HabitEntry entry = new ();
            entry.GetData (reader);
            habits.Add (entry);
         }
      });

      return habits;
   }

   public static List<DataEntry> GetLogsOfHabit (string habit)
   {
      List<DataEntry> logs = new ();

      ExecuteCommand ((cmd) =>
      {
         cmd.CommandText = "SELECT log.Id, log.Date, log.Quantity, habits.QuantityDesc FROM log " +
                           "INNER JOIN habits ON habits.Id = log.HabitId " +
                           "WHERE habits.Description = @desc " +
                           "ORDER BY Date";

         cmd.Parameters.AddWithValue ("@desc", habit);

         var reader = cmd.ExecuteReader ();
         while (reader.Read ())
         {
            LogEntry log = new ();
            log.GetData (reader);
            logs.Add (log);
         }
      });

      return logs;
   }
}
