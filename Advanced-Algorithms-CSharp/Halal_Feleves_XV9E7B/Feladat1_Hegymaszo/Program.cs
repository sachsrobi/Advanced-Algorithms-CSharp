using System.Drawing;
using System.Drawing.Imaging;

namespace Feladat1_Hegymaszo
{
    public struct PointF2
    {
        public float X, Y;
        public PointF2(float x, float y) { X = x; Y = y; }
    }

    public class SmallestCoveringQuadrilateral
    {
        public List<PointF2> Points { get; set; } = new();

        public List<PointF2> GetBoundingBox()
        {
            float minX = Points.Min(p => p.X);
            float maxX = Points.Max(p => p.X);
            float minY = Points.Min(p => p.Y);
            float maxY = Points.Max(p => p.Y);

            return new List<PointF2>
        {
            new PointF2(minX, minY),
            new PointF2(maxX, minY),
            new PointF2(maxX, maxY),
            new PointF2(minX, maxY)
        };
        }

        public bool CoversAllPoints(List<PointF2> polygon)
        {
            foreach (var pt in Points)
            {
                if (!PointInPolygon(pt, polygon))
                    return false;
            }
            return true;
        }

        public bool PointInPolygon(PointF2 pt, List<PointF2> polygon)
        {
            bool inside = false;
            int count = polygon.Count;
            for (int i = 0, j = count - 1; i < count; j = i++)
            {
                var pi = polygon[i];
                var pj = polygon[j];
                if (((pi.Y > pt.Y) != (pj.Y > pt.Y)) &&
                    (pt.X < (pj.X - pi.X) * (pt.Y - pi.Y) / (pj.Y - pi.Y) + pi.X))
                {
                    inside = !inside;
                }
            }
            return inside;
        }

        public float PolygonArea(List<PointF2> polygon)
        {
            float area = 0;
            int n = polygon.Count;
            for (int i = 0; i < n; i++)
            {
                PointF2 p1 = polygon[i];
                PointF2 p2 = polygon[(i + 1) % n];
                area += p1.X * p2.Y - p2.X * p1.Y;
            }
            return Math.Abs(area) / 2;
        }

        public float Distance(PointF2 a, PointF2 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public List<PointF2> RotatePolygon(List<PointF2> polygon, float angleDegrees)
        {
            float angle = angleDegrees * (float)Math.PI / 180f;
            PointF2 center = GetPolygonCenter(polygon);
            var rotated = new List<PointF2>(polygon.Count);
            foreach (var p in polygon)
            {
                float x = p.X - center.X;
                float y = p.Y - center.Y;
                float xr = x * (float)Math.Cos(angle) - y * (float)Math.Sin(angle);
                float yr = x * (float)Math.Sin(angle) + y * (float)Math.Cos(angle);
                rotated.Add(new PointF2(xr + center.X, yr + center.Y));
            }
            return rotated;
        }

        public PointF2 GetPolygonCenter(List<PointF2> polygon)
        {
            float cx = 0, cy = 0;
            foreach (var p in polygon)
            {
                cx += p.X;
                cy += p.Y;
            }
            int n = polygon.Count;
            return new PointF2(cx / n, cy / n);
        }

        public List<PointF2> HillClimbingRandomRestart(out int totalIterations, out long elapsedMs)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            totalIterations = 0;

            Random rnd = new Random();
            List<PointF2> bestGlobal = null;
            float minGlobalArea = float.MaxValue;
            int restartCount = 0;
            int maxRestarts = 20;

            while (restartCount < maxRestarts)
            {
                var p = GetBoundingBox();
                p = EnsureBoundingBoxCoversAllPoints(p);
                float fp = PolygonArea(p);

                bool stuck = false;
                float epsilon = 1.0f;

                while (!stuck && totalIterations < 20000) 
                {
                    totalIterations++; 
                    var q = GetRandomNeighbor(p, epsilon, rnd);

                    if (CoversAllPoints(q))
                    {
                        float fq = PolygonArea(q);
                        if (fq < fp)
                        {
                            p = q;
                            fp = fq;
                        }
                        else
                        {
                            if (epsilon < 0.01f) stuck = true;
                            else epsilon *= 0.9f;
                        }
                    }
                }

                if (fp < minGlobalArea)
                {
                    minGlobalArea = fp;
                    bestGlobal = p;
                }
                restartCount++;
            }

            watch.Stop();
            elapsedMs = watch.ElapsedMilliseconds;
            return bestGlobal;
        }

        private List<PointF2> GetRandomNeighbor(List<PointF2> p, float epsilon, Random rnd)
        {
            var q = new List<PointF2>(p);

            if (rnd.NextDouble() < 0.8)
            {
                int index = rnd.Next(q.Count);
                double irany = rnd.NextDouble() * 2 * Math.PI;

                q[index] = new PointF2(
                    q[index].X + (float)(Math.Cos(irany) * epsilon),
                    q[index].Y + (float)(Math.Sin(irany) * epsilon)
                );
            }
            else
            {
                float szog = (rnd.Next(2) == 0 ? 1 : -1) * epsilon;
                q = RotatePolygon(q, szog);
            }

            return q;
        }

        private List<PointF2> EnsureBoundingBoxCoversAllPoints(List<PointF2> boundingBox)
        {
            int maxIterations = 10;
            int iteration = 0;
            while (!CoversAllPoints(boundingBox) && iteration < maxIterations)
            {
                boundingBox = ExpandBoundingBox(boundingBox);
                iteration++;
            }
            return boundingBox;
        }

        private List<PointF2> ExpandBoundingBox(List<PointF2> boundingBox)
        {
            float expansionFactor = 1.1f;
            PointF2 center = GetPolygonCenter(boundingBox);
            var expanded = new List<PointF2>();
            foreach (var point in boundingBox)
            {
                float dx = point.X - center.X;
                float dy = point.Y - center.Y;
                expanded.Add(new PointF2(center.X + dx * expansionFactor, center.Y + dy * expansionFactor));
            }
            return expanded;
        }

        public void Draw(string filename, List<PointF2> polygon)
        {
            const int width = 600, height = 600;
            const float scale = 10f;
            const float offset = 50f;

            using var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            foreach (var p in Points)
            {
                float x = p.X * scale + offset;
                float y = height - (p.Y * scale + offset);
                g.FillEllipse(Brushes.Blue, x - 3, y - 3, 6, 6);
            }

            if (polygon != null && polygon.Count > 1)
            {
                PointF[] pts = polygon.Select(p => new PointF(p.X * scale + offset, height - (p.Y * scale + offset))).ToArray();
                g.DrawPolygon(Pens.Red, pts);
            }

            bmp.Save(filename, ImageFormat.Png);
        }
    }

    class Program
    {
        static void Main()
        {
            var problem = new SmallestCoveringQuadrilateral();
            Random rnd = new Random();

            int pontokSzama = 10;
            problem.Points = new List<PointF2>();

            while (problem.Points.Count < pontokSzama)
            {
                float x = (float)Math.Round(rnd.NextDouble() * 25.0, 2);
                float y = (float)Math.Round(rnd.NextDouble() * 25.0, 2);

                PointF2 ujPont = new PointF2(x, y);

                if (!problem.Points.Any(p => p.X == ujPont.X && p.Y == ujPont.Y))
                {
                    problem.Points.Add(ujPont);
                }
            }

            Console.WriteLine($"{pontokSzama} darab egyedi véletlen pont generálva (0.00 - 25.00):");
            foreach (var p in problem.Points)
            {
                Console.WriteLine($"Pont: ({p.X:F2}, {p.Y:F2})");
            }
            Console.WriteLine("------------------------------------");

            var solution = problem.HillClimbingRandomRestart(out int totalIter, out long timeMs);

            Console.WriteLine($"Futási idő: {timeMs} ms");
            Console.WriteLine($"Összes elvégzett iteráció: {totalIter}");

            Console.WriteLine("Az algoritmus által talált négyszög csúcsai:");
            foreach (var p in solution)
            {
                Console.WriteLine($"Csúcs: ({p.X:F2}, {p.Y:F2})");
            }

            problem.Draw("result.png", solution);
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Kép elmentve: result.png");
        }
    }
}
