using System;

namespace GameBrain
{
    public class AI
    {
        int lastHitX = -1; // coordinates
        int lastHitY = -1; // coordinates
        private Direction _lastDirection = Direction.None;

        public (int, int) getAiMoveCoords(CellState[,] board)
        {
            int x, y;
            Random r = new Random();
            Direction upDownLeftRight = Direction.None;
            if (lastHitX < 0)
            {
                x = r.Next(board.GetUpperBound(0) + 1);
                y = r.Next(board.GetUpperBound(1) + 1);
            }
            else
            {
                x = lastHitX;
                y = lastHitY;
                if (_lastDirection == Direction.None)
                {
                    int direction = r.Next(1, 5);
                    upDownLeftRight = (Direction)direction; // convert number to enum
                }
                else
                {
                    upDownLeftRight = _lastDirection;
                }

                switch (upDownLeftRight)
                {
                    case Direction.Up:
                        if (y > 0)
                        {
                            y -= 1;
                        }

                        break;
                    case Direction.Down:
                        if (y < board.GetUpperBound(1))
                        {
                            y += 1;
                        }

                        break;
                    case Direction.Left:
                        if (x > 0)
                        {
                            x -= 1;
                        }

                        break;
                    case Direction.Right:
                        if (x < board.GetUpperBound(0))
                        {
                            x += 1;
                        }

                        break;
                }
            }

            if (board[x, y] == CellState.Ship)
            {
                lastHitX = x;
                lastHitY = y;
                _lastDirection = upDownLeftRight;
            }
            else
            {
                lastHitX = lastHitY = -1;
            }

            if (board[x, y] == CellState.Miss || board[x, y] == CellState.Bomb)
            {
                (x, y) = getAiMoveCoords(board);
            }

            return (x, y);
        }
    }

    enum Direction
    {
        None,
        Up,
        Down,
        Left,
        Right
    }
}