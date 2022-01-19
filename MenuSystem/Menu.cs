using System;
using System.Collections.Generic;
using System.Linq;

namespace MenuSystem
{
    public enum MenuLevel
    {
        Level0,
        Level1,
        Level2
    }

    public class Menu
    {
        public Dictionary<string, MenuItem> MenuItems { get; set; } = new();
        private readonly MenuLevel _menuLevel;
        private readonly string[] _reservedActions;

        public Menu(MenuLevel level,string[] reservedActions)
        {
            _reservedActions = reservedActions;
            _menuLevel = level;
        }

        public void AddMenuItem(MenuItem item)
        {
            if (item.MenuLetter == "")
            {
                throw new ArgumentException("UserChoice cannot be empty");
            }

            MenuItems.Add(item.MenuLetter, item);
        }

        public void DrawMenu(int selectedElement = -1)
        {
            Console.Write("");

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Use [UP] and [DOWN] arrows to choose and [ENTER] to select!");    
            
                for (int i = 0; i < MenuItems.Count; i++)
                {
                    if (i == selectedElement)
                    {
                        Console.WriteLine(">>> " + MenuItems.ElementAt(i).Value);
                    }
                    else
                    {
                        Console.WriteLine(MenuItems.ElementAt(i).Value);
                    }
                    
                }

                selectedElement -= MenuItems.Count;
                

                switch (_menuLevel)
                {
                    case MenuLevel.Level0:
                        Console.WriteLine("=====================");
                        Console.WriteLine((selectedElement == 0 ? ">>> ":"") + "X) Exit");
                        break;
                    case MenuLevel.Level1:
                        Console.WriteLine("=====================");

                        Console.WriteLine((selectedElement == 0 ? ">>> ":"") +"M) Return to Main");
                        Console.WriteLine("");
                        Console.WriteLine((selectedElement == 1 ? ">>> ":"") +"X) Exit");
                        break;
                    case MenuLevel.Level2:
                        Console.WriteLine("");
                        Console.WriteLine((selectedElement == 0 ? ">>> ":"") +"R) Return to previous");
                        Console.WriteLine("");
                        Console.WriteLine((selectedElement == 1 ? ">>> ":"") +"M) Return to Main");
                        Console.WriteLine("");
                        Console.WriteLine((selectedElement == 2 ? ">>> ":"") +"X) Exit");
                        break;
                    default:
                        throw new Exception("Unknown menu depth!");
                }

                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.Write("-----------------------------------");
                Console.ResetColor();
                Console.WriteLine("");
                Console.Write(">>> ");
        }

        public string ArrowInput(Action boardDraw)  
        {
            int cursorIndex = 0;
            bool pressedEnter = false;

            while(!pressedEnter)
            {
                var keyPressed = Console.ReadKey().Key;
                if(keyPressed == ConsoleKey.UpArrow)
                {
                    cursorIndex--;
                    
                }
                else if (keyPressed == ConsoleKey.DownArrow)
                {
                    cursorIndex++;
                    
                }
                Console.Clear();
                // for cursor(>) limitation 
                cursorIndex = Math.Min(Math.Max(cursorIndex, 0), MenuItems.Count - 1 + _reservedActions.Length);
                boardDraw();
                DrawMenu(cursorIndex);

                pressedEnter = keyPressed == ConsoleKey.Enter;
            }

            if (cursorIndex > MenuItems.Count - 1)
            {
                cursorIndex -= MenuItems.Count - 1;
                return _reservedActions[cursorIndex -1];
            }
            
            return MenuItems.ElementAt(cursorIndex).Key;
        }
        
        public void ExecuteMenuItem(string userChoice, out string returnStatus)
        {
            returnStatus = "";
            // is it a reserved keyword
            if (!_reservedActions.Contains(userChoice))
            {
                // no it wasn't try to find keyword in MenuItems
                if (MenuItems.TryGetValue(userChoice, out var userMenuItem))
                {
                    userMenuItem.MethodToExecute();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("I dont have this option !");
                    Console.ResetColor();
                }
            }

            if (userChoice == "x")
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("Closing down.....");
                Console.ResetColor();
                returnStatus = "exit";

                //break;
            }

            if (_menuLevel != MenuLevel.Level0 && userChoice == "m")
            {
                //break;
                returnStatus = "main";
            }

            if (_menuLevel == MenuLevel.Level2 && userChoice == "r")
            {
                returnStatus = "previous";
                //break;
            }

        }
        
    }
}
