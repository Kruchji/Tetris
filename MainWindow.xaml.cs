using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Numerics;
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
using System.Windows.Threading;
using tetris.Previews;

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

        // music
        private MediaPlayer musicPlayer = new MediaPlayer();
        
        private enum GameMode
        {
            solo,
            twoplayer,
            computer
        }

        private GameMode CurrentGameMode;

        private readonly Image[,] imageControls1;
        private readonly Image[,] imageControls2;

        // new game
        private GameState gameState1 = new GameState(1, false);
        private GameState gameState2 = new GameState(2, false);

        public MainWindow()
        {
            InitializeComponent();
            imageControls1 = SetupGameCanvas(gameState1.GameGrid, gameState1.gameID);    // initialize game canvas
            imageControls2 = SetupGameCanvas(gameState2.GameGrid, gameState2.gameID);
        }

        // setup image of game grid
        private Image[,] SetupGameCanvas(GameGrid grid, int gameID)
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
                    Canvas CanvasField = GameCanvas1;
                    if (gameID == 2) CanvasField = GameCanvas2;

                    CanvasField.Children.Add(imageControl);
                    imageControls[r, c] = imageControl;
                }
            }

            return imageControls;
        }

        // draw game grid
        private void DrawGrid(GameGrid grid, Image[,] imageControls)
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
        private void DrawBlock(Block block, Image[,] imageControls)
        {
            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row, p.Column].Opacity = 1; // overrides ghost opacity
                imageControls[p.Row, p.Column].Source = tileImages[block.Id];
            }
        }

        // preview next block
        private void DrawNextBlock(BlockQueue blockQueue, int gameID)
        {
            if (gameID == 1) 
            {
                NextImage1Game1.Source = blockImages[blockQueue.NextBlocks[0].Id];
                NextImage2Game1.Source = blockImages[blockQueue.NextBlocks[1].Id];
                NextImage3Game1.Source = blockImages[blockQueue.NextBlocks[2].Id];
            }
            else
            {
                NextImage1Game2.Source = blockImages[blockQueue.NextBlocks[0].Id];
                NextImage2Game2.Source = blockImages[blockQueue.NextBlocks[1].Id];
                NextImage3Game2.Source = blockImages[blockQueue.NextBlocks[2].Id];
            }

        }

        // preview held block
        private void DrawHeldBlock(Block heldBlock, int gameID)
        {
            Image HoldField = HoldImage1;
            if (gameID == 2) HoldField = HoldImage2;

            if (heldBlock == null)
            {
                HoldField.Source = blockImages[0];
            }
            else
            {
                HoldField.Source = blockImages[heldBlock.Id];
            }
        }

        // draw ghost of where the block would end up if hard dropped
        private void DrawGhostBlock(Block block, Image[,] imageControls, GameState gameState)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row + dropDistance, p.Column].Opacity = 0.25;
                imageControls[p.Row + dropDistance, p.Column].Source = tileImages[block.Id];
            }
        }

        // get various statistics from gameState and display them on screen
        private void DrawTexts(GameState gameState)
        {
            if (gameState.gameID == 1)
            {
                ScoreText1.Text = $"Score: {gameState.Score}";
                LevelText1.Text = $"Level: {gameState.DiffLevel}";
                ComboText1.Text = $"Combo: {gameState.Combo + 1}"; ;
                LinesText1.Text = $"Lines: {gameState.TotalRowsCleared}";
            }
            else
            {
                ScoreText2.Text = $"Score: {gameState.Score}";
                LevelText2.Text = $"Level: {gameState.DiffLevel}";
                ComboText2.Text = $"Combo: {gameState.Combo + 1}"; ;
                LinesText2.Text = $"Lines: {gameState.TotalRowsCleared}";
            }
        }

        // draws grid, score, current and next block
        private void Draw(GameState gameState, Image[,] imageControls)
        {
            DrawGrid(gameState.GameGrid, imageControls);

            // called before draw block so opacity is correct
            DrawGhostBlock(gameState.CurrentBlock, imageControls, gameState);

            DrawBlock(gameState.CurrentBlock, imageControls);
            DrawNextBlock(gameState.BlockQueue, gameState.gameID);
            DrawHeldBlock(gameState.HeldBlock, gameState.gameID);

            DrawTexts(gameState);

        }

        private void Media_Ended(object sender, EventArgs e)
        {
            musicPlayer.Position = TimeSpan.Zero;
            musicPlayer.Play();
        }

        // async = waiting without blocking UI and inputs
        // draw UI and drop blocks
        private async Task GameLoop(GameState gameState, Image[,] imageControls)
        {
            Draw(gameState, imageControls);
            if (gameState.gameID == 1)
            {
                musicPlayer.Open(new Uri("../../../Assets/Theme.wav", UriKind.Relative));
                musicPlayer.MediaEnded += new EventHandler(Media_Ended);
                musicPlayer.Play();
            }

            await Task.Delay(500);
            // drop by 1 automatically
            while (!gameState.GameOver)
            {
                int delay = (int)(500 * Math.Pow(0.6, gameState.DiffLevel - 1));   // set lower delay based on difficulty level (based on: https://tetris.wiki/TETR.IO#Blitz)
                await Task.Delay(delay);
                gameState.AutoMoveBlockDown();
                Draw(gameState, imageControls);
                await Task.Delay(delay);        // divide delay so computer moves between movement in loop

                // move computer player
                if (CurrentGameMode == GameMode.computer && !gameState2.GameOver && gameState == gameState2)
                { 
                    gameState.MoveComputer();
                    Draw(gameState, imageControls);
                }

            }

            if (gameState1.GameOver && gameState2.GameOver)     // when both games end -> display game over menu and score
            {
                musicPlayer.Stop();
                GameOverMenu.Visibility = Visibility.Visible;

                // dont display two scores in solo
                if (CurrentGameMode != GameMode.solo)
                {
                    FinalScoreText1.Text = $"Left Player: {gameState1.Score}";
                    FinalScoreText1.FontSize = 25;
                    FinalScoreText2.Visibility = Visibility.Visible;
                    FinalScoreText2.Text = $"Right Player: {gameState2.Score}";

                    if (gameState1.Score > gameState2.Score)
                    {
                        FinalScoreText1.Foreground = Brushes.LawnGreen;
                        FinalScoreText2.Foreground = Brushes.Red;
                    }
                    else
                    {
                        FinalScoreText1.Foreground = Brushes.Red;
                        FinalScoreText2.Foreground = Brushes.LawnGreen;
                    }
                }
                else
                {
                    FinalScoreText1.Text = $"{gameState1.Score}";
                    FinalScoreText1.Foreground = Brushes.Gold;
                    FinalScoreText1.FontSize = 35;

                    // hide second player score
                    FinalScoreText2.Visibility = Visibility.Collapsed;

                    // check if score is higher than any leaderboard score and add it
                    var allLines = File.ReadAllLines(@"../../../HighScores.txt");
                    string[] newLines = new string[allLines.Length];
                    bool added = false;
                    for (int i = 0; i < 5; i++)
                    {
                        if (!added && int.Parse(allLines[i].Split(' ')[0]) < gameState1.Score)
                        {
                            var dialog = new UserInput();
                            if (dialog.ShowDialog() == true)    // returns when second window is closed
                            {
                                newLines[i] = $"{gameState1.Score} {dialog.ResponseText}";
                            }
                            else
                            {
                                // user closed input window -> register score as anonymous
                                newLines[i] = $"{gameState1.Score} Anonymous";
                            }
                            
                            added = true;
                        }
                        else
                        {
                            if (added)
                            {
                                newLines[i] = allLines[i-1];
                            }
                            else
                            {
                                newLines[i] = allLines[i];
                            }
                            
                        }
                    }
                    File.WriteAllLines(@"../../../HighScores.txt", newLines);
                }
            }
        }

        private void HandleKeyPressesGame2(GameState gameState, KeyEventArgs e, Image[,] imageControls)
        {
            if (!gameState.GameOver) // if game has ended -> ignore all keys
            {
                bool pressed = true;
                switch (e.Key)
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
                    case Key.NumPad2:
                        gameState.RotateBlockCW();
                        break;
                    case Key.NumPad1:
                        gameState.RotateBlockCCW();
                        break;
                    case Key.NumPad3:
                        gameState.HoldBlock();
                        break;
                    case Key.Up:
                        gameState.DropBlock(gameState.GameGrid);
                        break;
                    default:
                        pressed = false;
                        break;
                }

                if (pressed) Draw(gameState, imageControls);      // only redraw if player pressed a key that actually does something
            }
        }

        private void HandleKeyPressesGame1(GameState gameState, KeyEventArgs e, Image[,] imageControls)
        {
            if (!gameState.GameOver) // if game has ended -> ignore all keys
            {
                bool pressed = true;
                switch (e.Key)
                {
                    case Key.A:
                        gameState.MoveBlockLeft();
                        break;
                    case Key.D:
                        gameState.MoveBlockRight();
                        break;
                    case Key.S:
                        gameState.MoveBlockDown();
                        break;
                    case Key.J:
                        gameState.RotateBlockCW();
                        break;
                    case Key.H:
                        gameState.RotateBlockCCW();
                        break;
                    case Key.K:
                        gameState.HoldBlock();
                        break;
                    case Key.W:
                    case Key.Space:
                        gameState.DropBlock(gameState.GameGrid);
                        break;
                    default:
                        pressed = false;
                        break;
                }

                if (pressed) Draw(gameState, imageControls);      // only redraw if player pressed a key that actually does something

            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyPressesGame1(gameState1, e, imageControls1);
            if (CurrentGameMode == GameMode.twoplayer) HandleKeyPressesGame2(gameState2, e, imageControls2);

        }

        // clicked on play again button
        private async void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            // create new game, hide game over overlay
            GameOverMenu.Visibility = Visibility.Hidden;

            switch (CurrentGameMode)
            {
                case GameMode.solo:
                    gameState1 = new GameState(1, true);

                    await GameLoop(gameState1, imageControls1);
                    break;
                case GameMode.twoplayer:
                    gameState1 = new GameState(1, true);
                    gameState2 = new GameState(2, true);

                    await Task.WhenAll(GameLoop(gameState1, imageControls1), GameLoop(gameState2, imageControls2)); // run new game
                    break;
                case GameMode.computer:
                    gameState1 = new GameState(1, true);
                    gameState2 = new GameState(2, true);

                    await Task.WhenAll(GameLoop(gameState1, imageControls1), GameLoop(gameState2, imageControls2));
                    break;
            }

        }

        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            GameOverMenu.Visibility = Visibility.Hidden;
            Leaderboards.Visibility = Visibility.Hidden;
            MainMenu.Visibility = Visibility.Visible;

            Application.Current.MainWindow.MinWidth = 800;
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();     // close game with quit button
        }

        private void RestoreDoubleBoards()
        {
            WholeGameGrid.ColumnDefinitions[3].Width = new GridLength(1, GridUnitType.Star);
            WholeGameGrid.ColumnDefinitions[4].Width = GridLength.Auto;
            WholeGameGrid.ColumnDefinitions[5].Width = new GridLength(1, GridUnitType.Star);
            Application.Current.MainWindow.MinWidth = 1200;
            if (Application.Current.MainWindow.Width < 1200) Application.Current.MainWindow.Width = 1200;
        }

        private async void DoubleButton_Click(object sender, RoutedEventArgs e)
        {
            // create new game, hide game over overlay
            gameState1 = new GameState(1, true);
            gameState2 = new GameState(2, true);
            MainMenu.Visibility = Visibility.Hidden;
            CurrentGameMode = GameMode.twoplayer;

            RestoreDoubleBoards();

            await Task.WhenAll(GameLoop(gameState1, imageControls1), GameLoop(gameState2, imageControls2)); // run new game
        }

        private async void SoloButton_Click(object sender, RoutedEventArgs e)
        {
            // create new game, hide game over overlay
            gameState1 = new GameState(1, true);
            MainMenu.Visibility = Visibility.Hidden;
            CurrentGameMode = GameMode.solo;

            WholeGameGrid.ColumnDefinitions[3].Width = new GridLength(0, GridUnitType.Pixel);
            WholeGameGrid.ColumnDefinitions[4].Width = new GridLength(0, GridUnitType.Pixel);
            WholeGameGrid.ColumnDefinitions[5].Width = new GridLength(0, GridUnitType.Pixel);
            Application.Current.MainWindow.MinWidth = 800;

            await GameLoop(gameState1, imageControls1);

        }

        private async void ComputerButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentGameMode = GameMode.computer;
            gameState1 = new GameState(1, true);
            gameState2 = new GameState(2, true);
            MainMenu.Visibility = Visibility.Hidden;

            RestoreDoubleBoards();

            await Task.WhenAll(GameLoop(gameState1, imageControls1), GameLoop(gameState2, imageControls2));
        }

        private void LeaderboardsButton_Click(object sender, RoutedEventArgs e)
        {
            Leaderboards.Visibility = Visibility.Visible;
            MainMenu.Visibility = Visibility.Hidden;

            var allHighscores = File.ReadAllLines(@"../../../HighScores.txt");
            LeadPos1.Text = $"{allHighscores[0].Substring(allHighscores[0].Split(' ')[0].Length)}:  {allHighscores[0].Split(' ')[0]}";
            LeadPos2.Text = $"{allHighscores[1].Substring(allHighscores[1].Split(' ')[0].Length)}:  {allHighscores[1].Split(' ')[0]}";
            LeadPos3.Text = $"{allHighscores[2].Substring(allHighscores[2].Split(' ')[0].Length)}:  {allHighscores[2].Split(' ')[0]}";
            LeadPos4.Text = $"{allHighscores[3].Substring(allHighscores[3].Split(' ')[0].Length)}:  {allHighscores[3].Split(' ')[0]}";
            LeadPos5.Text = $"{allHighscores[4].Substring(allHighscores[4].Split(' ')[0].Length)}:  {allHighscores[4].Split(' ')[0]}";

        }
    }
}
