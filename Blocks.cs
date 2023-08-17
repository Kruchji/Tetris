using System.Collections.Generic;

namespace tetris
{
    // stores row and column position, can be addressed by name
    public class Position
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }

    // abstract -> derive classes for each specific block from this class
    public abstract class Block
    {
        // contains tile positions in 4 rotation variants (how tile looks in each rotation)
        protected abstract Position[][] Tiles { get; }

        // where block spawns in the grid
        protected abstract Position startOffset { get; }

        // id to distinguish blocks, 0 means empty tile
        public abstract int Id { get; }

        private int rotationState;  // how the block is rotated
        public Position offset { get; } // contains current position of the block

        public Block()
        {
            offset = new Position(startOffset.Row, startOffset.Column); // place block on starting position
        }

        // get tiles occupied by block in its current rotation and position
        public IEnumerable<Position> TilePositions()
        {
            foreach (Position p in Tiles[rotationState])    // loop over tile positions
            {
                yield return new Position(p.Row + offset.Row, p.Column + offset.Column);
                // yield = provide the next value in iteration
                // when used loop over TilePositions()
            }
        }

        // rotations in both directions
        public void RotateCW()
        {
            rotationState = (rotationState + 1) % Tiles.Length;
        }

        public void RotateCCW()
        {
            if (rotationState == 0)
            {
                rotationState = Tiles.Length - 1;
            }
            else
            {
                rotationState--;
            }
        }

        // moves block by given number of rows and columns
        public void Move(int rows, int columns)
        {
            offset.Row += rows;
            offset.Column += columns;
        }

        // resets rotation and position to starting state
        public void Reset()
        {
            rotationState = 0;
            offset.Row = startOffset.Row;
            offset.Column = startOffset.Column;
        }
    }

    // I-Block
    public class IBlock : Block
    {
        // rotation states
        private readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new(1, 0), new(1,1), new(1,2), new(1,3) },
            new Position[] { new(0, 2), new(1,2), new(2,2), new(3,2) },
            new Position[] { new(2, 0), new(2,1), new(2,2), new(2,3) },
            new Position[] { new(0, 1), new(1,1), new(2,1), new(3,1) }
        };

        public override int Id => 1;
        protected override Position startOffset => new Position(-1, 3);     // top row in the middle
        protected override Position[][] Tiles => tiles;
    }

    // J-Block
    public class JBlock : Block
    {
        // rotation states
        private readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new(0, 0), new(1,0), new(1,1), new(1,2) },
            new Position[] { new(0, 1), new(0,2), new(1,1), new(2,1) },
            new Position[] { new(1, 0), new(1,1), new(1,2), new(2,2) },
            new Position[] { new(0, 1), new(1,1), new(2,0), new(2,1) }
        };

        public override int Id => 2;
        protected override Position startOffset => new Position(0, 3);     // top row in the middle
        protected override Position[][] Tiles => tiles;
    }

    // L-Block
    public class LBlock : Block
    {
        // rotation states
        private readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new(0, 2), new(1,0), new(1,1), new(1,2) },
            new Position[] { new(0, 1), new(1,1), new(2,1), new(2,2) },
            new Position[] { new(1, 0), new(1,1), new(1,2), new(2,0) },
            new Position[] { new(0, 0), new(0,1), new(1,1), new(2,1) }
        };

        public override int Id => 3;
        protected override Position startOffset => new Position(0, 3);     // top row in the middle
        protected override Position[][] Tiles => tiles;
    }

    // O-Block
    public class OBlock : Block
    {
        private readonly Position[][] tiles = new Position[][]
        {
            new Position[]{ new(0,0), new(0, 1), new(1,0), new(1, 1) }
        };

        public override int Id => 4;
        protected override Position startOffset => new Position(0, 4);     // top row in the middle
        protected override Position[][] Tiles => tiles;
    }

    // S-Block
    public class SBlock : Block
    {
        // rotation states
        private readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new(0, 1), new(0,2), new(1,0), new(1,1) },
            new Position[] { new(0, 1), new(1,1), new(1,2), new(2,2) },
            new Position[] { new(1, 1), new(1,2), new(2,0), new(2,1) },
            new Position[] { new(0, 0), new(1,0), new(1,1), new(2,1) }
        };

        public override int Id => 5;
        protected override Position startOffset => new Position(0, 3);     // top row in the middle
        protected override Position[][] Tiles => tiles;
    }

    // T-Block
    public class TBlock : Block
    {
        // rotation states
        private readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new(0, 1), new(1,0), new(1,1), new(1,2) },
            new Position[] { new(0, 1), new(1,1), new(1,2), new(2,1) },
            new Position[] { new(1, 0), new(1,1), new(1,2), new(2,1) },
            new Position[] { new(0, 1), new(1,0), new(1,1), new(2,1) }
        };

        public override int Id => 6;
        protected override Position startOffset => new Position(0, 3);     // top row in the middle
        protected override Position[][] Tiles => tiles;
    }

    // Z-Block
    public class ZBlock : Block
    {
        // rotation states
        private readonly Position[][] tiles = new Position[][]
        {
            new Position[] { new(0, 0), new(0,1), new(1,1), new(1,2) },
            new Position[] { new(0, 2), new(1,1), new(1,2), new(2,1) },
            new Position[] { new(1, 0), new(1,1), new(2,1), new(2,2) },
            new Position[] { new(0, 1), new(1,0), new(1,1), new(2,0) }
        };

        public override int Id => 7;
        protected override Position startOffset => new Position(0, 3);     // top row in the middle
        protected override Position[][] Tiles => tiles;
    }

}
