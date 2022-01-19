using System.Collections.Generic;

namespace Domain
{
    public class GameOption
    {
        public int GameOptionId { get; set; }

        public int BoardSize { get; set; } = 10;
        public int MaxShipSize { get; set; } = 5;
        public int ShipAmount { get; set; } = 5;
        public TouchState TouchState { get; set; } = TouchState.Corner;
        public bool Combo { get; set; } = true;

        public override string ToString()
        {
            return "Board size: " + BoardSize + ", MaxShipSize: " + MaxShipSize + ", Ship amount: " + ShipAmount +
                   ", TouchState: " + TouchState + ", Combo: " + Combo;
        }
    }
}