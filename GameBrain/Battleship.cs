using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using DAL;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace GameBrain
{
    public class Battleship
    {
        public GameOption Config;
        private  CellState[,] _board = new CellState[10,10];
        private  CellState[,] _board2 = new CellState[10,10];
        public int maxShipSize = 5;
        public AI Ai = new AI();
        public Ship[] ships;
        public bool VersusAI = false;
        public List<(int x, int y)> journal = new List<(int, int)>();
        public int ReplayStep = -1;
        public bool IsInReplayMode = false;

        public Battleship(GameOption config)
        {

            Config = config;
            var size = Config.BoardSize;
            _board = new CellState [size, size];
            _board2 = new CellState [size, size];
            maxShipSize = Config.MaxShipSize;
            ships = new Ship[(Config.ShipAmount)];
        }

        public void Start(bool versusAi = false)
        {
            VersusAI = versusAi;
            for (int i = 0; i < ships.Length; i++)
            {
                ships[i] = new Ship(size: Math.Max(maxShipSize - i, 1));
            }
        }

        public (int x, int y, string orientation) GetRandomShipPosition(CellState[,] board)
        {
            Random r = new();
            int x = r.Next(0, board.GetUpperBound(0) + 1); // because random is exclusive
            int y = r.Next(0, board.GetUpperBound(1) + 1); // we need to add 1 
            string orientation = r.Next(2) == 0 ? "H" : "V";
            return (x, y, orientation);
        }
        
        public CellState[,] GetBoard(int board)
        {
            CellState[,] selectedBoard = board == 1 ? _board : _board2;

            return selectedBoard;
        }

        public bool CheckCorrectPosition(int x, int y, CellState[,] board,Ship ship, string orientation, out string status)
        {
            for (int i = 0; i < ship.Size; i++)
            {
                
                var xi = orientation == "H" ? i : 0; // if orientation is H then check x
                var yi = orientation == "V" ? i : 0; // if orientation is V then check y

                if (x + xi < 0 || y + yi < 0 || x + xi > board.GetUpperBound(0)
                    || y + yi > board.GetUpperBound(1))
                {
                    status = "Out of board bounds! ";
                    return false;
                }

                if (board[x + xi, y + yi] != CellState.Empty
                    || !CheckNeighbours(x + xi, y + yi, board)) // check row availability
                {
                    status = "Position is occupied, try another position!";
                    return false;
                }

            }

            status = "OK";

            return true;
        }
        
        public bool NextMoveByPlayer1 { get;  set; } = true;
        
        public void MakeAMove(int x, int y)  // checks board at position 
        {
            
            var board = NextMoveByPlayer1 ? _board2 : _board;

            if (!IsInReplayMode)
            {
                journal.Add((x,y));
            }

            if (board[x, y] == CellState.Empty)
            {
                board[x, y] = CellState.Miss;
            }
            else if(board[x,y] == CellState.Ship)
            {
                board[x, y] = CellState.Bomb;
                if (Config.Combo) return;   // if hit then hit again
            }
            ChangeTurn();
        }

        public void ChangeTurn()
        {
            NextMoveByPlayer1 = !NextMoveByPlayer1;
        }
        
        public bool WinCheck(CellState[,] board)
        {
            for (int y = 0; y <= board.GetUpperBound(1); y++)
            {
                for (int x = 0; x <= board.GetUpperBound(0); x++)
                {
                    if (board[x,y] == CellState.Ship)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void PlaceShip(Ship ship, CellState[,] board)
        {
            for (int i = 0; i < ship.Size; i++)
            {
               
                if (ship.IsHorizontal)
                {
                    board[ship.X + i,ship.Y] = CellState.Ship;
                }
                else
                {
                    board[ship.X,ship.Y + i] = CellState.Ship;
                }
            }
        }
        
        public bool CheckNeighbours(int x, int y, CellState[,] board)
        {
            if (Config.TouchState == TouchState.Yes) return true;
            
            if (x > 0)
            {
                if (board[x-1,y] == CellState.Ship)
                {
                    return false;
                }
            }

            if (x < board.GetUpperBound(0))  // getting width
            {
                if (board[x+1,y] == CellState.Ship)
                {
                    return false;
                }
            }

            if (y > 0)
            {
                if (board[x,y-1] == CellState.Ship)
                {
                    return false;
                }
            }
            if (y < board.GetUpperBound(1))  // getting height
            {
                if (board[x,y+1] == CellState.Ship)
                {
                    return false;
                }
            }

            if (Config.TouchState == TouchState.Corner) return true;
            if (x > 0 && y > 0)  // getting upper left diagonal
            {
                if (board[x-1,y-1] == CellState.Ship)
                {
                    return false;
                }
            }
            if (x < board.GetUpperBound(0) && y < board.GetUpperBound(1))  // getting bottom right diagonal
            {
                if (board[x+1,y+1] == CellState.Ship)
                {
                    return false;
                }
            }
            if (x < board.GetUpperBound(0) && y > 0)  // getting upper right diagonal
            {
                if (board[x+1,y-1] == CellState.Ship)
                {
                    return false;
                }
            }
            if (x > 0 && y < board.GetUpperBound(1))  // getting down left diagonal
            {
                if (board[x-1,y+1] == CellState.Ship)
                {
                    return false;
                }
            }

            return true;
        }

        public void ReplayForward()
        {
            ReplayStep++;
            if (ReplayStep >= journal.Count)
            {
                ReplayStep = journal.Count - 1;
                return;
            }
            
            MakeAMove(journal[ReplayStep].x, journal[ReplayStep].y);
        }
        
        public void ReplayBackward()
        {
            ReplayStep--;
            if (ReplayStep < 0)
            {
                ReplayStep = 0; return;
            };

            var (x, y) = journal[ReplayStep];
            if (GetBoard(NextMoveByPlayer1 ? 2 : 1)[x, y] == CellState.Bomb)
            {
                GetBoard(NextMoveByPlayer1 ? 2 : 1)[x, y] = CellState.Ship;
            }
            else
            {
                GetBoard(NextMoveByPlayer1 ? 2 : 1)[x, y] = CellState.Empty;
            }
            ChangeTurn();
        }

        public void ClearBoardForReplay()
        {
            for (int i = 0; i <= _board.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= _board.GetUpperBound(1); j++)
                {
                    if (_board[i, j] == CellState.Miss)
                    {
                        _board[i, j] = CellState.Empty;
                        
                    }
                    else if (_board[i, j] == CellState.Bomb)
                    {
                        _board[i, j] = CellState.Ship;
                    }
                }
            }
            
            for (int i = 0; i <= _board2.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= _board2.GetUpperBound(1); j++)
                {
                    if (_board2[i, j] == CellState.Miss)
                    {
                        _board2[i, j] = CellState.Empty;
                        
                    }
                    else if (_board2[i, j] == CellState.Bomb)
                    {
                        _board2[i, j] = CellState.Ship;
                    }
                }
            }
        }
        
        public string GetSerializedGameState()
        {
            
            var state = new GameState
            {
                NextMoveByX = NextMoveByPlayer1, 
                Width = _board.GetLength(0), 
                Height = _board.GetLength(1),
                VersusAi = VersusAI,
            };
            
            state.Board = new CellState[state.Width ][];
            state.Board2 = new CellState[state.Width ][];
            
            for (var i = 0; i < state.Board.Length; i++)
            {
                state.Board[i] = new CellState[state.Height];
                state.Board2[i] = new CellState[state.Height];
            }

            for (var x = 0; x < state.Width; x++)
            {
                for (var y = 0; y < state.Height; y++)
                {
                    state.Board[x][y] = _board[x, y];
                    state.Board2[x][y] = _board2[x, y];
                }
            }

            state.Journal = new int[journal.Count][];
            for (int i = 0; i <= state.Journal.GetUpperBound(0); i++)
            {
                state.Journal[i] = new int[2];
                state.Journal[i][0] = journal[i].x;
                state.Journal[i][1] = journal[i].y;
            }

            var jsonOptions = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(state, jsonOptions);
            
        }

        public void SetGameStateFromJsonString(string jsonString)
        {
            var state = JsonSerializer.Deserialize<GameState>(jsonString);
            
            // restore actual state from deserialized state
            VersusAI = state!.VersusAi;
            NextMoveByPlayer1 = state!.NextMoveByX;
            _board =  new CellState[state.Width, state.Height];
            _board2 =  new CellState[state.Width, state.Height];
            
            for (var x = 0; x < state.Width; x++)
            {
                for (var y = 0; y < state.Height; y++)
                {
                    _board[x, y] = state.Board[x][y];
                    _board2[x, y] = state.Board2[x][y];
                }
            }
            
            journal.Clear();
            for (int i = 0; i <= state.Journal.GetUpperBound(0); i++)
            {
                journal.Add((state.Journal[i][0], state.Journal[i][1]));
            }
        }

        public static Battleship LoadGameAction(string fileName)
        {
            var jsonString = System.IO.File.ReadAllText(fileName);

            var gameJs = jsonString.Split("divider")[0];
            var configJs = jsonString.Split("divider")[1];
            var newBattleship = new Battleship(JsonSerializer.Deserialize<GameOption>(configJs));

            newBattleship.SetGameStateFromJsonString(gameJs);

            return newBattleship;

        }

        public string SaveToFile(string filename)
        {
            var defaultName = "save_" + DateTime.Now.ToString("yyyy-MM-dd");
            
            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = defaultName;
            }

            var serializedGame = GetSerializedGameState();
            var serializedConfig = JsonSerializer.Serialize(Config);
            File.WriteAllText(filename, serializedGame + "divider" + serializedConfig);

            return "";
        }

        public void SaveToDb(string description = "First vs Second Player")
        {
            using var db = new AppDbContext();

            if (description == string.Empty) description = "save_" + DateTime.Now.ToString("yyyy-MM-dd");
                var game = new Game()
            {
                Description = description,
                GameOption = Config,
                BoardData = GetSerializedGameState()
            };

            db.GameOptions.Update(Config);

            db.Games.Add(game);
            
            // this will actually save data to db
            db.SaveChanges();
            
        }

        public static Battleship LoadFromDb(int saveIndex = 0)
        {
            using var db = new AppDbContext();
            var saveGame = db.Games.ToList()[saveIndex];

            var x = db.GameOptions.First(x => x.GameOptionId == saveGame.GameOptionId); 
                                                                                        // wont work without it

            Debug.Assert(saveGame.GameOption != null, "saveGame.GameOption != null");
            var loadBs = new Battleship(saveGame.GameOption);
            loadBs.SetGameStateFromJsonString(saveGame.BoardData);
            return loadBs;
        }
    }
}