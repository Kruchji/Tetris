﻿using System;
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
            Image NextField = NextImage1;
            if (gameID == 2) NextField = NextImage2;

            Block next = blockQueue.NextBlock;
            NextField.Source = blockImages[next.Id];
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

        // draws grid, score, current and next block
        private void Draw(GameState gameState, Image[,] imageControls)
        {
            DrawGrid(gameState.GameGrid, imageControls);

            // called before draw block so opacity is correct
            DrawGhostBlock(gameState.CurrentBlock, imageControls, gameState);

            DrawBlock(gameState.CurrentBlock, imageControls);
            DrawNextBlock(gameState.BlockQueue, gameState.gameID);
            DrawHeldBlock(gameState.HeldBlock, gameState.gameID);

            TextBlock TextField = ScoreText1;
            if (gameState.gameID == 2) TextField = ScoreText2;

            TextField.Text = $"Score: {gameState.Score}";
            
        }
            

        // async = waiting without blocking UI and inputs
        // draw UI and drop blocks
        private async Task GameLoop(GameState gameState, Image[,] imageControls)
        {
            Draw(gameState, imageControls);

            // drop by 1 every 500 ms
            while (!gameState.GameOver)
            {
                int delay = (int)(1000 * Math.Pow(0.6, gameState.DiffLevel - 1));   // set lower delay based on difficulty level (based on: https://tetris.wiki/TETR.IO#Blitz)
                await Task.Delay(delay);
                gameState.AutoMoveBlockDown();
                Draw(gameState, imageControls);
            }

            if (gameState1.GameOver && gameState2.GameOver)     // when both games end -> display game over menu and score
            {
                GameOverMenu.Visibility = Visibility.Visible;
                FinalScoreText1.Text = $"Left Player: {gameState1.Score}";
                
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
                        gameState.DropBlock();
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
                    case Key.Space:
                        gameState.DropBlock();
                        break;
                    default:
                        pressed = false;
                        break;
                }

                if (pressed) Draw(gameState, imageControls);      // only redraw if player pressed a key that actually does something

            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)      // TODO: change input handling to activate immediately
        {
            HandleKeyPressesGame1(gameState1, e, imageControls1);
            HandleKeyPressesGame2(gameState2, e, imageControls2);
            
        }

        // clicked on play again button
        private async void PlayAgain_Click(object sender, RoutedEventArgs e)        // TODO: change to reflect game mode
        {
            // create new game, hide game over overlay
            gameState1 = new GameState(1, true);
            gameState2 = new GameState(2, true);
            GameOverMenu.Visibility = Visibility.Hidden;

            await Task.WhenAll(GameLoop(gameState1, imageControls1), GameLoop(gameState2, imageControls2)); // run new game
        }

        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            GameOverMenu.Visibility = Visibility.Hidden;
            MainMenu.Visibility = Visibility.Visible;

            /*
            // TODO: Application.Current.MainWindow.Width = 1200;
            GameViewbox2.Visibility = Visibility.Collapsed;
            ScoreText2.Visibility = Visibility.Collapsed;
            DispHoldBlock2.Visibility = Visibility.Collapsed;
            DispNextBlock2.Visibility = Visibility.Collapsed;
            WholeGameGrid.ColumnDefinitions[3].Width = new GridLength(0, GridUnitType.Pixel);
            WholeGameGrid.ColumnDefinitions[4].Width = new GridLength(0, GridUnitType.Pixel);
            WholeGameGrid.ColumnDefinitions[5].Width = new GridLength(0, GridUnitType.Pixel);
             
             */
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();     // close game with quit button
        }

        private async void DoubleButton_Click(object sender, RoutedEventArgs e)
        {
            // create new game, hide game over overlay
            gameState1 = new GameState(1, true);
            gameState2 = new GameState(2, true);
            MainMenu.Visibility = Visibility.Hidden;

            await Task.WhenAll(GameLoop(gameState1, imageControls1), GameLoop(gameState2, imageControls2)); // run new game
        }

        private void SoloButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ComputerButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
