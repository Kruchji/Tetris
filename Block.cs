using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tetris
{
    // abstract -> derive classes for each specific block from this class
    public abstract class Block
    {
        // contains tile positions in 4 rotation variants (how tile looks in each rotation)
        protected abstract Position[][] Tiles { get; }

        // where block spawns in the grid
        protected abstract Position StartOffset { get; }

        // id to distinguish blocks
        public abstract int Id { get; }

        private int rotationState;  // how the block is rotated
        public Position offset { get; } // determines position of the block

        public Block()
        {
            offset = new Position(StartOffset.Row, StartOffset.Column); // place block on starting position
        }

        // get tiles occupied by block in its current rotation and position
        public IEnumerable<Position> TilePositions()
        {
            foreach (Position p in Tiles[rotationState])    // loop over tile positions
            {
                yield return new Position(p.Row + offset.Row, p.Column + offset.Column);    // yield = provide the next value in iteration
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

        // resets rotation and position
        public void Reset()
        {
            rotationState = 0;
            offset.Row = StartOffset.Row;
            offset.Column = StartOffset.Column;
        }
    }
}
