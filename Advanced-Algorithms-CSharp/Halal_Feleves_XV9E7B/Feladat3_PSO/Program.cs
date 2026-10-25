using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

namespace Feladat_PSO
{
    public class Particle
    {
        public double Position;
        public double Velocity;
        public double BestPosition;
        public double BestFitness;

        public Particle(double initialPos, double initialVelo)
        {
            Position = initialPos;
            Velocity = initialVelo;
            BestPosition = initialPos;
            BestFitness = double.MaxValue;
        }
    }

    public class PSOSolver
    {
        private List<Particle> _particles;
        private double _gOpt;
        private double _gOptFitness;
        private int _iterations = 0;
        private readonly Random _rng = new();

        public PSOSolver(int swarmSize, double minRange, double maxRange)
        {
            _particles = new List<Particle>();
            _gOptFitness = double.MaxValue;
            for (int i = 0; i < swarmSize; i++)
                _particles.Add(new Particle(_rng.NextDouble() * (maxRange - minRange) + minRange, _rng.NextDouble() * 2 - 1));
        }

        private void CalculateVelocity(double omega, double phiP, double phiG)
        {
            foreach (var p in _particles)
            {
                double rp = _rng.NextDouble();
                double rg = _rng.NextDouble();
                p.Velocity = omega * p.Velocity + phiP * rp * (p.BestPosition - p.Position) + phiG * rg * (_gOpt - p.Position);
            }
        }

        public double Solve(Func<double, double> f, int maxIter)
        {
            foreach (var p in _particles)
            {
                double fit = f(p.Position);
                if (fit < p.BestFitness) { p.BestFitness = fit; p.BestPosition = p.Position; }
                if (fit < _gOptFitness) { _gOptFitness = fit; _gOpt = p.Position; }
            }

            Console.WriteLine($"Iteráció: 000 | Globális optimum: {_gOpt:F4} | Fitnesz: {_gOptFitness:F4}");

            for (_iterations = 1; _iterations <= maxIter; _iterations++)
            {
                CalculateVelocity(0.5, 1.5, 1.5);
                foreach (var p in _particles) p.Position += p.Velocity;

                foreach (var p in _particles)
                {
                    double fit = f(p.Position);
                    if (fit < p.BestFitness) { p.BestFitness = fit; p.BestPosition = p.Position; }
                    if (fit < _gOptFitness) { _gOptFitness = fit; _gOpt = p.Position; }
                }

                if (_iterations % 10 == 0)
                {
                    Console.WriteLine($"Iteráció: {_iterations:D3} | Globális optimum: {_gOpt:F4} | Fitnesz: {_gOptFitness:F4}");
                }
            }
            return _gOpt;
        }

        public void Draw(string filename, Func<double, double> f, double minX, double maxX)
        {
            int w = 800, h = 600;
            using var bmp = new Bitmap(w, h);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            List<PointF> curve = new List<PointF>();
            for (int i = 0; i < w; i++)
            {
                double x = minX + (i / (double)w) * (maxX - minX);
                curve.Add(new PointF(i, h - (float)(f(x) * 20 + 100)));
            }
            g.DrawLines(Pens.Black, curve.ToArray());

            foreach (var p in _particles)
            {
                float x = (float)((p.Position - minX) / (maxX - minX) * w); 
                float y = h - (float)(f(p.Position) * 20 + 100);
                g.FillEllipse(Brushes.Red, x - 4, y - 4, 8, 8);
            }
            bmp.Save(filename, ImageFormat.Png);
        }
    }

    class Program
    {
        static void Main()
        {
            Random rnd = new Random();
            double target = rnd.NextDouble() * 10 - 5;
            Func<double, double> f = x => Math.Sin(x) * x + Math.Pow(x - 2, 2);
            var solver = new PSOSolver(30, -10, 10);

            var watch = System.Diagnostics.Stopwatch.StartNew();
            double result = solver.Solve(f, 100);
            watch.Stop();

            Console.WriteLine($"Talált minimum helye: {result:F4}");
            Console.WriteLine($"Idő: {watch.ElapsedMilliseconds} ms, Iteráció: 100");

            solver.Draw("result.png", f, -10, 10);
            Console.WriteLine("Eredmény elmentve: result.png");
        }
    }
}