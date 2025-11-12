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
        private Func<double, double> func = x => (3 * x + 1) / Math.Atan(x);
        private double xMin = 0.1;
        private double xMax = 1.5;
        
        // Розраховані вручну мінімум та максимум функції на цьому діапазоні
        // y(0.1) ≈ 13.05
        // y(1.5) ≈ 5.59
        // Візьмемо діапазон з невеликим запасом.
        private double yMin = 5.0; 
        private double yMax = 14.0;

        // Відступ від країв форми для осей
        private int padding = 80; 

        // Шрифти та пензлі для рисування
        private Font axisFont = new Font("Arial", 8);
        private Brush textBrush = Brushes.Black;
        private Pen graphPen = new Pen(Color.Red, 3);
        private Pen axisPen = new Pen(Color.Black, 2);
        private Pen gridPen = new Pen(Color.LightGray, 1) { DashStyle = DashStyle.Dash };
        private Brush pointBrush = Brushes.Blue;

        public GraphForm()
        {
            this.Text = "Графік функції y = (3x + 1) / arctg(x)";
            this.Width = 800;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // --- Ключові властивості ---
            
            // 1. Вказує формі, що її потрібно повністю перерисовувати
            //    при кожній зміні розміру.
            this.ResizeRedraw = true; 
            
            // 2. Використовує подвійну буферизацію, щоб уникнути
            //    мерехтіння графіка під час перерисування.
            this.DoubleBuffered = true; 
        }

        /// <summary>
        /// Цей метод викликається щоразу, коли форму потрібно перерисувати
        /// (наприклад, при запуску, зміні розміру, відновленні).
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            
            // Вмикаємо згладжування для гарного вигляду ліній
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Отримуємо *поточні* розміри клієнтської області вікна.
            // Це гарантує, що графік завжди масштабується до поточного розміру.
            int width = this.ClientSize.Width;
            int height = this.ClientSize.Height;

            // --- 1. Рисуємо осі та сітку ---
            DrawAxesAndGrid(g, width, height);

            // --- 2. Рисуємо плавну лінію графіка ---
            DrawGraphLine(g, width, height);

            // --- 3. Рисуємо точки з кроком Δx = 0.2 ---
            DrawDataPoints(g, width, height);
        }

        /// <summary>
        /// Рисує осі X, Y, числову сітку та мітки.
        /// </summary>
        private void DrawAxesAndGrid(Graphics g, int width, int height)
        {
            // --- Осі ---
            // Вісь Y (з урахуванням відступів)
            g.DrawLine(axisPen, padding, height - padding, padding, padding);
            // Вісь X (з урахуванням відступів)
            g.DrawLine(axisPen, padding, height - padding, width - padding, height - padding);

            // Назви осей
            g.DrawString("Y", this.Font, textBrush, padding - 20, padding - 20);
            g.DrawString("X", this.Font, textBrush, width - padding + 5, height - padding - 20);
            // Зміщення початку позначки "0" для візуального коригування, бо значення "1.1" перекриває "0"
            g.DrawString("0", this.Font, textBrush, padding - 15 - 5, height - padding + 5);

            // --- Мітки та сітка по осі X (від 0.1 до 1.5 з кроком 0.2) ---
            for (double x = xMin; x <= xMax + 0.01; x += 0.2)
            {
                int px = MapX(x, width);
                g.DrawLine(gridPen, px, height - padding - 5, px, padding); // Вертикальна лінія сітки
                g.DrawLine(Pens.Black, px, height - padding - 5, px, height - padding + 5); // Засічка
                g.DrawString(x.ToString("F1"), axisFont, textBrush, px - 10, height - padding + 10);
            }

            // --- Мітки та сітка по осі Y (від 5 до 14 з кроком 1) ---
            for (double y = yMin; y <= yMax; y += 1)
            {
                int py = MapY(y, height);
                g.DrawLine(gridPen, padding + 5, py, width - padding, py); // Горизонтальна лінія сітки
                g.DrawLine(Pens.Black, padding - 5, py, padding + 5, py); // Засічка
                g.DrawString(y.ToString("F0"), axisFont, textBrush, padding - 30, py - 6);
            }
        }
        
        /// <summary>
        /// Рисує плавну лінію графіка функції.
        /// </summary>
        private void DrawGraphLine(Graphics g, int width, int height)
        {
            List<Point> points = new List<Point>();
            double dx_smooth = 0.01; // Малий крок для плавної лінії

            for (double x = xMin; x <= xMax; x += dx_smooth)
            {
                double y = func(x);
                // Додаємо точку, лише якщо вона в межах нашого логічного діапазону
                if (y >= yMin && y <= yMax)
                {
                    points.Add(new Point(MapX(x, width), MapY(y, height)));
                }
            }

            if (points.Count > 1)
            {
                g.DrawLines(graphPen, points.ToArray());
            }
        }

        /// <summary>
        /// Рисує окремі точки на графіку згідно кроку Δx = 0.2.
        /// </summary>
        private void DrawDataPoints(Graphics g, int width, int height)
        {
            double dx_user = 0.2; // Крок з завдання
            Font pointFont = new Font("Arial", 8, FontStyle.Bold);

            for (double x = xMin; x <= xMax + 0.01; x += dx_user)
            {
                double currentX = Math.Round(x, 2); // Округлення для уникнення проблем з точністю
                double y = func(currentX);
                
                int px = MapX(currentX, width);
                int py = MapY(y, height);

                // Рисуємо коло в точці
                g.FillEllipse(pointBrush, px - 4, py - 4, 8, 8);
                
                // Виводимо підпис з координатами
                string coord = $"({currentX:F1}, {y:F2})";
                g.DrawString(coord, pointFont, Brushes.Black, px + 10, py - 5);
            }
        }

        // --- Методи трансформери ---

        /// <summary>
        /// Перетворює логічну X-координату (з діапазону [xMin, xMax])
        /// у фізичну піксельну X-координату на формі.
        /// </summary>
        private int MapX(double x, int clientWidth)
        {
            return (int)(padding + (x - xMin) / (xMax - xMin) * (clientWidth - 2 * padding));
        }

        /// <summary>
        /// Перетворює логічну Y-координату (з діапазону [yMin, yMax])
        /// у фізичну піксельну Y-координату на формі.
        /// (Враховує, що вісь Y у GDI+ напрямлена вниз).
        /// </summary>
        private int MapY(double y, int clientHeight)
        {
            return (int)(clientHeight - padding - (y - yMin) / (yMax - yMin) * (clientHeight - 2 * padding));
        }
    }

}