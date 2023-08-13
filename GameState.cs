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
                currentBlock.Reset();

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
        public int Score { get; private set; }
        private int[] ClearedScoring = {0, 100, 300, 500, 800};    // added score by how many rows were cleared

        public GameState()
        {
            GameGrid = new GameGrid(22, 10); // tetris board is 20x10, added two extra invisible rows at the top
            BlockQueue = new BlockQueue();
            CurrentBlock = BlockQueue.GetAndUpdate();
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

            Score += ClearedScoring[GameGrid.ClearFullRows()];   // check if any rows were completed

            // get next block if game is not over
            if (IsGameOver())
            {
                GameOver = true;
            }
            else
            {
                CurrentBlock = BlockQueue.GetAndUpdate();
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

    }
}
