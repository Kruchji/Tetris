using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tetris
{
    public class GameState
    {
        private Block currentBlock;

        public Block CurrentBlock           // TODO: change name (capital C)
        {
            get => currentBlock;

            // when updating current block -> the block is reset to starting position
            private set
            {
                currentBlock = value;
                currentBlock.Reset();   // when set in any way (next or hold), use starting position instead of last

                // try to move block automatically to visible area if possible
                for (int i = 0; i < 2; i++)
                {
                    currentBlock.Move(1, 0);

                    if (!BlockFits())
                    {
                        currentBlock.Move(-1, 0);
                    }
                }
            }
        }

        public GameGrid GameGrid { get; }
        public BlockQueue BlockQueue { get; }
        public bool GameOver { get; private set; }

        // scoring
        public int Score { get; private set; } = 0;
        private readonly int[] ClearedScoring = {0, 100, 300, 500, 800};    // added score by how many rows were cleared
        public int Combo { get; private set; } = -1;  // TODO: display the current combo on screen

        // difficulty level
        public int DiffLevel { get; private set; } = 1;
        public int TotalRowsCleared { get; private set; } = 0;  // TODO: display number of cleared rows
        private int NextClearedGoal = 3;    // number of lines to clear for next level

        // block holding
        public Block HeldBlock { get; private set; }
        public bool CanHold { get; private set; }

        public GameState()
        {
            GameGrid = new GameGrid(22, 10); // tetris board is 20x10, added two extra invisible rows at the top
            BlockQueue = new BlockQueue();
            CurrentBlock = BlockQueue.GetAndUpdate();
            CanHold = true;     // held block is empty at start
        }

        // checks if the current block is in a legal position (empty space inside board)
        private bool BlockFits()
        {
            foreach (Position p in CurrentBlock.TilePositions())
            {
                if (!GameGrid.IsEmpty(p.Row, p.Column)) return false;   // IsEmpty also checks if it is inside
            }

            return true;
        }

        public void HoldBlock()
        {
            if (!CanHold) return;

            if (HeldBlock == null)  // not holding anything
            {
                HeldBlock = CurrentBlock;
                CurrentBlock = BlockQueue.GetAndUpdate();
            }
            else
            {
                // swap held and current block
                (CurrentBlock, HeldBlock) = (HeldBlock, CurrentBlock);
            }

            CanHold = false;    // prevents spamming hold
        }

        // rotates the block if it is possible
        public void RotateBlockCW()
        {
            CurrentBlock.RotateCW();

            if (!BlockFits())
            {
                CurrentBlock.RotateCCW();
            }
        }

        public void RotateBlockCCW()
        {
            CurrentBlock.RotateCCW();

            if (!BlockFits())
            {
                CurrentBlock.RotateCW();
            }
        }

        // moves left and right if it is possible
        public void MoveBlockLeft()
        {
            CurrentBlock.Move(0, -1);

            if (!BlockFits())
            {
                CurrentBlock.Move(0, 1);
            }
        }

        public void MoveBlockRight()
        {
            CurrentBlock.Move(0, 1);

            if (!BlockFits())
            {
                CurrentBlock.Move(0, -1);
            }
        }

        // checks if the game is over by checking if a block is offscreen at the top
        private bool IsGameOver()
        {
            return !(GameGrid.IsRowEmpty(0) && GameGrid.IsRowEmpty(1));     // top two offscreen rows
        }

        // called when block cannot be moved down
        private void PlaceBlock()
        {
            // add block to grid
            foreach (Position p in CurrentBlock.TilePositions())
            {
                GameGrid[p.Row, p.Column] = CurrentBlock.Id;
            }

            int rowsCleared = GameGrid.ClearFullRows();

            // update difficulty level
            TotalRowsCleared += rowsCleared;
            if (TotalRowsCleared >= NextClearedGoal)
            {
                DiffLevel++;
                NextClearedGoal += 1 + DiffLevel * 2;   // each difficulty requires 2 more line clears then previous one
            }

            Score += ClearedScoring[rowsCleared] * DiffLevel;   // add score according to the cleared rows
            
            // update combo
            if (rowsCleared > 0)
            {
                Combo++;
                Score += 50 * Combo * DiffLevel;    // 50 * combo counter is the number of points awarded on line clear -> that's why combo starts at -1
            }
            else
            {
                Combo = -1; // reset combo when no line was cleared
            }

            // get next block if game is not over
            if (IsGameOver())
            {
                GameOver = true;
            }
            else
            {
                CurrentBlock = BlockQueue.GetAndUpdate();
                CanHold = true;     // enable pressing hold again
            }
        }

        // moves block down by one if possible
        public bool AutoMoveBlockDown()
        {
            CurrentBlock.Move(1, 0);

            // if block cannot be moved down -> place it
            if (!BlockFits())
            {
                CurrentBlock.Move(-1, 0);
                PlaceBlock();
                return false;
            }
            return true;
        }

        // moves block down by one if possible and adds score (soft dropping)
        public void MoveBlockDown()
        {
            if (AutoMoveBlockDown()) Score++;    // Soft drop -> 1 point per cell
        }

        // takes position and returns number of empty cells immediately below it
        private int TileDropDistance(Position p)
        {
            int drop = 0;

            while (GameGrid.IsEmpty(p.Row + drop + 1, p.Column)) drop++;

            return drop;
        }

        // get drop distance of whole block (by taking minimum of all it's tiles)
        public int BlockDropDistance()
        {
            int drop = GameGrid.Rows;

            foreach (Position p in CurrentBlock.TilePositions())
            {
                drop = System.Math.Min(drop, TileDropDistance(p));
            }

            return drop;
        }

        // move current block down as many rows as possible and places it
        public void DropBlock()
        {
            int dropDistance = BlockDropDistance();
            Score += 2 * dropDistance;   // drop score is 2 points per cell dropped
            CurrentBlock.Move(dropDistance, 0);
            PlaceBlock();
        }

    }
}
