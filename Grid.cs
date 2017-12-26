using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameOfLife
{
    class Grid
    {

        private int SizeX;
        private int SizeY;
        private Cell[,] cells;
        private Cell[,] nextGenerationCells;
        private static Random rnd;
        private Canvas drawCanvas;
        private Ellipse[,] cellsVisuals;

        
        public Grid(Canvas c)
        {
            drawCanvas = c;
            rnd = new Random();
            SizeX = (int) (c.Width / 5);
            SizeY = (int)(c.Height / 5);
            cells = new Cell[SizeX, SizeY];
            nextGenerationCells = new Cell[SizeX, SizeY];
            cellsVisuals = new Ellipse[SizeX, SizeY];
 
            for (int i = 0; i < SizeX; i++)
                for (int j = 0; j < SizeY; j++)
                {
                    cells[i, j] = new Cell(i, j, 0, false);
                    nextGenerationCells[i, j] = new Cell(i, j, 0, false);
                }

            SetRandomPattern();
            InitCellsVisuals();
            UpdateGraphics();
            
        }


        public void Clear()
        {
            for (int i = 0; i < SizeX; i++)
                for (int j = 0; j < SizeY; j++)
                {
                    cells[i, j] = new Cell(i, j, 0, false);
                    nextGenerationCells[i, j] = new Cell(i, j, 0, false);
                    cellsVisuals[i, j].Fill = Brushes.Gray;
                }
        }


        void MouseMove(object sender, MouseEventArgs e)
        {
            var cellVisual = sender as Ellipse;
            
            int i = (int) cellVisual.Margin.Left / 5;
            int j = (int) cellVisual.Margin.Top / 5;
            

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!cells[i, j].IsAlive)
                {
                    cells[i, j].IsAlive = true;
                    cells[i, j].Age = 0;
                    cellVisual.Fill = Brushes.White;
                }
            }
        }

        public void UpdateGraphics()
        {
            for (int i = 0; i < SizeX; i++)
                for (int j = 0; j < SizeY; j++)
                    cellsVisuals[i, j].Fill = cells[i, j].IsAlive
                                                  ? (cells[i, j].Age < 2 ? Brushes.White : Brushes.DarkGray)
                                                  : Brushes.Gray;
        }

        public void InitCellsVisuals()
        {
            for (int i = 0; i < SizeX; i++)
                for (int j = 0; j < SizeY; j++)
                {
                    cellsVisuals[i, j] = new Ellipse();
                    cellsVisuals[i, j].Width = cellsVisuals[i, j].Height = 5;
                    double left = cells[i, j].PositionX;
                    double top = cells[i, j].PositionY;
                    cellsVisuals[i, j].Margin = new Thickness(left, top, 0, 0);
                    cellsVisuals[i, j].Fill = Brushes.Gray;
                    drawCanvas.Children.Add(cellsVisuals[i, j]);

                    cellsVisuals[i, j].MouseMove += MouseMove;
                    cellsVisuals[i, j].MouseLeftButtonDown += MouseMove;
                 }
            UpdateGraphics();
                    
        }
        

        public static bool GetRandomBoolean()
        {
            return rnd.NextDouble() > 0.8;
        }

        public void SetRandomPattern()
        {
            for (int i = 0; i < SizeX; i++)
                for (int j = 0; j < SizeY; j++)
                    cells[i, j].IsAlive = GetRandomBoolean();
        }
        
        public void UpdateToNextGeneration()
        {
            for (int i = 0; i < SizeX; i++)
                for (int j = 0; j < SizeY; j++)
                {
                    cells[i, j].IsAlive = nextGenerationCells[i, j].IsAlive;
                    cells[i, j].Age = nextGenerationCells[i, j].Age;
                }

            UpdateGraphics();
        }
        

        public void Update()
        {
            bool alive = false;
            int age = 0;

            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
//                    nextGenerationCells[i, j] = CalculateNextGeneration(i,j);          // UNOPTIMIZED
                    CalculateNextGeneration(i, j, ref alive, ref age);   // OPTIMIZED
                    nextGenerationCells[i, j].IsAlive = alive;  // OPTIMIZED
                    nextGenerationCells[i, j].Age = age;  // OPTIMIZED
                }
            }
            UpdateToNextGeneration();
        }

        public Cell CalculateNextGeneration(int row, int column)    // UNOPTIMIZED
        {
            bool alive;
            int count, age;

            alive = cells[row, column].IsAlive;
            age = cells[row, column].Age;
            count = CountNeighbors(row, column);

            if (alive && count < 2)
                return new Cell(row, column, 0, false);
            
            if (alive && (count == 2 || count == 3))
            {
                cells[row, column].Age++;
                return new Cell(row, column, cells[row, column].Age, true);
            }

            if (alive && count > 3)
                return new Cell(row, column, 0, false);
            
            if (!alive && count == 3)
                return new Cell(row, column, 0, true);
            
            return new Cell(row, column, 0, false);
        }

        public void CalculateNextGeneration(int row, int column, ref bool isAlive, ref int age)     // OPTIMIZED
        {
			isAlive = cells[row, column].IsAlive;
			age = cells[row, column].Age;

			int count = CountNeighbors(row, column);

			if (isAlive && count < 2)
			{
				isAlive = false;
				age = 0;
			}

			if (isAlive && (count == 2 || count == 3))
			{
				cells[row, column].Age++;
				isAlive = true;
				age = cells[row, column].Age;
			}

			if (isAlive && count > 3)
			{
				isAlive = false;
				age = 0;
			}

			if (!isAlive && count == 3)
			{
				isAlive = true;
				age = 0;
			}
		}

        public int CountNeighbors(int i, int j)
        {
			int count = 0;
			int lastXIndex = SizeX - 1;
			int lastYIndex = SizeY - 1;
			int incI = i + 1;
			int incJ = j + 1;
			int decI = i - 1;
			int decJ = j - 1;

			if (i != lastXIndex)
			{
				if (cells[incI, j].IsAlive) count++;
				if (j != lastYIndex && cells[incI, incJ].IsAlive) count++;
				if (j != 0 && cells[incI, decJ].IsAlive) count++;
			}

			if (j != lastYIndex && cells[i, incJ].IsAlive) count++;
			if (i != 0)
			{
				if (j != lastYIndex && cells[decI, incJ].IsAlive) count++;
				if (cells[decI, j].IsAlive) count++;
				if (j != 0 && cells[decI, decJ].IsAlive) count++;
			}

			if (j != 0 && cells[i, decJ].IsAlive) count++;

			return count;
		}
    }
}