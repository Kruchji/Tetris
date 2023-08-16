using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace tetris
{
    public class GameState
    {
        private Block currentBlock;

        public Block CurrentBlock           // TODO: change name (from capital C)
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

        // scoring, based on: https://tetris.wiki/Scoring
        public int Score { get; private set; } = 0;
        private readonly int[] ClearedScoring = {0, 100, 300, 500, 800};    // added score by how many rows were cleared
        private readonly int[] TSpinScoring = { 400, 800, 1200, 1600 };
        public int Combo { get; private set; } = -1;
        bool LastOperationRotation = false; // for detecting T-Spins

        // difficulty level
        public int DiffLevel { get; private set; } = 1;
        public int TotalRowsCleared { get; private set; } = 0;
        private int NextClearedGoal = 3;    // number of lines to clear for next level

        // block holding
        public Block HeldBlock { get; private set; }
        public bool CanHold { get; private set; }


        // ID of game
        public int gameID { get; private set; }

        // sound effects
        private SoundPlayer musicPlayer = new SoundPlayer(Properties.Resources.Clack);

        public GameState(int gameID, bool started)
        {
            GameGrid = new GameGrid(22, 10); // tetris board is 20x10, added two extra invisible rows at the top
            BlockQueue = new BlockQueue();
            CurrentBlock = BlockQueue.GetAndUpdate();
            CanHold = true;     // held block is empty at start
            this.gameID = gameID;
            GameOver = !started;    // set game over to true on not started game
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

            LastOperationRotation = false;
            CanHold = false;    // prevents spamming hold
        }

        // rotates the block if it is possible
        public void RotateBlockCW()
        {
            CurrentBlock.RotateCW();

            if (!BlockFits())
            {
                CurrentBlock.Move(0, 1);
                if (!BlockFits())
                {
                    CurrentBlock.Move(0, -2);
                    if (!BlockFits())
                    {
                        // special case for I Block -> try moving 2 tiles
                        if (CurrentBlock.Id == 1)
                        {
                            CurrentBlock.Move(0, -1);
                            if (!BlockFits())
                            {
                                CurrentBlock.Move(0, 4);
                                if (!BlockFits())
                                {
                                    // cant rotate even when shifted to side -> revert back
                                    CurrentBlock.Move(0, -2);
                                    CurrentBlock.RotateCCW();
                                    LastOperationRotation = false;
                                }
                            }
                        }
                        else
                        {
                            // cant rotate even when shifted to side -> revert back
                            CurrentBlock.Move(0, 1);
                            CurrentBlock.RotateCCW();
                            LastOperationRotation = false;
                        }

                    }
                }
            }
        }

        public void RotateBlockCCW()
        {
            CurrentBlock.RotateCCW();
            LastOperationRotation = true;

            if (!BlockFits())
            {
                CurrentBlock.Move(0, 1);
                if (!BlockFits())
                {
                    CurrentBlock.Move(0, -2);
                    if (!BlockFits())
                    {
                        // special case for I Block -> try moving 2 tiles
                        if (CurrentBlock.Id == 1)
                        {
                            CurrentBlock.Move(0, -1);
                            if (!BlockFits())
                            {
                                CurrentBlock.Move(0, 4);
                                if (!BlockFits())
                                {
                                    // cant rotate even when shifted to side -> revert back
                                    CurrentBlock.Move(0, -2);
                                    CurrentBlock.RotateCW();
                                    LastOperationRotation = false;
                                }
                            }
                        }
                        else
                        {
                            // cant rotate even when shifted to side -> revert back
                            CurrentBlock.Move(0, 1);
                            CurrentBlock.RotateCW();
                            LastOperationRotation = false;
                        }
                        
                    }
                } 
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
            else
            {
                LastOperationRotation = false;
            }
        }

        public void MoveBlockRight()
        {
            CurrentBlock.Move(0, 1);

            if (!BlockFits())
            {
                CurrentBlock.Move(0, -1);
            }
            else
            {
                LastOperationRotation = false;
            }
        }

        // checks if the game is over by checking if a block is offscreen at the top
        private bool IsGameOver()
        {
            return !(GameGrid.IsRowEmpty(0) && GameGrid.IsRowEmpty(1));     // top two offscreen rows
        }

        // called when block cannot be moved down
        private void PlaceBlock(GameGrid grid)
        {
            // add block to grid
            foreach (Position p in CurrentBlock.TilePositions())
            {
                grid[p.Row, p.Column] = CurrentBlock.Id;
            }

            int rowsCleared = grid.ClearFullRows();

            // update difficulty level
            TotalRowsCleared += rowsCleared;
            if (TotalRowsCleared >= NextClearedGoal)
            {
                DiffLevel++;
                NextClearedGoal += 1 + DiffLevel * 2;   // each difficulty requires 2 more line clears then previous one
            }

            // T-Spin scoring
            if (CurrentBlock.Id == 6 && LastOperationRotation)   // is a T-Block and was just rotated
            {
                int fullEdgeTiles = 0;
                if (grid.IsInside(CurrentBlock.offset.Row, CurrentBlock.offset.Column)) 
                {
                    if (grid[CurrentBlock.offset.Row, CurrentBlock.offset.Column] != 0) fullEdgeTiles++;
                }
                if (grid.IsInside(CurrentBlock.offset.Row + 2, CurrentBlock.offset.Column))
                {
                    if (grid[CurrentBlock.offset.Row + 2, CurrentBlock.offset.Column] != 0) fullEdgeTiles++;
                }
                if (grid.IsInside(CurrentBlock.offset.Row, CurrentBlock.offset.Column + 2))
                {
                    if (grid[CurrentBlock.offset.Row, CurrentBlock.offset.Column + 2] != 0) fullEdgeTiles++;
                }
                if (grid.IsInside(CurrentBlock.offset.Row + 2, CurrentBlock.offset.Column + 2))
                {
                    if (grid[CurrentBlock.offset.Row + 2, CurrentBlock.offset.Column + 2] != 0) fullEdgeTiles++;
                }

                // if at least 3 edge tiles are non empty -> is T-Spin
                if (fullEdgeTiles >= 3)
                {
                    Score += (TSpinScoring[rowsCleared] * DiffLevel) - (ClearedScoring[rowsCleared] * DiffLevel);
                }
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
                LastOperationRotation = false;
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
                PlaceBlock(GameGrid);
                return false;
            }

            LastOperationRotation = false;
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
        public void DropBlock(GameGrid grid)
        {
            int dropDistance = BlockDropDistance();
            Score += 2 * dropDistance;   // drop score is 2 points per cell dropped
            CurrentBlock.Move(dropDistance, 0);
            PlaceBlock(grid);

            musicPlayer.Play();
            LastOperationRotation = false;
        }

        private void TestPlaceBlock(GameGrid grid)
        {
            // add block to grid
            foreach (Position p in CurrentBlock.TilePositions())
            {
                grid[p.Row, p.Column] = CurrentBlock.Id;
            }
        }

        public int TestDropBlock(GameGrid grid)
        {
            int dropDistance = BlockDropDistance();
            CurrentBlock.Move(dropDistance, 0);         // TODO: change current block handling for tests
            TestPlaceBlock(grid);
            return dropDistance;
        }

        private (int, int, int) FindBestMove()
        {
            (int maxRating, int maxPos, int maxRot) DropValues = (int.MinValue, 0, 0);

            // go through all rotations
            for (int n = 0; n < 4; n++)
            {
                // start always to the right
                for (int i = 0; i < GameGrid.Rows; i++) MoveBlockRight();
                // go through each position and calculate it's score
                for (int i = 0; i < GameGrid.Rows; i++)
                {
                    int dropDistance = BlockDropDistance();             // how low block falls, bigger is better

                    // clone grid and try to drop block
                    GameGrid TestGrid = (GameGrid)GameGrid.Clone();
                    int droppedDistance = TestDropBlock(TestGrid);

                    int emptyLines = TestGrid.NumberOfEmptyLines();      // number of lines of empty space, bigger is better
                    int clearedLines = TestGrid.NumberOfFullLines();     // number of lines that would be cleared, bigger is better

                    int holeCount = TestGrid.HolesInBoard();        // how many empty tiles below non-empty tile, smaller is better
                    int totalHeightDiff = TestGrid.ColsHeightDiff();    // sum of differences in height of neighbouring columns, smaller is better
                    int wellCount = TestGrid.WellsCount();


                    // TODO: change ratio of rating variables
                    int moveRating = emptyLines + clearedLines * 1000 + dropDistance - (holeCount * 3 + totalHeightDiff * 3 + wellCount * 20);    // calculate rating of move from these stats
                    

                    if (DropValues.maxRating < moveRating)
                    {
                        DropValues.maxRating = moveRating;
                        DropValues.maxRot = n;
                        DropValues.maxPos = i;
                    }

                    foreach (Position p in CurrentBlock.TilePositions())
                    {
                        TestGrid[p.Row, p.Column] = 0;
                    }
                    CurrentBlock.Move(-droppedDistance, 0);

                    MoveBlockLeft();
                }
                RotateBlockCW();
            }

            return DropValues;
        }

        // track if computer pressed Hold on last move so it can't press it again (and stall the game)
        // theoretically not required but here it fixes some edge cases
        // example: both ratings are so close that after swap the position is different1 by 1 or 2 tiles and it tries to swap again
        private bool lastMoveHold = false;

        // finds next computer move and executes it
        public void MoveComputer()
        {
            bool holdPress = false;
            (int maxRating, int maxPos, int maxRot) CurrMax = FindBestMove();


            // try also next block or held block (and apply or revert change)
            if (HeldBlock == null)
            {
                Block tmp = currentBlock;
                currentBlock = BlockQueue.NextBlocks[0];

                (int maxRating, int maxPos, int maxRot) PotentialMax = FindBestMove();
                if (PotentialMax.maxRating > CurrMax.maxRating) holdPress = true;

                currentBlock = tmp;
            }
            else
            {
                Block tmp = currentBlock;
                currentBlock = HeldBlock;

                (int maxRating, int maxPos, int maxRot) PotentialMax = FindBestMove();
                if (PotentialMax.maxRating > CurrMax.maxRating) holdPress = true;

                currentBlock = tmp;
            }

            
            if (holdPress && !lastMoveHold)
            {
                HoldBlock();
                lastMoveHold = true;
            }
            else
            {
                for (int n = 0; n < CurrMax.maxRot; n++) RotateBlockCW();
                for (int i = 0; i < GameGrid.Rows; i++) MoveBlockRight();
                for (int i = 0; i < CurrMax.maxPos; i++) MoveBlockLeft();

                DropBlock(GameGrid);
                lastMoveHold = false;
            }
            
        }

    }
}
