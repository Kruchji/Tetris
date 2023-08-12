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

    }
}
