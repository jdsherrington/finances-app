using System.Text.Json;

namespace ConsoleApp
{
    class Program
    {
        static void Main()
        {
            string filePath = GetFilePath();

            // read JSON data from file
            string jsonData = File.ReadAllText(filePath);
            
            UserData userData = JsonSerializer.Deserialize<UserData>(jsonData)!;

            // handles any possible null from JSON deserialization
            if (userData == null)
            {
                Console.WriteLine("ERROR: Failed to deserialize JSON data.");

                string userInput = GetInput("Type DELETE to delete user data, or EXIT to close the app.", ["delete", "exit"]);

                switch (userInput)
                {
                    case "delete":
                        DeleteUserData();
                        break;
                    
                    case "exit":
                        Console.WriteLine("SYSTEM: Exiting program.");
                        Environment.Exit(1);
                        break;
                }
            }

            if (userData!.LastUser != null)
            {
                Console.WriteLine($"Last User: {userData!.LastUser.Username}");
                Console.WriteLine($"Last Modified Date: {userData.LastModifiedDate}");
            }
            else
            {
                CreateNewUser(userData);
            }

            


            Console.Clear();
            Console.WriteLine("Welcome to the Finances app.");
        }


        private static string GetFilePath()
        {
            // Specifies user data JSON filepath and filename
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string userDataDir = Path.Combine(appDataPath, "Finances App");
            string userDataFile = "userdata.json";
            string userDataPath = Path.Combine(userDataDir, userDataFile);

            // Checks if JSON exists
            if (!File.Exists(userDataPath))
            {
                // Create directory if doesn't it exist
                if (!Directory.Exists(userDataDir))
                {
                    Directory.CreateDirectory(userDataDir);
                    Console.WriteLine("SYSTEM: Directory created in AppData.");
                }

                // Creates the JSON file
                UserData userData = new UserData
                {
                    Users = new List<User>(),
                    LastUser = null,
                    LastModifiedDate = null
                };

                WriteToJson(userData, userDataPath);

                Console.WriteLine("SYSTEM: JSON file has been created.");
            }

            return userDataPath;
        }


        private static void DeleteUserData()
        {
            string filePath = GetFilePath();
            File.Delete(filePath);
            Console.WriteLine("SYSTEM: User data deleted. Exiting program.");
            Environment.Exit(1);
        }


        private static void ClearConsoleLines(int lines)
        {
            // Store the current cursor position
            int originalLeft = Console.CursorLeft;
            int originalTop = Console.CursorTop;

            // Move the cursor to the beginning of the last N lines
            Console.SetCursorPosition(0, Console.CursorTop - lines);

            // Clear the last N lines
            for (int i = 0; i < lines; i++)
            {
                Console.Write(new string(' ', Console.WindowWidth));
            }

            // Restore the cursor position to the original left and current top - N
            Console.SetCursorPosition(originalLeft, Console.CursorTop - lines);
            Console.WriteLine("");
        }


        private static void CreateNewUser(UserData userData)
        {
            Console.WriteLine("CREATE USER ACCOUNT\n");

            User newUser = new User
            {
                Id = userData.Users!.Count + 1,
                Name = GetInput("Your name:"),
                Username = GetInput("Username:"),
                Password = GetPasswordInput("Password:")
            };

            userData.Users.Add(newUser);
            userData.LastUser = newUser;
            UpdateLastModified(userData);
            WriteToJson(userData, GetFilePath());
        }


        private static string GetInput(string prompt, string[]? valid = null)
        {
            string? readResult;
            Console.WriteLine(prompt);
            readResult = Console.ReadLine();

            // if no list of valid inputs are provided
            if (valid == null)
            {
                // then only validate that the input isn't null or empty
                while (String.IsNullOrEmpty(readResult))
                {
                    ClearConsoleLines(2);
                    Console.WriteLine(prompt);
                    readResult = Console.ReadLine();
                    
                }
            }
            else
            {
                while (String.IsNullOrEmpty(readResult) || !Array.Exists(valid, element => element == readResult.ToLower()))
                {
                    ClearConsoleLines(2);
                    Console.WriteLine(prompt);
                    readResult = Console.ReadLine();
                }
            }

            ClearConsoleLines(2);
            return readResult;

        }

        private static string GetPasswordInput(string prompt)
        {
            Console.WriteLine(prompt);
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Check if the key is a valid character
                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    // Handle backspace: remove the last character from the password
                    password = password.Substring(0, password.Length - 1);

                    // Move the cursor back and replace the character with a space
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    Console.Write(" ");
                    Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                }

            } while (key.Key != ConsoleKey.Enter);

            // Move to the next line after pressing Enter
            Console.WriteLine();

            return password;
        }


        private static void UpdateLastModified(UserData userData)
        {
            userData.LastModifiedDate = DateTime.Now;
        }

        private static void WriteToJson(UserData userData, string userDataPath)
        {
            string initialJson = JsonSerializer.Serialize(userData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(userDataPath, initialJson);
        }

    }

    public class UserData
    {
        public List<User>? Users { get; set; }
        public User? LastUser { get; set; }
        public DateTime? LastModifiedDate { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

}