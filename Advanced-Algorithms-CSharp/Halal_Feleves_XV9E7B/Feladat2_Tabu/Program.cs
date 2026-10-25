using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;

namespace Feladat2_Tabu
{
    public struct Point2D
    {
        public float X, Y;
        public Point2D(float x, float y) => (X, Y) = (x, y);
    }

    public class TabuSearchTSPSolver
    {
        private readonly List<Point2D> _points;
        private readonly Random _rng = new();
        private List<string> _tabuList = new();
        private int _currentInnerIterations = 0;
        private const int MaxInnerIterations = 500;
        private int _totalIterations = 0;

        public TabuSearchTSPSolver(List<Point2D> points) => _points = points;

        private bool StopCondition() => ++_currentInnerIterations >= MaxInnerIterations;

        public int GetTotalIterations() => _totalIterations;

        public List<int> TabuSearch(int maxOuterIterations = 30)
        {
            List<int> popt = null;

            for (int outer = 0; outer < maxOuterIterations; outer++)
            {
                List<int> p = Enumerable.Range(0, _points.Count).OrderBy(x => _rng.Next()).ToList();
                SetTabuBarrier();
                bool stuck = false;

                while (!stuck && !IsTabu(p) && !StopCondition())
                {
                    _totalIterations++;

                    if (popt == null || Fitnesz(p) < Fitnesz(popt))
                        popt = new List<int>(p);

                    AddTabu(p);
                    PurgeTabu(500);

                    List<int> q = GetBestNeighbor(p);
                    if (q != null && Fitnesz(q) < Fitnesz(p))
                        p = q;
                    else
                        stuck = true;
                }
            }
            return popt;
        }

        private float Fitnesz(List<int> path)
        {
            float dist = 0;
            for (int i = 0; i < path.Count; i++)
            {
                var p1 = _points[path[i]];
                var p2 = _points[path[(i + 1) % path.Count]];
                dist += (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
            }
            return dist;
        }

        private List<int> GetBestNeighbor(List<int> p)
        {
            List<int> bestQ = null;
            float bestF = float.MaxValue;

            for (int i = 0; i < p.Count; i++)
            {
                for (int j = i + 1; j < p.Count; j++)
                {
                    List<int> neighbor = new List<int>(p);
                    (neighbor[i], neighbor[j]) = (neighbor[j], neighbor[i]);

                    float fq = Fitnesz(neighbor);
                    if (fq < bestF) { bestF = fq; bestQ = neighbor; }
                }
            }
            return bestQ;
        }

        private bool IsTabu(List<int> p) => _tabuList.Contains(string.Join(",", p));
        private void AddTabu(List<int> p) => _tabuList.Add(string.Join(",", p));
        private void SetTabuBarrier() => _tabuList.Add("BARRIER_" + Guid.NewGuid());
        private void PurgeTabu(int limit) { if (_tabuList.Count > limit) _tabuList.RemoveAt(0); }

        public void Draw(string filename, List<int> path)
        {
            using var bmp = new Bitmap(600, 600);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            float scale = 20f, offset = 50f;
            var points = path.Select(i => new PointF(_points[i].X * scale + offset, 600 - (_points[i].Y * scale + offset))).ToArray();
            g.DrawLines(Pens.Red, points.Concat(new[] { points[0] }).ToArray());
            foreach (var p in points) g.FillEllipse(Brushes.Blue, p.X - 4, p.Y - 4, 8, 8);
            bmp.Save(filename, ImageFormat.Png);
        }
    }

    class Program
    {
        static void Main()
        {
            Random rnd = new Random();
            var points = Enumerable.Range(0, 20).Select(_ => new Point2D((float)Math.Round(rnd.NextDouble() * 25, 2), (float)Math.Round(rnd.NextDouble() * 25, 2))).ToList();

            var solver = new TabuSearchTSPSolver(points);
            Console.WriteLine("Eredeti véletlen városok:");
            points.ForEach(p => Console.Write($"({p.X:F2}, {p.Y:F2}) "));

            var watch = System.Diagnostics.Stopwatch.StartNew();

            var result = solver.TabuSearch();

            watch.Stop();

            Console.WriteLine($"\n\nFutási idő: {watch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Összes iteráció: {solver.GetTotalIterations()}");
            Console.WriteLine("\nA legrövidebb útvonal sorrendje:");
            Console.WriteLine(string.Join(" -> ", result));
            solver.Draw("result.png", result);
            Console.WriteLine("Az eredmény elmentve: result.png");
        }
    }
}