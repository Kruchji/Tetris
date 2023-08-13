using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace tetris
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // arrays of tile and block images in order of block IDs
        private readonly ImageSource[] tileImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/TileEmpty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileCyan.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileBlue.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileOrange.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileYellow.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileGreen.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TilePurple.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileRed.png", UriKind.Relative))
        };

        private readonly ImageSource[] blockImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/Block-Empty.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-I.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-J.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-L.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-O.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-S.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-T.png", UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-Z.png", UriKind.Relative))
        };


        private readonly Image[,] imageControls;

        // new game
        private GameState gameState = new GameState();

        public MainWindow()
        {
            InitializeComponent();
            imageControls = SetupGameCanvas(gameState.GameGrid);    // initialize game canvas
        }

        // setup image of game grid
        private Image[,] SetupGameCanvas(GameGrid grid)
        {
            Image[,] imageControls = new Image[grid.Rows, grid.Columns];    // size same as game grid
            int cellSize = 25;  // 500 px / 20 tiles

            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    // new cell image
                    Image imageControl = new Image
                    {
                        Width = cellSize,
                        Height = cellSize
                    };

                    // adjust distance from top left of canvas
                    Canvas.SetTop(imageControl, (r - 2) * cellSize + 10);    // push top 2 hidden rows up outside canvas and account for 10 extra pixels (to peek)
                    Canvas.SetLeft(imageControl, c * cellSize);

                    // add it as UI child to canvas and save to array of all cells
                    GameCanvas.Children.Add(imageControl);
                    imageControls[r, c] = imageControl;
                }
            }

            return imageControls;
        }

        // draw game grid
        private void DrawGrid(GameGrid grid)
        {
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    int id = grid[r, c];
                    imageControls[r, c].Opacity = 1;    // clears ghost opacity
                    imageControls[r,c].Source = tileImages[id];
                }
            }
        }

        // draw current block
        private void DrawBlock(Block block)
        {
            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row, p.Column].Opacity = 1; // overrides ghost opacity
                imageControls[p.Row, p.Column].Source = tileImages[block.Id];
            }
        }

        // preview next block
        private void DrawNextBlock(BlockQueue blockQueue)
        {
            Block next = blockQueue.NextBlock;
            NextImage.Source = blockImages[next.Id];
        }

        // preview held block
        private void DrawHeldBlock(Block heldBlock)
        {
            if (heldBlock == null)
            {
                HoldImage.Source = blockImages[0];
            }
            else
            {
                HoldImage.Source = blockImages[heldBlock.Id];
            }
        }

        // draw ghost of where the block would end up if hard dropped
        private void DrawGhostBlock(Block block)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row + dropDistance, p.Column].Opacity = 0.25;
                imageControls[p.Row + dropDistance, p.Column].Source = tileImages[block.Id];
            }
        }

        // draws grid, score, current and next block
        private void Draw(GameState gameState)
        {
            DrawGrid(gameState.GameGrid);

            // called before draw block so opacity is correct
            DrawGhostBlock(gameState.CurrentBlock);

            DrawBlock(gameState.CurrentBlock);
            DrawNextBlock(gameState.BlockQueue);
            DrawHeldBlock(gameState.HeldBlock);
            
            ScoreText.Text = $"Score: {gameState.Score}";
        }

        // async = waiting without blocking UI and inputs
        // draw UI and drop blocks
        private async Task GameLoop()
        {
            Draw(gameState);

            // drop by 1 every 500 ms
            while (!gameState.GameOver)
            {
                int delay = (int)(1000 * Math.Pow(0.6, gameState.DiffLevel - 1));   // set lower delay based on difficulty level (based on: https://tetris.wiki/TETR.IO#Blitz)
                await Task.Delay(delay);
                gameState.AutoMoveBlockDown();
                Draw(gameState);
            }

            // when game ends -> display game over menu and score
            GameOverMenu.Visibility = Visibility.Visible;
            FinalScoreText.Text = $"Score: {gameState.Score}";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (gameState.GameOver) return;     // if game has ended -> ignore all keys

            switch(e.Key)
            {
                case Key.Left:
                    gameState.MoveBlockLeft();
                    break;
                case Key.Right:
                    gameState.MoveBlockRight();
                    break;
                case Key.Down:
                    gameState.MoveBlockDown();
                    break;
                case Key.Up:
                    gameState.RotateBlockCW();
                    break;
                case Key.Z:
                    gameState.RotateBlockCCW();
                    break;
                case Key.C:
                    gameState.HoldBlock();
                    break;
                case Key.Space:
                    gameState.DropBlock();
                    break;
                default:
                    return;     // only redraw if player pressed a key that actually does something
            }

            Draw(gameState);
        }

        // when game canvas has loaded
        private async void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            await GameLoop();   // run game after load
        }

        // clicked on play again button
        private async void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            // create new game, hide game over overlay
            gameState = new GameState();
            GameOverMenu.Visibility = Visibility.Hidden;

            await GameLoop();   // run new game
        }
    }
}
