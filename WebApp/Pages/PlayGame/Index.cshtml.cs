using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GameBrain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;


namespace WebApp.Pages.PlayGame
{
    public class Index : PageModel
    {
        private readonly DAL.AppDbContext _context;

        public IList<GameOption> GameOptions { get; set; } = default!;

        public Battleship? BattleShip { get; set; }


        public Index(DAL.AppDbContext context)
        {
            _context = context;
        }

        public GameState CurrentGameState;

        public string Winner = "Has saved the game";

        public enum GameState
        {
            NotStarted,
            PlacingShips,
            Started,
            Replay,
            Over
        }

        private bool _Player1Random;
        private bool _Player2Random;

        public async Task OnGetAsync(int x = -1, int y = -1,
            bool o = true, int gameToLoad = -1, bool isReplay = false,
            string replayFunction = "none")
        {
            GameOptions = await _context.GameOptions.ToListAsync(); 
            int? gs = HttpContext.Session.GetInt32("GameState");
            if (gs != null)
            {
                CurrentGameState = (GameState) gs;
            }

            if (gameToLoad != -1)
            {
                BattleShip = Battleship.LoadFromDb(gameToLoad);
                if (isReplay)
                {
                    CurrentGameState = GameState.Replay;
                    BattleShip.ClearBoardForReplay();
                    BattleShip.IsInReplayMode = true;
                    BattleShip.NextMoveByPlayer1 = true;
                }
                else
                {
                    CurrentGameState = GameState.Started;
                }
                
                WriteGameToSession();
            }

            switch (CurrentGameState)
            {
                case GameState.Over:
                    CurrentGameState = GameState.NotStarted;
                    break;
                case GameState.PlacingShips:
                {
                    BattleShip = ReadBattleship();
                    if (x < 0 || y < 0) break;
                    
                    int currentPlayer = BattleShip.NextMoveByPlayer1 ? 1 : 2;
                    CellState[,] currentBoard = BattleShip.GetBoard(currentPlayer);

                    _Player1Random = bool.Parse(HttpContext.Session.GetString("Player1Random"));
                    _Player2Random = bool.Parse(HttpContext.Session.GetString("Player2Random"));
                    if ((currentPlayer == 1 && _Player1Random) || (currentPlayer == 2 && _Player2Random))
                    {
                        foreach (var ship in BattleShip.ships)
                        {
                            var isCorrect = false;

                            int shipX = 0, shipY = 0;
                            string orientation = "V";

                            while (!isCorrect)
                            {
                                (shipX, shipY, orientation) = BattleShip.GetRandomShipPosition(currentBoard);
                                isCorrect = BattleShip.CheckCorrectPosition(shipX, shipY, currentBoard, ship, orientation,
                                    out string status);
                            }

                            ship.X = shipX;
                            ship.Y = shipY;
                            ship.IsHorizontal = orientation == "H";
                            BattleShip.PlaceShip(ship, currentBoard);
                        }
                        NextStep();
                    
                    }
                    else
                    {
                        int shipToPlace = HttpContext.Session.GetInt32("ShipPlaced") ?? 0;
                        BattleShip.ships[shipToPlace].X = x;
                        BattleShip.ships[shipToPlace].Y = y;
                        BattleShip.ships[shipToPlace].IsHorizontal = o;
                        if (BattleShip.CheckCorrectPosition(x, y, currentBoard, BattleShip.ships[shipToPlace],
                            o ? "H" : "V",
                            out string status))
                        {
                            BattleShip.PlaceShip(BattleShip.ships[shipToPlace], currentBoard);
                            shipToPlace++;
                            if (shipToPlace < BattleShip.ships.Length)
                            {
                                HttpContext.Session.SetInt32("ShipPlaced", shipToPlace);
                            }
                            else
                            {
                                HttpContext.Session.SetInt32("ShipPlaced", 0);
                                NextStep();
                            }
                        }
                    }

                    WriteGameToSession();
                    break;
                }
                case GameState.Started:
                {
                    
                    BattleShip = ReadBattleship();
                    if (x < 0 || y < 0) break;
                    BattleShip.MakeAMove(x, y);
                        
                    CheckWhoWon();
                    if (BattleShip.VersusAI && !BattleShip.NextMoveByPlayer1 && CurrentGameState != GameState.Over)
                    {
                        
                        do
                        {
                            
                            (x, y) = BattleShip.Ai.getAiMoveCoords(BattleShip.GetBoard(1));
                            BattleShip.MakeAMove(x, y);
                            Console.WriteLine(BattleShip.NextMoveByPlayer1);
                        } while (BattleShip.GetBoard(1)[x, y] == CellState.Bomb && (BattleShip.Config.Combo) && !BattleShip!.WinCheck(BattleShip.GetBoard(1)));
                        CheckWhoWon();
                    }

                    
                    WriteGameToSession();
                    break;
                }
                case GameState.Replay:
                    BattleShip = ReadBattleship();
                    if (replayFunction == "forward")
                    {
                        BattleShip!.ReplayForward();
                        CheckWhoWon();
                    }
                    else if (replayFunction == "backward") BattleShip!.ReplayBackward();
                    else if (replayFunction == "continue")
                    {
                        CurrentGameState = GameState.Started;
                        BattleShip.IsInReplayMode = false;
                    }
                    WriteGameToSession();
                    break;
            }

            if (CurrentGameState == GameState.Over)
            {
                Winner = HttpContext.Session.GetString("Winner");
            }
        }

        public void OnPostAsync()
        {
            int? gs = HttpContext.Session.GetInt32("GameState");
            if (gs != null)
            {
                CurrentGameState = (GameState) gs;
            }

            Console.WriteLine(CurrentGameState);

            if (Request.Form["saveName"] != default(StringValues))
            {
                var saveName = Request.Form["saveName"];
                ReadBattleship().SaveToDb(saveName);
                return;
            }
            
            if (CurrentGameState is GameState.NotStarted or GameState.Over)
            {
                var gameOptionsList = _context.GameOptions.ToList();
                var requestGameOptions = Request.Form["selectOptions"];
                var requestVersus = Request.Form["VersusRadio"];
                HttpContext.Session.SetString("Player1Random",(Request.Form["Player1Random"] == "on").ToString());
                HttpContext.Session.SetString("Player2Random",(Request.Form["Player2Random"] == "on").ToString());
                BattleShip = new Battleship(gameOptionsList[int.Parse(requestGameOptions)]);
                BattleShip.Start(requestVersus == "AI");
                CurrentGameState = GameState.PlacingShips;
                WriteGameToSession();
            }
            
        }

        private void WriteGameToSession()
        {
            HttpContext.Session.SetInt32("GameState", (int) CurrentGameState);
            var stateOfGame = Newtonsoft.Json.JsonConvert.SerializeObject(BattleShip);
            var board = BattleShip?.GetSerializedGameState();
            HttpContext.Session.SetString("Game", stateOfGame);
            HttpContext.Session.SetString("Board", board);
            
        }

        private Battleship ReadBattleship()
        {
            BattleShip =
                Newtonsoft.Json.JsonConvert.DeserializeObject<Battleship>(HttpContext.Session.GetString("Game"));
            BattleShip.SetGameStateFromJsonString(HttpContext.Session.GetString("Board"));
            return BattleShip;
        }
        
        private void NextStep ()
        {
           // Debug.Assert(BattleShip != null, nameof(BattleShip) + " != null"); // Rider assertion
            if (!BattleShip!.NextMoveByPlayer1)
            {
                CurrentGameState = GameState.Started;
            }
            BattleShip.ChangeTurn();
        }

        private void CheckWhoWon()
        {
            if (BattleShip!.WinCheck(BattleShip.GetBoard(1)) || BattleShip.WinCheck(BattleShip.GetBoard(2)))
            {
                Console.WriteLine("Winner is: " + " " + BattleShip.NextMoveByPlayer1);
                
                HttpContext.Session.SetString("Winner", BattleShip.NextMoveByPlayer1 ?
                    "PLAYER 1! 🎆" : BattleShip.VersusAI ?
                        "AI! 🎆" : "PLAYER 2! 🎆");
                CurrentGameState = GameState.Over;

            }
        }
    }
}