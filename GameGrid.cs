using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tetris
{
    public class GameGrid
    {
        // representation of game board (coords are [rows, columns])
        private readonly int[,] grid;

        // number or rows and columns
        public int Rows { get; }
        public int Columns { get; }

        // indexer for easy access to the array -> indexing directly on GameGrid object
        public int this[int r, int c]
        {
            get => grid[r, c];
            set => grid[r, c] = value;
        }

        public GameGrid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            grid = new int[rows, columns];
        }
        public object Clone()
        {
            // create new grid and copy this grid content into it
            GameGrid NewGrid = new GameGrid(Rows, Columns);
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    NewGrid.grid[r, c] = grid[r, c];
                }
            }
            return NewGrid;
        }

        // checks if given row and column is inside the grid
        public bool IsInside(int r, int c)
        {
            return r >= 0 && r < Rows && c >= 0 && c < Columns;
        }

        // checks if given cell is empty (and inside the grid)
        public bool IsEmpty(int r, int c)
        {
            return IsInside(r, c) && grid[r, c] == 0;
        }

        // checks if an entire row is full (complete)
        public bool IsRowFull(int r)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (grid[r, c] == 0) return false;
            }
            return true;
        }

        // checks if row is completely empty
        public bool IsRowEmpty(int r)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (grid[r, c] != 0) return false;
            }
            return true;
        }

        // clears input row
        private void ClearRow(int r)
        {
            for (int c = 0; c < Columns; c++)
            {
                grid[r, c] = 0;
            }
        }

        // moves a row down by a certain amount of rows (that were cleared below)
        private void MoveRowDown(int r, int numRows)
        {
            for (int c = 0; c < Columns; c++)
            {
                grid[r + numRows, c] = grid[r, c];  // add numRows -> moves down ( 0,0 is top left)
                grid[r, c] = 0;
            }
        }

        // clears all full rows and moves others accordingly, returns number of cleared rows
        public int ClearFullRows()
        {
            int cleared = 0;

            for (int r = Rows - 1; r >= 0; r--)
            {
                if (IsRowFull(r))   // clear full row
                {
                    ClearRow(r);
                    cleared++;
                }
                else if (cleared > 0)   // move by number of cleared rows below
                {
                    MoveRowDown(r, cleared);
                }
            }

            return cleared;
        }

        // counts holes in one column
        private int HolesInColumn(int c)
        {
            bool firstTile = false;
            int holes = 0;
            for (int r = 0; r < Rows; r++)
            {
                if (grid[r, c] == 0 && firstTile) holes++;
                if (grid[r, c] != 0 && !firstTile) firstTile = true;
            }
            return holes;
        }

        // counts holes in the board, holes are empty tiles below non-empty cells
        public int HolesInBoard()
        {
            int holes = 0;
            for (int c = 0; c < Columns; c++)
            {
                holes += HolesInColumn(c);
            }
            return holes;
        }

        // gets height of column
        private int ColumnHeight(int c)
        {
            for (int r = 0; r < Rows; r++)
            {
                if (grid[r, c] != 0) return (Rows - r);
            }
            return 0;
        }

        // gets sum of differences between heights of neighbouring columns
        public int ColsHeightDiff()
        {
            int previousHeight = ColumnHeight(0);
            int totalDiffSum = 0;
            for (int c = 1; c < Columns; c++)
            {
                int currHeight = ColumnHeight(c);
                totalDiffSum += Math.Abs(ColumnHeight(c) - previousHeight);
                previousHeight = currHeight;
            }
            return totalDiffSum;
        }

        public int WellsCount()
        {
            int wells = 0;

            int previousHeight1 = ColumnHeight(0);
            int previousHeight2 = ColumnHeight(1);
            for (int c = 2; c < Columns; c++)
            {
                int currHeight = ColumnHeight(c);

                if ((previousHeight1 - previousHeight2) + (currHeight - previousHeight2) >= 6) wells++;     // well = hole at least 3 deep (diff 3 + 3 = 6)

                previousHeight1 = previousHeight2;
                previousHeight2 = currHeight;
            }

            return wells;
        }

        public int NumberOfFullLines()
        {
            int full = 0;
            for (int i = 0; i < Rows; i++)
            {
                if (IsRowFull(i)) full++;
            }
            return full;
        }

        public int NumberOfEmptyLines()
        {
            int empty = 0;
            for (int i = 0; i < Rows; i++)
            {
                if (IsRowEmpty(i)) empty++;
            }
            return empty;
        }

    }
}
