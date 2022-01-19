namespace GameBrain
{
    public class Ship
    {
        public int X;  // coordinates
        public int Y; // coordinates
        public bool IsHorizontal;
        public int Size;
        public Ship(int x = 0, int y = 0, bool isHorizontal = true, int size = 1)
        {
            X = x;
            Y = y;
            IsHorizontal = isHorizontal;
            Size = size;
        }
    }
}