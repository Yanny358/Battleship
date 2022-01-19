using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using GameBrain;
using GameConsoleUI;
using MenuSystem;
using System.Text.Json;
using DAL;
using Domain;
using Microsoft.EntityFrameworkCore;


namespace ConsoleApp
{
    class Program
    {
        public static string basePath;
        private static Menu _currentMenu = null!;
        private static Battleship _game = null!; // for board initialization 
        private static bool _isGameOver = false;
        private static bool _wasGameLoaded;
        private static string saveMethod;
        
        static void Main(string[] args)
        {
            basePath = args.Length == 1 ? args[0] : Directory.GetCurrentDirectory();

            
            WriteConfig();

            Console.WriteLine("====== Battleship! ======");
            
            var mainMenu = new Menu(MenuLevel.Level0, new[] {"x"});

            mainMenu.AddMenuItem(new MenuItem("", "New game human vs human", "1", () => StartBattleship()));
            mainMenu.AddMenuItem(new MenuItem("", "New game puny human vs mighty AI", "2",
                () => StartBattleship(versusAi: true)));
            mainMenu.AddMenuItem(new MenuItem("", "Edit settings", "3", Settings));
            mainMenu.AddMenuItem(new MenuItem("", "Load Games", "4", LoadedGameForConsole));
            mainMenu.AddMenuItem(new MenuItem("", "Replay Games", "5", LoadReplay));
            
            do
            {
                Console.WriteLine("Choose your save method(type 'json' or 'db')");
                saveMethod = Console.ReadLine().Trim().ToLower();
            } while (saveMethod != "json" && saveMethod != "db");
            

            _currentMenu = mainMenu;
            string status = "";
            while (true)
            {
                if (_isGameOver)
                {
                    Console.WriteLine("GAME OVER!");
                    Console.ReadLine(); // when user types enter it will return to main menu
                    _currentMenu = mainMenu;
                    _isGameOver = false;
                }

                _currentMenu.DrawMenu();

                // to draw menu everytime after key stroke
                var keyStroke = _currentMenu.ArrowInput(_game != null
                    ? DrawBoards
                    : () => {});

                _currentMenu.ExecuteMenuItem(keyStroke, out status);

                if (_currentMenu.MenuItems.ContainsKey("p"))
                {
                    _currentMenu.MenuItems["p"].Label =
                        $"{(_game.NextMoveByPlayer1 ? "Player1" : "Player2")} make a move";
                }

                if (status == "exit")
                {
                    break;
                }

                if (status == "main")
                {
                    _currentMenu = mainMenu;
                }
            }
        }

        public static void DrawBoards()
        {
            BattleshipConsoleUI.DrawBoard(_game.GetBoard(2));
            Console.WriteLine("⬆️⬆️⬆️⬆️PLAYER2 BOARD⬆️⬆️⬆️⬆️");
            Console.WriteLine("================================");
            Console.WriteLine("⬇️⬇️⬇️⬇️PLAYER1 BOARD⬇️⬇️⬇️⬇️");
            BattleshipConsoleUI.DrawBoard(_game.GetBoard(1));
        }

        static void Settings()
        {
            var gameOption = new GameOption();
            string input = "";
            int boardSize = 10;
            do
            {
                Console.WriteLine("Choose boards size (min 5, max 50)");
                input = Console.ReadLine();
            } while (!int.TryParse(input, out boardSize) || boardSize is < 5 or > 50);

            gameOption.BoardSize = boardSize;
            //defaultConfig["Board size"] = boardSize.ToString();

            int maxShipSize = 5;
            do
            {
                Console.WriteLine("Choose max ship size");
                input = Console.ReadLine();
            } while (!int.TryParse(input, out maxShipSize) || maxShipSize < 1 || maxShipSize > boardSize);

            gameOption.MaxShipSize = maxShipSize;
            //defaultConfig["Max ship size"] = maxShipSize.ToString();

            int shipAmount = 5;
            do
            {
                Console.WriteLine("Choose  ship amount");
                input = Console.ReadLine();
            } while (!int.TryParse(input, out shipAmount) || shipAmount < 1 || shipAmount > (boardSize + 1) / 2);

            gameOption.ShipAmount = shipAmount;
            //defaultConfig["Ship amount"] = shipAmount.ToString();

            TouchState touchState = TouchState.Corner;
            do
            {
                Console.WriteLine("Choose touch state [yes/no/corner]");
                input = Console.ReadLine().ToLower();
            } while (!TouchState.TryParse(input, true, out touchState));

            gameOption.TouchState = touchState;
            //defaultConfig["Touch state"] = input;

            do
            {
                Console.WriteLine("Can players make a combo?[true,false]");
                input = Console.ReadLine().ToLower();
            } while (input != "true" && input != "false");

            gameOption.Combo = Boolean.Parse(input);
            //defaultConfig["Combo"] = input;

            WriteConfig(gameOption);

        }

        static void WriteConfig(GameOption gameOption = null)
        {
            /*var fileNameStandardConfig = basePath +  "Configs" +
                                         Path.DirectorySeparatorChar + filename;
            if (!File.Exists(fileNameStandardConfig) || overwrite)
            {
                var jsonOptions = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
               
                var confJsonString = JsonSerializer.Serialize(defaultConfig, jsonOptions);
                File.WriteAllText(fileNameStandardConfig,confJsonString );
            }*/

            if (saveMethod == "db")
            {
                using var db = new AppDbContext();
                if (!db.GameOptions.Any())
                {
                    db.GameOptions.Add(new GameOption());
                }

                if (gameOption != null)
                {
                    db.GameOptions.Update(gameOption);
                }

                db.SaveChanges();
            }
            else
            {
                if (gameOption == null)
                {
                    return;
                }
                var configFolderPath = basePath +  "Configs";
                var js = JsonSerializer.Serialize(gameOption);
                Console.WriteLine("Name your config");
                var fileName = Console.ReadLine();
                File.WriteAllText(Path.Join(configFolderPath, fileName) ,js);
            }

        }

        static GameOption ReadConfigFile()
        {
            /*var fileNameStandardConfig = basePath  + "Configs" +
                                         Path.DirectorySeparatorChar + filename;
            
            return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(fileNameStandardConfig));*/

            //DbContextOptions<AppDbContext> options = new DbContextOptions<AppDbContext>();
            if (saveMethod == "db")
            {
                using var db = new AppDbContext();
                var dbGameOptions = db.GameOptions.ToList();
                int i = 0;
                foreach (var options in dbGameOptions)
                {
                    Console.WriteLine(i + ")" + options);
                    i++;
                }

                string userChoice;
                int index;
                do
                {
                    Console.WriteLine("Choose game options (1,2,3 etc.)");
                    userChoice = Console.ReadLine();

                } while (!int.TryParse(userChoice, out index) || index > dbGameOptions.Count || index < 0);
                return dbGameOptions[index];
            }
            else
            {
                
                var files = Directory.EnumerateFiles(Path.Join(basePath  , "Configs")).ToList();
                for (int i = 0; i < files.Count; i++)
                {
                    Console.WriteLine($"{i} - {files[i]}");
                }

                Console.WriteLine("Choose config number (1,2,3 etc.)");
                
                var fileNo = Console.ReadLine();
                var fileName = files[int.Parse(fileNo!.Trim())];

                return JsonSerializer.Deserialize<GameOption>(File.ReadAllText(fileName));
            }

        }

        static void ChangeCurrentMenu(Menu newMenu)
        {
            _currentMenu = newMenu;
        }

        static string[] AskCoordinates()
        {
            Console.WriteLine("Upper left corner is (A,1)!");
            Console.Write($"Give X (A-{(char) (_game.GetBoard(1).GetUpperBound(0) + (int) 'A')})," +
                          $" Y (1-{_game.GetBoard(1).GetUpperBound(1) + 1}):");
            var coordinates = Console.ReadLine().Split(',');
            return coordinates;
        }

        static string AskOrientation()
        {
            var userChoice = "";
            while (userChoice != "H" && userChoice != "V")
            {
                Console.WriteLine("Choose ship orientation on board");
                Console.WriteLine("\"H\" is for Horizontal, \"V\" is for Vertical");
                userChoice = Console.ReadLine();
            }

            return userChoice;
        }

        static void CheckWhoWon(bool versusAi)
        {
            if (_game.WinCheck(_game.GetBoard(2)))
            {
                Console.WriteLine("Player 1 WIN!");
                _isGameOver = true;
            }
            else if (_game.WinCheck(_game.GetBoard(1)))
            {
                Console.WriteLine(versusAi ? "AI WIN!" : "Player 2 WIN");
                _isGameOver = true;
            }
        }

        static void StartBattleship(bool versusAi = false)
        {
            string[] userShipCoordinate = new string[0];
            var (x, y) = (0, 0); // default position

            if (!_wasGameLoaded)
            {
                _game = new Battleship(ReadConfigFile());
                _game.Start(versusAi);

                var orientation = "H";
                Random r = new Random();
                for (int p = 1; p <= 2; p++) // p is for player (V is for Vendetta)
                {
                    var randomOrNot = "";
                    if (!(versusAi && p == 2)) // nand logic
                    {
                        do
                        {
                            Console.WriteLine("Do you want to place ships randomly?[y / n]");
                            randomOrNot = Console.ReadLine().ToLower();
                        } while (randomOrNot != "y" && randomOrNot != "n");
                    }

                    if (randomOrNot == "n") Console.WriteLine("Pick ship position in board");
                    var board = _game.GetBoard(p);
                    foreach (var ship in _game.ships)
                    {
                        bool isCorrect = false; // checking for right position
                        int attemptPositionCounter = 0;
                        while (!isCorrect)
                        {
                            if (randomOrNot == "n")
                            {
                                do
                                {
                                    userShipCoordinate =
                                        AskCoordinates(); // ask user ship placement (as number and character)
                                    try
                                    {
                                        (x, y) = GetMoveCordinates(
                                            userShipCoordinate); // converts ship placement to int
                                    }
                                    catch
                                    {
                                        userShipCoordinate = new string[2]; // two empty strings
                                        Console.WriteLine("Invalid input");
                                    }
                                } while (string.IsNullOrEmpty(userShipCoordinate[0]) ||
                                         string.IsNullOrEmpty(userShipCoordinate[1]) || userShipCoordinate.Length != 2);

                                orientation = AskOrientation();
                            }
                            else
                            {
                                // if we have board size 5x5 then upper bound get last index of it which is 4
                                // 4 x 4 is 16, but we need to check 5 x 5 which is 25 positions so we need to add
                                // 1 to get 5 x 5
                                if (attemptPositionCounter >= Math.Pow(board.GetUpperBound(0) + 1, 2))
                                {
                                    Console.WriteLine("BOARD SIZE CAN NOT SUPPORT THIS MANY SHIP!");
                                    return;
                                }

                                (x, y, orientation) = _game.GetRandomShipPosition(board);
                                attemptPositionCounter++;
                            }

                            isCorrect = _game.CheckCorrectPosition(x, y, board, ship, orientation, out var status);
                            if (!isCorrect)
                            {
                                Console.WriteLine(status);
                            }
                        }

                        ship.X = x; // origin point
                        ship.Y = y; // origin point
                        ship.IsHorizontal = orientation == "H"; // sets orientation

                        _game.PlaceShip(ship, board);
                        BattleshipConsoleUI.DrawBoard(board, false);
                    }

                    Console.Clear();
                    Console.WriteLine("Player 2 board");
                }
                DrawBoards();
            }
            else
            {
                _wasGameLoaded = false;
            }

            DrawBoards();

            var battleshipMenu = new Menu(MenuLevel.Level0, new[] {"x"});
            if (!_game.IsInReplayMode)
            {

                battleshipMenu.AddMenuItem(new MenuItem(
                    "", $" {(_game.NextMoveByPlayer1 ? "Player1" : "Player2")} make a move",
                    menuLetter: "p",
                    () =>
                    {
                        Console.WriteLine("Put bomb position in board");
                        do
                        {
                            do
                            {
                                userShipCoordinate = AskCoordinates(); // ask user bomb placement (as number and character)
                            } while (userShipCoordinate.Length != 2 || userShipCoordinate[0] == "" ||
                                     userShipCoordinate[1] == "");

                            (x, y) = GetMoveCordinates(userShipCoordinate); // converts bomb placement to int
                        } while (x > _game.GetBoard(1).GetUpperBound(0) || y > _game.GetBoard(1)
                            .GetUpperBound(1) || x < 0 || y < 0);

                        _game.MakeAMove(x, y);
                        CheckWhoWon(_game.VersusAI);

                        if (_game.VersusAI && !_game.NextMoveByPlayer1 && !_isGameOver)
                        {
                            do
                            {
                                (x, y) = _game.Ai.getAiMoveCoords(_game.GetBoard(1));
                                _game.MakeAMove(x, y);
                                Console.WriteLine("AI MOVE:" + (char) ('A' + x) + ", " + (y + 1));
                            } while (_game.GetBoard(1)[x, y] == CellState.Bomb && (_game.Config.Combo) && !_game.WinCheck(_game.GetBoard(1)));
                            CheckWhoWon(_game.VersusAI);
                        }
                        DrawBoards();
                    })
                );

                battleshipMenu.AddMenuItem(new MenuItem(
                    "", $"Save game",
                    menuLetter: "s",
                    () =>
                    {
                        Console.WriteLine("Enter save name");
                        var saveName = Console.ReadLine();
                        var actualName = saveName != String.Empty ? saveName :
                            "save_" + DateTime.Now.ToString("yyyy-MM-dd");

                        if (saveMethod == "db")
                        {
                            _game.SaveToDb(actualName);
                        }
                        else
                        {
                            var saves = Path.Join(basePath, "Saves", actualName);
                            _game.SaveToFile(saves);
                        }
                    })
                );

                battleshipMenu.AddMenuItem(new MenuItem(
                    "", $"Load game",
                    menuLetter: "l",
                    LoadedGameForConsole)
                );
            }
            else
            {
                battleshipMenu.AddMenuItem(new MenuItem(
                    "", $"Next Move",
                    menuLetter: "next",
                    () =>
                    {
                        _game.ReplayForward();
                        Console.WriteLine("replay step:" + _game.ReplayStep);
                        DrawBoards();
                    })
                );
                battleshipMenu.AddMenuItem(new MenuItem(
                    "", $"Previous Move",
                    menuLetter: "prev",
                    () =>
                    {
                        _game.ReplayBackward();
                        Console.WriteLine("replay step:" + _game.ReplayStep);
                        DrawBoards();
                    })
                );
                battleshipMenu.AddMenuItem(new MenuItem(
                    "", $"Continue game",
                    menuLetter: "cont",
                    () =>
                    {
                        _game.IsInReplayMode = false;
                        _wasGameLoaded = true;
                        StartBattleship();
                    })
                );
            }
            
            ChangeCurrentMenu(battleshipMenu);

        }

        public static void LoadReplay()
        {
            var gameIndex = AskSaveIndex();

            _game = Battleship.LoadFromDb(gameIndex);
            _wasGameLoaded = true;
            _game.ClearBoardForReplay();
            _game.IsInReplayMode = true;
            StartBattleship();
        }

        public static void LoadedGameForConsole()
        {
            if (saveMethod == "db")
            {
                int gameIndex = AskSaveIndex();
                _game = Battleship.LoadFromDb(gameIndex);
            }
            else
            {
                LoadGameAction();
            }
            _wasGameLoaded = true;
            StartBattleship();
        }

        public static int AskSaveIndex()
        {
            using var db = new AppDbContext();
            var gamesFromDB = db.Games.ToList();
            int i = 0;
            foreach (var games in gamesFromDB)
            {
                Console.WriteLine(i + ")" + games.Description);
                i++;
            }

            string userChoice;
            int index;
            do
            {
                Console.WriteLine("Choose game(1,2,3 etc.)");
                userChoice = Console.ReadLine();

            } while (!int.TryParse(userChoice, out index) || index > gamesFromDB.Count || index < 0);

            return index;
        } 

        static (int x, int y) GetMoveCordinates(string[] userValue)
        {
            var x = (int) userValue[0][0]; // convert  string to char, then char to int
            x -= 'A'; // 65 - 65
            //var x = int.Parse(userValue[0].Trim()) - 1;
            var y = int.Parse(userValue[1].Trim()) - 1;

            return (x, y);
        }

        static string LoadGameAction()
        {
            var files = Directory.EnumerateFiles(Path.Join(basePath  , "Saves")).ToList();
            for (int i = 0; i < files.Count; i++)
            {
                Console.WriteLine($"{i} - {files[i]}");
            }

            var fileNo = Console.ReadLine();
            var fileName = files[int.Parse(fileNo!.Trim())];
            
            _game = Battleship.LoadGameAction(fileName);
            //BattleshipConsoleUI.DrawBoard(_game.GetBoard(2));
            //BattleshipConsoleUI.DrawBoard(_game.GetBoard(1));
            return "";
        }
        

    }
}
