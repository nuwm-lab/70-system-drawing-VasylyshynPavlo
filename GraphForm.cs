using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LabWork
{
    public class GraphForm : Form
    {
        // --- Параметри функції ---
        // y = (3x + 1) / arctg(x)
        private Func<double, double> _func = x => (3 * x + 1) / Math.Atan(x);
        private double _xMin = 0.1;
        private double _xMax = 1.5;
        
        // Межі Y тепер будуть розраховані динамічно
        private double _yMin; 
        private double _yMax;

        // Відступ від країв форми для осей (використовуємо 80, як у вашому коді)
        private int _padding = 80; 

        // Шрифти та пензлі для рисування
        private Font _axisFont = new Font("Arial", 8);
        private Brush _textBrush = Brushes.Black;
        private Pen _graphPen = new Pen(Color.Red, 3);
        private Pen _axisPen = new Pen(Color.Black, 2);
        private Pen _gridPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dash };
        private Brush _pointBrush = Brushes.Blue;
        private Font _pointFont = new Font("Arial", 8, FontStyle.Bold);

        public GraphForm()
        {
            // --- 1. Спочатку розраховуємо межі Y ---
            CalculateYBounds();
            
            // --- 2. Тепер налаштовуємо форму ---
            this.Text = "Графік функції y = (3x + 1) / arctg(x) (Динамічний)";
            this.Width = 800;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            this.ResizeRedraw = true; 
            this.DoubleBuffered = true; 
        }

        /// <summary>
        /// Динамічно розраховує yMin та yMax на основі діапазону X.
        /// </summary>
        private void CalculateYBounds()
        {
            _yMin = double.MaxValue;
            _yMax = double.MinValue;
            double dx_smooth = 0.01; // Використовуємо малий крок для точності

            for (double x = _xMin; x <= _xMax; x += dx_smooth)
            {
                double y = _func(x);
                if (y < _yMin) _yMin = y;
                if (y > _yMax) _yMax = y;
            }

            // Додаємо 10% "повітря" зверху та знизу
            double margin = (_yMax - _yMin) * 0.1;
            if (margin == 0) margin = 1.0; // Захист, якщо лінія пласка

            _yMin -= margin;
            _yMax += margin;
        }

        /// <summary>
        /// Цей метод викликається щоразу, коли форму потрібно перерисувати.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;

            // Перевіряємо, чи є взагалі місце для рисування
            if (width <= 2 * _padding || height <= 2 * _padding)
            {
                // Вікно занадто мале, нічого не робимо
                return;
            }

            DrawAxesAndGrid(g, width, height);
            DrawGraphLine(g, width, height);
            DrawDataPoints(g, width, height);
        }

        /// <summary>
        /// Рисує осі X, Y, числову сітку та мітки.
        /// </summary>
        private void DrawAxesAndGrid(Graphics g, int width, int height)
        {
            // --- Осі ---
            g.DrawLine(_axisPen, _padding, height - _padding, _padding, _padding);
            g.DrawLine(_axisPen, _padding, height - _padding, width - _padding, height - _padding);

            // --- Назви осей ---
            g.DrawString("Y", _axisFont, _textBrush, _padding - 20, _padding - 20);
            g.DrawString("X", _axisFont, _textBrush, width - _padding + 5, height - _padding - 20);
            // Використовуємо ваш зсув для "0"
            g.DrawString("0", _axisFont, _textBrush, _padding - 15 - 5, height - _padding + 5);

            // --- Мітки та сітка по осі X (від 0.1 до 1.5 з кроком 0.2) ---
            for (double x = _xMin; x <= _xMax + 0.01; x += 0.2)
            {
                int px = MapX(x, width);
                g.DrawLine(_gridPen, px, height - _padding - 5, px, _padding);
                g.DrawLine(Pens.Black, px, height - _padding - 5, px, height - _padding + 5);
                g.DrawString(x.ToString("F1"), _axisFont, _textBrush, px - 10, height - _padding + 10);
            }

            // --- Динамічні мітки та сітка по осі Y ---
            // Рисуємо мітки з кроком 1.0, починаючи з першого цілого числа у видимому діапазоні
            double firstYTick = Math.Ceiling(_yMin);
            for (double y = firstYTick; y <= _yMax; y += 1.0)
            {
                int py = MapY(y, height);
                
                // Рисуємо, тільки якщо мітка потрапляє у видиму область
                if (py >= _padding && py <= height - _padding)
                {
                    g.DrawLine(_gridPen, _padding + 5, py, width - _padding, py);
                    g.DrawLine(Pens.Black, _padding - 5, py, _padding + 5, py);
                    g.DrawString(y.ToString("F0"), _axisFont, _textBrush, _padding - 30, py - 6);
                }
            }
        }
        
        /// <summary>
        /// Рисує плавну лінію графіка функції.
        /// </summary>
        private void DrawGraphLine(Graphics g, int width, int height)
        {
            List<Point> points = new List<Point>();
            double dx_smooth = 0.01; 

            for (double x = _xMin; x <= _xMax; x += dx_smooth)
            {
                double y = _func(x);
                // Додаємо точку, лише якщо вона в межах нових динамічних меж
                if (y >= _yMin && y <= _yMax)
                {
                    points.Add(new Point(MapX(x, width), MapY(y, height)));
                }
            }

            if (points.Count > 1)
            {
                g.DrawLines(_graphPen, points.ToArray());
            }
        }

        /// <summary>
        /// Рисує окремі точки на графіку згідно кроку Δx = 0.2.
        /// </summary>
        private void DrawDataPoints(Graphics g, int width, int height)
        {
            double dx_user = 0.2; // Крок з завдання

            for (double x = _xMin; x <= _xMax + 0.01; x += dx_user)
            {
                double currentX = Math.Round(x, 2); 
                double y = _func(currentX);
                
                // Рисуємо точку та підпис, лише якщо Y знаходиться у видимих межах
                if (y >= _yMin && y <= _yMax)
                {
                    int px = MapX(currentX, width);
                    int py = MapY(y, height);

                    g.FillEllipse(_pointBrush, px - 4, py - 4, 8, 8);
                    
                    string coord = $"({currentX:F1}, {y:F2})";
                    g.DrawString(coord, _pointFont, Brushes.Black, px + 10, py - 5);
                }
            }
        }

        // --- Методи трансформери ---

        /// <summary>
        /// Перетворює логічну X-координату в піксельну X-координату.
        /// </summary>
        private int MapX(double x, int clientWidth)
        {
            // --- Захист від ділення на нуль ---
            if (_xMax == _xMin)
            {
                return _padding; // Повертаємо ліву вісь
            }
            
            return (int)(_padding + (x - _xMin) / (_xMax - _xMin) * (clientWidth - 2 * _padding));
        }

        /// <summary>
        /// Перетворює логічну Y-координату в піксельну Y-координату.
        /// </summary>
        private int MapY(double y, int clientHeight)
        {
            // --- Захист від ділення на нуль ---
            if (_yMax == _yMin)
            {
                return clientHeight - _padding; // Повертаємо нижню вісь
            }

            return (int)(clientHeight - _padding - (y - _yMin) / (_yMax - _yMin) * (clientHeight - 2 * _padding));
        }
    }
}