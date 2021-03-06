﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Checker
{
    public class Checker : Game
    {
        const int _TILESIZE = 75;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        enum PlayerTurn
        {
            RedTurn,
            YellowTurn
        }

        PlayerTurn _currentPlayerTurn;
        List<Point> _possibleClicked;
        List<Point> _checkKing;

        //TODO: Game State Machine
        enum GameState
        {
            TurnBeginning,
            WaitingForSelection,
            ChipSelected,
            Check,
            WaitingForCheck,
            ChangePawn,
            TrunEnded,
            GameEnded
        }
        GameState _currenGameState;
        MouseState _mouseState, _previousMouseState;
        Point _clickedPos, _selectPos, _posPawn, _kingPos;

        //TODO: Image files and font
        Texture2D _king, _queen, _bishtop, _rook, _knight, _pawn, _rect, _rectText;
        SpriteFont _font;
        int[,] _gameTable;

        public Checker()
        {
            _graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 600;  // set this value to the desired width of your window
            _graphics.PreferredBackBufferHeight = 855;   // set this value to the desired height of your window
            _graphics.ApplyChanges();

            _currentPlayerTurn = PlayerTurn.RedTurn;

            _currenGameState = GameState.TurnBeginning;

            _gameTable = new int[8, 8]
            {
                { 3,5,4,2,1,4,5,3},
                { 6,6,6,6,6,6,6,6},
                { 0,0,0,0,0,0,0,0},
                { 0,0,0,0,0,0,0,0},
                { 0,0,0,0,0,0,0,0},
                { 0,0,0,0,0,0,0,0},
                { -6,-6,-6,-6,-6,-6,-6,-6},
                { -3,-5,-4,-1,-2,-4,-5,-3}
                //{ 3,5,4,0,0,4,5,3},
                //{ 6,6,6,0,0,6,6,6},
                //{ 66,0,0,0,0,22,0,0},
                //{ -66,0,0,0,0,0,0,0},
                //{ 0,0,0,0,0,0,0,0},
                //{ 0,0,0,0,0,0,0,0},
                //{ -6,0,-6,-6,-6,-6,-6,-6},
                //{ -3,0,-4,-1,-2,-4,-5,-3}
            };

            _possibleClicked = new List<Point>();
            _checkKing = new List<Point>();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _king = this.Content.Load<Texture2D>("king");
            _queen = this.Content.Load<Texture2D>("queen");
            _rook = this.Content.Load<Texture2D>("rook");
            _bishtop = this.Content.Load<Texture2D>("bishop");
            _knight = this.Content.Load<Texture2D>("knight");
            _pawn = this.Content.Load<Texture2D>("pawn");
            _font = this.Content.Load<SpriteFont>("GameText");

            //_chip = this.Content.Load<Texture2D>("Chip");
            //_horse = this.Content.Load<Texture2D>("king");
            _rect = new Texture2D(_graphics.GraphicsDevice, _TILESIZE, _TILESIZE);
            Color[] data = new Color[_TILESIZE * _TILESIZE];
            for (int i = 0; i < data.Length; i++) data[i] = Color.White;
            _rect.SetData(data);
            _rectText = new Texture2D(_graphics.GraphicsDevice, 75, 30);
            Color[] dataText = new Color[75 * 30];
            for (int i = 0; i < dataText.Length; i++) dataText[i] = Color.White;
            _rectText.SetData(dataText);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _previousMouseState = _mouseState;

            //TODO: Game logic in state machine
            switch (_currenGameState)
            {
                case GameState.TurnBeginning:
                    // Search for available moves
                    _possibleClicked.Clear();
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (_currentPlayerTurn == PlayerTurn.RedTurn)
                            {
                                // red
                                if (_gameTable[j, i] < 0)
                                {
                                    if (FindPossibleMoves(new Point(i, j)).Count > 0)
                                    {
                                        _possibleClicked.Add(new Point(i, j));
                                    }
                                }
                            }
                            else if (_currentPlayerTurn == PlayerTurn.YellowTurn)
                            {
                                // red
                                if (_gameTable[j, i] > 0)
                                {
                                    if (FindPossibleMoves(new Point(i, j)).Count > 0)
                                    {
                                        _possibleClicked.Add(new Point(i, j));
                                    }

                                }
                            }
                        }
                    }
                    if (_possibleClicked.Count == 0)
                    {
                        _currenGameState = GameState.GameEnded;
                    }
                    else
                    {
                        _currenGameState = GameState.WaitingForSelection;
                    }
                    break;
                case GameState.WaitingForSelection:
                    _mouseState = Mouse.GetState();
                    if (_mouseState.LeftButton == ButtonState.Pressed)
                    {
                        int xPos = _mouseState.X / _TILESIZE;
                        int yPos = _mouseState.Y / _TILESIZE;

                        _clickedPos = new Point(xPos, yPos);
                        if (_possibleClicked.Contains(_clickedPos))
                        {
                            _selectPos = _clickedPos;
                            _possibleClicked.Clear();

                            _possibleClicked.AddRange(FindPossibleMoves(_clickedPos));
                            _currenGameState = GameState.ChipSelected;
                        }
                    }
                    break;
                case GameState.ChipSelected:
                    _mouseState = Mouse.GetState();
                    if (_mouseState.LeftButton == ButtonState.Pressed)
                    {
                        int xPos = _mouseState.X / _TILESIZE;
                        int yPos = _mouseState.Y / _TILESIZE;

                        _clickedPos = new Point(xPos, yPos);
                        if (_possibleClicked.Contains(_clickedPos))
                        {
                            _gameTable[_clickedPos.Y, _clickedPos.X] = _gameTable[_selectPos.Y, _selectPos.X];
                            _gameTable[_selectPos.Y, _selectPos.X] = 0;
                            //_CheckPos
                            int endGameCheck = 0;

                            for (int i = 0; i < 8; i++)
                            {
                                for (int j = 0; j < 8; j++)
                                {
                                    if (_currentPlayerTurn == PlayerTurn.RedTurn)
                                    {
                                        if (_gameTable[j, i] == 1)
                                        {
                                            endGameCheck = 1;
                                        }

                                    }
                                    else if (_currentPlayerTurn == PlayerTurn.YellowTurn)
                                    {

                                        if (_gameTable[j, i] == 1)
                                        {
                                            endGameCheck = 1;
                                        }

                                    }
                                }
                            }
                            if (endGameCheck == 1)
                            {
                                if (_currentPlayerTurn == PlayerTurn.RedTurn)
                                {
                                    if (_gameTable[_clickedPos.Y, _clickedPos.X] == -66)
                                    {
                                        _gameTable[_clickedPos.Y, _clickedPos.X] = -6;
                                    }
                                    if (_gameTable[_clickedPos.Y, _clickedPos.X] == -6 && System.Math.Abs(_clickedPos.Y - _selectPos.Y) == 2)
                                    {
                                        _gameTable[_clickedPos.Y, _clickedPos.X] = -66;
                                    }
                                    if (_clickedPos.Y == 0 && System.Math.Abs(_gameTable[_clickedPos.Y, _clickedPos.X]) == 6)
                                    {
                                        _posPawn = _clickedPos;
                                        _currenGameState = GameState.ChangePawn;
                                    }
                                    else
                                    {
                                        _currenGameState = GameState.Check;
                                    }
                                }
                                else if (_currentPlayerTurn == PlayerTurn.YellowTurn)
                                {
                                    if (_gameTable[_clickedPos.Y, _clickedPos.X] == 66)
                                    {
                                        _gameTable[_clickedPos.Y, _clickedPos.X] = 6;
                                    }
                                    if (_gameTable[_clickedPos.Y, _clickedPos.X] == 6 && System.Math.Abs(_clickedPos.Y - _selectPos.Y) == 2)
                                    {
                                        _gameTable[_clickedPos.Y, _clickedPos.X] = 66;
                                    }
                                    if (_clickedPos.Y == 7 && System.Math.Abs(_gameTable[_clickedPos.Y, _clickedPos.X]) == 6)
                                    {
                                        _posPawn = _clickedPos;
                                        _currenGameState = GameState.ChangePawn;
                                    }
                                    else
                                    {
                                        _currenGameState = GameState.Check;
                                    }
                                }
                            }
                            else
                            {
                                _currenGameState = GameState.GameEnded;
                            }
                        }

                    }
                    break;
                case GameState.ChangePawn:
                    _mouseState = Mouse.GetState();
                    _possibleClicked.Clear();
                    _possibleClicked.Add(new Point(3, 9));
                    _possibleClicked.Add(new Point(4, 9));
                    _possibleClicked.Add(new Point(5, 9));
                    _possibleClicked.Add(new Point(6, 9));
                    if (_mouseState.LeftButton == ButtonState.Pressed)
                    {
                        int xPos = _mouseState.X / _TILESIZE;
                        int yPos = _mouseState.Y / _TILESIZE;

                        _clickedPos = new Point(xPos, yPos);
                        if (_possibleClicked.Contains(_clickedPos))
                        {
                            if (_clickedPos.Y == 9)
                            {
                                switch (_clickedPos.X)
                                {
                                    case 3:
                                        if (_currentPlayerTurn == PlayerTurn.RedTurn) _gameTable[_posPawn.Y, _posPawn.X] = -3;
                                        else _gameTable[_posPawn.Y, _posPawn.X] = 3;
                                        break;
                                    case 4:
                                        if (_currentPlayerTurn == PlayerTurn.RedTurn) _gameTable[_posPawn.Y, _posPawn.X] = -4;
                                        else _gameTable[_posPawn.Y, _posPawn.X] = 4;
                                        break;
                                    case 5:
                                        if (_currentPlayerTurn == PlayerTurn.RedTurn) _gameTable[_posPawn.Y, _posPawn.X] = -2;
                                        else _gameTable[_posPawn.Y, _posPawn.X] = 2;
                                        break;
                                    case 6:
                                        if (_currentPlayerTurn == PlayerTurn.RedTurn) _gameTable[_posPawn.Y, _posPawn.X] = -5;
                                        else _gameTable[_posPawn.Y, _posPawn.X] = 5;
                                        break;
                                }
                            }
                            _currenGameState = GameState.Check;
                        }
                    }
                    break;
                case GameState.Check:
                    _checkKing.Clear();
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (_currentPlayerTurn == PlayerTurn.RedTurn)
                            {
                                // red
                                if (_gameTable[j, i] < 0)
                                {
                                    if (FindPossibleMoves(new Point(i, j)).Count > 0)
                                    {
                                        _checkKing.AddRange(FindPossibleMoves(new Point(i, j)));
                                    }
                                }
                            }
                            else if (_currentPlayerTurn == PlayerTurn.YellowTurn)
                            {
                                // red
                                if (_gameTable[j, i] > 0)
                                {

                                    if (FindPossibleMoves(new Point(i, j)).Count > 0)
                                    {
                                        _checkKing.AddRange(FindPossibleMoves(new Point(i, j)));
                                    }
                                }
                            }
                        }
                    }
                    int printCheck = 0;
                    foreach (Point p in _checkKing)
                    {
                        if (System.Math.Abs(_gameTable[p.Y, p.X]) == 1)
                        {
                            _kingPos = p;
                            printCheck = 1;
                        }
                    }
                    if (printCheck == 1)
                    {
                        _currenGameState = GameState.WaitingForCheck;
                    }
                    else
                    {
                        _currenGameState = GameState.TrunEnded;
                    }
                    break;
                case GameState.WaitingForCheck:
                    _mouseState = Mouse.GetState();
                    _possibleClicked.Clear();
                    _possibleClicked.Add(new Point(_kingPos.X, _kingPos.Y));
                    _possibleClicked.Add(new Point(675 / _TILESIZE, 262 / _TILESIZE));
                    if (_mouseState.LeftButton == ButtonState.Pressed)
                    {
                        int xPos = _mouseState.X / _TILESIZE;
                        int yPos = _mouseState.Y / _TILESIZE;

                        _clickedPos = new Point(xPos, yPos);
                        if (_possibleClicked.Contains(_clickedPos))
                        {
                            _currenGameState = GameState.TrunEnded;
                            Console.WriteLine("fsvfjknvfjknvbfjknvfv");
                        }
                    }
                    break;
                case GameState.TrunEnded:
                    // swap turn
                    if (_currentPlayerTurn == PlayerTurn.RedTurn) _currentPlayerTurn = PlayerTurn.YellowTurn;
                    else if (_currentPlayerTurn == PlayerTurn.YellowTurn) _currentPlayerTurn = PlayerTurn.RedTurn;
                    _currenGameState = GameState.TurnBeginning;
                    break;
                case GameState.GameEnded:
                    break;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            //TODO: Draw board
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (((i + j) % 2) == 0)
                    {
                        _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.Tan, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                }
            }
            switch (_currenGameState)
            {
                case GameState.TurnBeginning:
                    break;
                case GameState.WaitingForSelection:
                    foreach (Point p in _possibleClicked)
                    {
                        _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * p.X, _TILESIZE * p.Y), null, Color.DarkSeaGreen, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                    break;
                case GameState.ChipSelected:
                    foreach (Point p in _possibleClicked)
                    {
                        _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * p.X, _TILESIZE * p.Y), null, Color.LightCoral, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                    break;
                case GameState.ChangePawn:
                    _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * _posPawn.X, _TILESIZE * _posPawn.Y), null, Color.Gold, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * 3, _TILESIZE * 9), null, Color.LightSkyBlue, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.Draw(_rook, new Vector2(_TILESIZE * 3, _TILESIZE * 9), null, Color.AntiqueWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * 4, _TILESIZE * 9), null, Color.LightSkyBlue, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.Draw(_bishtop, new Vector2(_TILESIZE * 4, _TILESIZE * 9), null, Color.AntiqueWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * 5, _TILESIZE * 9), null, Color.LightSkyBlue, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.Draw(_queen, new Vector2(_TILESIZE * 5, _TILESIZE * 9), null, Color.AntiqueWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * 6, _TILESIZE * 9), null, Color.LightSkyBlue, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.Draw(_knight, new Vector2(_TILESIZE * 6, _TILESIZE * 9), null, Color.AntiqueWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.DrawString(_font, "CHANGE PARW TO =>", new Vector2(40, 700), Color.Black);
                    break;
                case GameState.Check:
                    break;
                case GameState.WaitingForCheck:
                    _spriteBatch.Draw(_rect, new Vector2(_TILESIZE * _kingPos.X, _TILESIZE * _kingPos.Y), null, Color.Crimson, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.Draw(_rectText, new Vector2(262, 675), null, Color.Crimson, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    _spriteBatch.DrawString(_font, "CHECK", new Vector2(270, 678), Color.Black);
                    break;
                case GameState.TrunEnded:
                    break;
                case GameState.GameEnded:
                    if (_currentPlayerTurn == PlayerTurn.RedTurn)
                    {
                        _spriteBatch.DrawString(_font, "BLACK WIN", new Vector2(270, 678), Color.Black);
                    }
                    else if (_currentPlayerTurn == PlayerTurn.YellowTurn)
                    {
                        _spriteBatch.DrawString(_font, "WHITE WIN", new Vector2(270, 678), Color.Black);
                    }
                    break;
            }

            //TODO: draw chips
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    switch (System.Math.Abs(_gameTable[i, j]))
                    {
                        case 1:
                            if (_gameTable[i, j] < 0) _spriteBatch.Draw(_king, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.DimGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            else _spriteBatch.Draw(_king, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.AntiqueWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            break;
                        case 2:
                            if (_gameTable[i, j] < 0) _spriteBatch.Draw(_queen, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.DimGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            else _spriteBatch.Draw(_queen, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.AntiqueWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            break;
                        case 3:
                            if (_gameTable[i, j] < 0) _spriteBatch.Draw(_rook, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.DimGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            else _spriteBatch.Draw(_rook, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.AntiqueWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            break;
                        case 4:
                            if (_gameTable[i, j] < 0) _spriteBatch.Draw(_bishtop, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.DimGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            else _spriteBatch.Draw(_bishtop, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.AntiqueWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            break;
                        case 5:
                            if (_gameTable[i, j] < 0) _spriteBatch.Draw(_knight, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.DimGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            else _spriteBatch.Draw(_knight, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.AntiqueWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            break;
                        case 6:
                            if (_gameTable[i, j] < 0) _spriteBatch.Draw(_pawn, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.DimGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            else _spriteBatch.Draw(_pawn, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.AntiqueWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            break;
                        case 66:
                            if (_gameTable[i, j] < 0) _spriteBatch.Draw(_pawn, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.DimGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            else _spriteBatch.Draw(_pawn, new Vector2(_TILESIZE * j, _TILESIZE * i), null, Color.AntiqueWhite, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                            break;
                    }
                }
            }

            _spriteBatch.End();

            _graphics.BeginDraw();

            base.Draw(gameTime);
        }
        protected List<Point> FindPossibleMoves(Point cuurentTitle)
        {
            List<Point> returnVectors = new List<Point>();
            //red Normal chip
            if (_gameTable[cuurentTitle.Y, cuurentTitle.X] < 0)
            {
                switch (System.Math.Abs(_gameTable[cuurentTitle.Y, cuurentTitle.X]))
                {
                    // king
                    case 1:
                        // บน
                        if (cuurentTitle.Y - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X] == 0 || _gameTable[cuurentTitle.Y - 1, cuurentTitle.X] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, cuurentTitle.Y - 1));
                            }
                        }
                        // บนขวา
                        if (cuurentTitle.X + 1 < 8 && cuurentTitle.Y - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X + 1] == 0 || _gameTable[cuurentTitle.Y - 1, cuurentTitle.X + 1] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y - 1));
                            }
                        }
                        //// ขวา
                        if (cuurentTitle.X + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y, cuurentTitle.X + 1] == 0 || _gameTable[cuurentTitle.Y, cuurentTitle.X + 1] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y));
                            }
                        }
                        //// ล่างขวา
                        if (cuurentTitle.X + 1 < 8 && cuurentTitle.Y + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X + 1] == 0 || _gameTable[cuurentTitle.Y + 1, cuurentTitle.X + 1] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y + 1));
                            }
                        }
                        //// ล่าง
                        if (cuurentTitle.Y + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X] == 0 || _gameTable[cuurentTitle.Y + 1, cuurentTitle.X] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, cuurentTitle.Y + 1));
                            }
                        }
                        //// ล่างซ้าย
                        if (cuurentTitle.X - 1 >= 0 && cuurentTitle.Y + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X - 1] == 0 || _gameTable[cuurentTitle.Y + 1, cuurentTitle.X - 1] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y + 1));
                            }
                        }
                        //// ซ้าย
                        if (cuurentTitle.X - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y, cuurentTitle.X - 1] == 0 || _gameTable[cuurentTitle.Y, cuurentTitle.X - 1] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y));
                            }
                        }
                        //// บนซ้าย
                        if (cuurentTitle.X - 1 >= 0 && cuurentTitle.Y - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X - 1] == 0 || _gameTable[cuurentTitle.Y - 1, cuurentTitle.X - 1] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y - 1));
                            }
                        }
                        break;
                    // queen
                    case 2:
                        // บน
                        for (int i = cuurentTitle.Y - 1; i >= 0; i--)
                        {
                            if (_gameTable[i, cuurentTitle.X] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                            }
                            else if (_gameTable[i, cuurentTitle.X] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // บนขวา
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y - i >= 0 && cuurentTitle.X + i < 8)
                            {
                                if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X + i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y - i));
                                }
                                else if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X + i] > 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y - i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }
                        //// ขวา
                        for (int i = cuurentTitle.X + 1; i < 8; i++)
                        {
                            if (_gameTable[cuurentTitle.Y, i] == 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                            }
                            else if (_gameTable[cuurentTitle.Y, i] > 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // ล่างขวา
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y + i < 8 && cuurentTitle.X + i < 8)
                            {
                                if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X + i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y + i));
                                }
                                else if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X + i] > 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y + i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }
                        //// ล่าง
                        for (int i = cuurentTitle.Y + 1; i < 8; i++)
                        {
                            if (_gameTable[i, cuurentTitle.X] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                            }
                            else if (_gameTable[i, cuurentTitle.X] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // ล่างซ้าย
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y + i < 8 && cuurentTitle.X - i >= 0)
                            {
                                if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X - i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y + i));
                                }
                                else if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X - i] > 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y + i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }
                        // ซ้าย
                        for (int i = cuurentTitle.X - 1; i >= 0; i--)
                        {
                            if (_gameTable[cuurentTitle.Y, i] == 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                            }
                            else if (_gameTable[cuurentTitle.Y, i] > 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // บนซ้าย
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y - i >= 0 && cuurentTitle.X - i >= 0)
                            {
                                if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X - i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y - i));
                                }
                                else if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X - i] > 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y - i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    // rook
                    case 3:
                        // บน
                        for (int i = cuurentTitle.Y - 1; i >= 0; i--)
                        {
                            if (_gameTable[i, cuurentTitle.X] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                            }
                            else if (_gameTable[i, cuurentTitle.X] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // ขวา
                        for (int i = cuurentTitle.X + 1; i < 8; i++)
                        {
                            if (_gameTable[cuurentTitle.Y, i] == 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                            }
                            else if (_gameTable[cuurentTitle.Y, i] > 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // ล่าง
                        for (int i = cuurentTitle.Y + 1; i < 8; i++)
                        {
                            if (_gameTable[i, cuurentTitle.X] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                            }
                            else if (_gameTable[i, cuurentTitle.X] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // ซ้าย
                        for (int i = cuurentTitle.X - 1; i >= 0; i--)
                        {
                            if (_gameTable[cuurentTitle.Y, i] == 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                            }
                            else if (_gameTable[cuurentTitle.Y, i] > 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    // bishop
                    case 4:
                        // บนขวา
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y - i >= 0 && cuurentTitle.X + i < 8)
                            {
                                if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X + i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y - i));
                                }
                                else if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X + i] > 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y - i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }
                        // ล่างขวา
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y + i < 8 && cuurentTitle.X + i < 8)
                            {
                                if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X + i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y + i));
                                }
                                else if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X + i] > 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y + i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }
                        // ล่างซ้าย
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y + i < 8 && cuurentTitle.X - i >= 0)
                            {
                                if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X - i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y + i));
                                }
                                else if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X - i] > 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y + i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        // บนซ้าย
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y - i >= 0 && cuurentTitle.X - i >= 0)
                            {
                                if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X - i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y - i));
                                }
                                else if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X - i] > 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y - i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }

                        break;
                    // knight
                    case 5:
                        if (cuurentTitle.Y - 1 >= 0 && cuurentTitle.X + 2 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X + 2] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 2, cuurentTitle.Y - 1));
                            }
                            else if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X + 2] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 2, cuurentTitle.Y - 1));
                            }
                        }
                        if (cuurentTitle.Y - 2 >= 0 && cuurentTitle.X + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y - 2, cuurentTitle.X + 1] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y - 2));
                            }
                            else if (_gameTable[cuurentTitle.Y - 2, cuurentTitle.X + 1] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y - 2));
                            }
                        }
                        if (cuurentTitle.Y - 2 >= 0 && cuurentTitle.X - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y - 2, cuurentTitle.X - 1] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y - 2));
                            }
                            else if (_gameTable[cuurentTitle.Y - 2, cuurentTitle.X - 1] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y - 2));
                            }
                        }
                        if (cuurentTitle.Y - 1 >= 0 && cuurentTitle.X - 2 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X - 2] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 2, cuurentTitle.Y - 1));
                            }
                            else if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X - 2] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 2, cuurentTitle.Y - 1));
                            }
                        }
                        if (cuurentTitle.Y + 1 < 8 && cuurentTitle.X - 2 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X - 2] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 2, cuurentTitle.Y + 1));
                            }
                            else if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X - 2] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 2, cuurentTitle.Y + 1));
                            }
                        }
                        if (cuurentTitle.Y + 2 < 8 && cuurentTitle.X - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y + 2, cuurentTitle.X - 1] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y + 2));
                            }
                            else if (_gameTable[cuurentTitle.Y + 2, cuurentTitle.X - 1] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y + 2));
                            }
                        }
                        if (cuurentTitle.Y + 1 < 8 && cuurentTitle.X + 2 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X + 2] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 2, cuurentTitle.Y + 1));
                            }
                            else if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X + 2] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 2, cuurentTitle.Y + 1));
                            }
                        }
                        if (cuurentTitle.Y + 2 < 8 && cuurentTitle.X + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y + 2, cuurentTitle.X + 1] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y + 2));
                            }
                            else if (_gameTable[cuurentTitle.Y + 2, cuurentTitle.X + 1] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y + 2));
                            }
                        }
                        break;
                    // pawn
                    case 6:
                    case 66:
                        if (cuurentTitle.Y - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, cuurentTitle.Y - 1));
                            }
                        }
                        if (cuurentTitle.Y - 2 >= 0 && cuurentTitle.Y == 6)
                        {
                            if (_gameTable[cuurentTitle.Y - 2, cuurentTitle.X] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, cuurentTitle.Y - 2));
                            }
                        }
                        if (cuurentTitle.Y - 1 >= 0 && cuurentTitle.X + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X + 1] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y - 1));
                            }
                            else if (_gameTable[cuurentTitle.Y, cuurentTitle.X + 1] == 66)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y - 1));
                            }
                        }
                        if (cuurentTitle.Y - 1 >= 0 && cuurentTitle.X - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X - 1] > 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y - 1));
                            }
                            else if (_gameTable[cuurentTitle.Y, cuurentTitle.X - 1] == 66)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y - 1));
                            }
                        }

                        break;
                }
            }
            else
            {
                switch (System.Math.Abs(_gameTable[cuurentTitle.Y, cuurentTitle.X]))
                {
                    case 1:
                        // บน
                        if (cuurentTitle.Y - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X] == 0 || _gameTable[cuurentTitle.Y - 1, cuurentTitle.X] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, cuurentTitle.Y - 1));
                            }
                        }
                        // บนขวา
                        if (cuurentTitle.X + 1 < 8 && cuurentTitle.Y - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X + 1] == 0 || _gameTable[cuurentTitle.Y - 1, cuurentTitle.X + 1] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y - 1));
                            }
                        }
                        //// ขวา
                        if (cuurentTitle.X + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y, cuurentTitle.X + 1] == 0 || _gameTable[cuurentTitle.Y, cuurentTitle.X + 1] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y));
                            }
                        }
                        //// ล่างขวา
                        if (cuurentTitle.X + 1 < 8 && cuurentTitle.Y + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X + 1] == 0 || _gameTable[cuurentTitle.Y + 1, cuurentTitle.X + 1] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y + 1));
                            }
                        }
                        //// ล่าง
                        if (cuurentTitle.Y + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X] == 0 || _gameTable[cuurentTitle.Y + 1, cuurentTitle.X] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, cuurentTitle.Y + 1));
                            }
                        }
                        //// ล่างซ้าย
                        if (cuurentTitle.X - 1 >= 0 && cuurentTitle.Y + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X - 1] == 0 || _gameTable[cuurentTitle.Y + 1, cuurentTitle.X - 1] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y + 1));
                            }
                        }
                        //// ซ้าย
                        if (cuurentTitle.X - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y, cuurentTitle.X - 1] == 0 || _gameTable[cuurentTitle.Y, cuurentTitle.X - 1] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y));
                            }
                        }
                        //// บนซ้าย
                        if (cuurentTitle.X - 1 >= 0 && cuurentTitle.Y - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X - 1] == 0 || _gameTable[cuurentTitle.Y - 1, cuurentTitle.X - 1] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y - 1));
                            }
                        }
                        break;
                    case 2:
                        // บน
                        for (int i = cuurentTitle.Y - 1; i >= 0; i--)
                        {
                            if (_gameTable[i, cuurentTitle.X] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                            }
                            else if (_gameTable[i, cuurentTitle.X] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // บนขวา
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y - i >= 0 && cuurentTitle.X + i < 8)
                            {
                                if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X + i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y - i));
                                }
                                else if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X + i] < 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y - i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }
                        //// ขวา
                        for (int i = cuurentTitle.X + 1; i < 8; i++)
                        {
                            if (_gameTable[cuurentTitle.Y, i] == 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                            }
                            else if (_gameTable[cuurentTitle.Y, i] < 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // ล่างขวา
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y + i < 8 && cuurentTitle.X + i < 8)
                            {
                                if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X + i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y + i));
                                }
                                else if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X + i] < 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y + i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }
                        //// ล่าง
                        for (int i = cuurentTitle.Y + 1; i < 8; i++)
                        {
                            if (_gameTable[i, cuurentTitle.X] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                            }
                            else if (_gameTable[i, cuurentTitle.X] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // ล่างซ้าย
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y + i < 8 && cuurentTitle.X - i >= 0)
                            {
                                if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X - i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y + i));
                                }
                                else if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X - i] < 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y + i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }
                        // ซ้าย
                        for (int i = cuurentTitle.X - 1; i >= 0; i--)
                        {
                            if (_gameTable[cuurentTitle.Y, i] == 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                            }
                            else if (_gameTable[cuurentTitle.Y, i] < 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // บนซ้าย
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y - i >= 0 && cuurentTitle.X - i >= 0)
                            {
                                if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X - i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y - i));
                                }
                                else if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X - i] < 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y - i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    case 3:
                        // บน
                        for (int i = cuurentTitle.Y - 1; i >= 0; i--)
                        {
                            if (_gameTable[i, cuurentTitle.X] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                            }
                            else if (_gameTable[i, cuurentTitle.X] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // ขวา
                        for (int i = cuurentTitle.X + 1; i < 8; i++)
                        {
                            if (_gameTable[cuurentTitle.Y, i] == 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                            }
                            else if (_gameTable[cuurentTitle.Y, i] < 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // ล่าง
                        for (int i = cuurentTitle.Y + 1; i < 8; i++)
                        {
                            if (_gameTable[i, cuurentTitle.X] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                            }
                            else if (_gameTable[i, cuurentTitle.X] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, i));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // ซ้าย
                        for (int i = cuurentTitle.X - 1; i >= 0; i--)
                        {
                            if (_gameTable[cuurentTitle.Y, i] == 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                            }
                            else if (_gameTable[cuurentTitle.Y, i] < 0)
                            {
                                returnVectors.Add(new Point(i, cuurentTitle.Y));
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                        break;
                    case 4:
                        // บนขวา
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y - i >= 0 && cuurentTitle.X + i < 8)
                            {
                                if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X + i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y - i));
                                }
                                else if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X + i] < 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y - i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }
                        // ล่างขวา
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y + i < 8 && cuurentTitle.X + i < 8)
                            {
                                if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X + i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y + i));
                                }
                                else if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X + i] < 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X + i, cuurentTitle.Y + i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }
                        // ล่างซ้าย
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y + i < 8 && cuurentTitle.X - i >= 0)
                            {
                                if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X - i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y + i));
                                }
                                else if (_gameTable[cuurentTitle.Y + i, cuurentTitle.X - i] < 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y + i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                        // บนซ้าย
                        for (int i = 1; i <= 8; i++)
                        {
                            if (cuurentTitle.Y - i >= 0 && cuurentTitle.X - i >= 0)
                            {
                                if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X - i] == 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y - i));
                                }
                                else if (_gameTable[cuurentTitle.Y - i, cuurentTitle.X - i] < 0)
                                {
                                    returnVectors.Add(new Point(cuurentTitle.X - i, cuurentTitle.Y - i));
                                    break;
                                }
                                else
                                {
                                    break;
                                }

                            }
                            else
                            {
                                break;
                            }
                        }

                        break;
                    case 5:
                        if (cuurentTitle.Y - 1 >= 0 && cuurentTitle.X + 2 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X + 2] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 2, cuurentTitle.Y - 1));
                            }
                            else if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X + 2] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 2, cuurentTitle.Y - 1));
                            }
                        }
                        if (cuurentTitle.Y - 2 >= 0 && cuurentTitle.X + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y - 2, cuurentTitle.X + 1] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y - 2));
                            }
                            else if (_gameTable[cuurentTitle.Y - 2, cuurentTitle.X + 1] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y - 2));
                            }
                        }
                        if (cuurentTitle.Y - 2 >= 0 && cuurentTitle.X - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y - 2, cuurentTitle.X - 1] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y - 2));
                            }
                            else if (_gameTable[cuurentTitle.Y - 2, cuurentTitle.X - 1] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y - 2));
                            }
                        }
                        if (cuurentTitle.Y - 1 >= 0 && cuurentTitle.X - 2 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X - 2] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 2, cuurentTitle.Y - 1));
                            }
                            else if (_gameTable[cuurentTitle.Y - 1, cuurentTitle.X - 2] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 2, cuurentTitle.Y - 1));
                            }
                        }
                        if (cuurentTitle.Y + 1 < 8 && cuurentTitle.X - 2 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X - 2] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 2, cuurentTitle.Y + 1));
                            }
                            else if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X - 2] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 2, cuurentTitle.Y + 1));
                            }
                        }
                        if (cuurentTitle.Y + 2 < 8 && cuurentTitle.X - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y + 2, cuurentTitle.X - 1] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y + 2));
                            }
                            else if (_gameTable[cuurentTitle.Y + 2, cuurentTitle.X - 1] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y + 2));
                            }
                        }
                        if (cuurentTitle.Y + 1 < 8 && cuurentTitle.X + 2 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X + 2] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 2, cuurentTitle.Y + 1));
                            }
                            else if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X + 2] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 2, cuurentTitle.Y + 1));
                            }
                        }
                        if (cuurentTitle.Y + 2 < 8 && cuurentTitle.X + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y + 2, cuurentTitle.X + 1] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y + 2));
                            }
                            else if (_gameTable[cuurentTitle.Y + 2, cuurentTitle.X + 1] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y + 2));
                            }
                        }
                        break;
                    case 6:
                    case 66:
                        if (cuurentTitle.Y + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, cuurentTitle.Y + 1));
                            }
                        }
                        if (cuurentTitle.Y + 2 < 8 && cuurentTitle.Y == 1)
                        {
                            if (_gameTable[cuurentTitle.Y + 2, cuurentTitle.X] == 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X, cuurentTitle.Y + 2));
                            }
                        }
                        if (cuurentTitle.Y + 1 < 8 && cuurentTitle.X + 1 < 8)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X + 1] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y + 1));
                            }
                            else if (_gameTable[cuurentTitle.Y, cuurentTitle.X + 1] == -66)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X + 1, cuurentTitle.Y + 1));
                            }
                        }
                        if (cuurentTitle.Y + 1 < 8 && cuurentTitle.X - 1 >= 0)
                        {
                            if (_gameTable[cuurentTitle.Y + 1, cuurentTitle.X - 1] < 0)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y + 1));
                            }
                            else if (_gameTable[cuurentTitle.Y, cuurentTitle.X - 1] == -66)
                            {
                                returnVectors.Add(new Point(cuurentTitle.X - 1, cuurentTitle.Y + 1));
                            }
                        }
                        break;
                }
            }
            return returnVectors;
        }
    }
}
