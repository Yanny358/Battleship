using System;
using GameBrain;

namespace GameConsoleUI
{
    public class BattleshipConsoleUI
    {
        public static void DrawBoard(CellState[,] board, bool gameStarted = true)
        {
            // add plus 1, since this is 0 based 
            var width = board.GetUpperBound(0)+1; // x
            var height = board.GetUpperBound(1)+1; // y

            Console.WriteLine();
            Console.Write("\t");  //for padding
            Console.ForegroundColor = ConsoleColor.Blue;
            for (int columnIndex = 0; columnIndex < width; columnIndex++) // for letters
            {
                Console.Write("  ");
                Console.Write((char)('A' + columnIndex)); // convert to characters
                Console.Write("  ");
            }

            Console.WriteLine();
            Console.Write("\t");
            for (int columnIndex = 0; columnIndex < width; columnIndex++)
            {
                Console.Write($"+---+");
            }

            Console.WriteLine();
            for (int rowIndex = 0; rowIndex < height; rowIndex++)
            {
                Console.Write(rowIndex + 1 + "\t"); //for side numbers
                for (int columnIndex = 0; columnIndex < width; columnIndex++)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    if (gameStarted && board[columnIndex,rowIndex]== CellState.Ship )
                    {
                        Console.Write($"| {CellString(CellState.Empty)} |");
                    }
                    else
                    {
                        if (board[columnIndex,rowIndex] == CellState.Ship)Console.ForegroundColor = ConsoleColor.Yellow;
                        if (board[columnIndex,rowIndex] == CellState.Bomb)Console.ForegroundColor = ConsoleColor.Red;
                        if (board[columnIndex,rowIndex] == CellState.Miss)Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write($"| {CellString(board[columnIndex, rowIndex])} |");

                    }
                }

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine();
                Console.Write("\t");
                for (int columnIndex = 0; columnIndex < width; columnIndex++)
                {
                    Console.Write($"+---+");
                }

                Console.WriteLine();
            }
        }

        public static string CellString(CellState cellState)
        {
            switch (cellState)
            {
              case CellState.Empty: return " ";
              case CellState.Bomb: return "᳀";
              case CellState.Miss: return "᳃";
              case CellState.Ship: return "▦";
            }

            return "-";
        }
    }
}