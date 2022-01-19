using System.Collections.Generic;

namespace GameBrain
{
    public class GameState
    {
        public bool NextMoveByX { get; set; }
        public CellState[][] Board { get; set; } = null!;
        public CellState[][] Board2 { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
        public bool VersusAi { get; set; }
        public int[][] Journal { get; set; } = null!;
    }
}