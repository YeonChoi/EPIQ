using System;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;

namespace Epiq
{
    public class Interpolatable
    {
        private readonly float _xOffset;
        private double _yOffset; // to make all y's positive..
        
        private IInterpolation _interpolation;

        protected Interpolatable()
        {
        }

        protected Interpolatable(Interpolatable o, float xOffset)
        {
            Xstart = o.Xstart + xOffset;
            Xend = o.Xend + xOffset;
            Xapex = o.Xapex + xOffset;
            PointCount = o.PointCount;
            Ymin = o.Ymin;
            Yapex = o.Yapex;
            _xOffset = xOffset;
            _yOffset = o._yOffset;
            _interpolation = o._interpolation;
        }

        public Interpolatable(float[] xs, float[] ys)
        {
            SetInterpolator(xs, ys);
        }

        public float Xstart { get; private set; }
        public float Xend { get; private set; }
        public float Xapex { get; private set; }
        public float Ymin { get; private set; }
        public float Yapex { get; private set; }
        public int PointCount { get; private set; }


        public void SetInterpolator(float[] xs, float[] ys)
        {
            var flanking = 0;
            var xds = new double[xs.Length + flanking*2];
            var yds = new double[xs.Length + flanking*2];


            for (var i = 0; i < xds.Length; i++)
                if (i < flanking)
                    xds[i] = xs[0] - flanking + i;
                else if (i >= xds.Length - flanking) xds[i] = xs[xs.Length - 1] + (i - (xds.Length - flanking - 1));
                else xds[i] = xs[i - flanking];

            _yOffset = 1e-4;
            for (var i = 0; i < yds.Length; i++)
            {
                if (i < flanking)
                    yds[i] = ys[0];
                else if (i >= yds.Length - flanking) yds[i] = ys[ys.Length - 1];
                else yds[i] = ys[i - flanking];
                _yOffset = Math.Min(_yOffset, yds[i]);
            }

            if (_yOffset <= 0)
            {
                for (var i = 0; i < yds.Length; i++)
                {
                    yds[i] -= _yOffset - 1e-4;
                }
            }


            _interpolation = Interpolate.LogLinear(xds, yds);
            Xstart = xs[0];
            Xend = xs[xs.Length - 1];
            Ymin = float.PositiveInfinity;
            foreach (var t in ys)
                Ymin = Math.Min(Ymin, t);
            PointCount = xs.Length;

            Yapex = float.NegativeInfinity;
            
            /*for (var i = 0; i < xs.Length; i++)
            {
                var y = ys[i];
                if (y < Yapex) continue;
                Yapex = y;
                Xapex = xs[i];
            }*/

            const int l = 200;
            for (var i = 0; i < l; i++)
            {
                var x = Xstart + (Xend - Xstart)/(l - 1) * i;
                var y = InterpolateAt(x);
                if (y < Yapex) continue;
                Yapex = y;
                Xapex = x;
            }
        }

        public float GetXSpan()
        {
            return Xend - Xstart;
        }

        public float GetYSpan()
        {
            return Yapex - Ymin;
        }

        public float InterpolateAt(float x)
        {
            var v = (float)(_interpolation.Interpolate(x - _xOffset) + _yOffset);
           // Console.WriteLine(v + " " + _yOffset);
            if (x < Xstart) return Math.Min(v, InterpolateAt(Xstart));
            if (x > Xend) return Math.Min(v, InterpolateAt(Xend));
            //0;//Ymin;
            return v; //Math.Min(Ymax * 1.1f, Math.Max(Ymin * .9f, v));
            //return v;
        }


        public float DifferentiateAt(float x)
        {
            return (float) _interpolation.Differentiate(x - _xOffset);
        }

        public float DifferentiateAt(float x, float xDelta)
        {
            return (InterpolateAt(x + xDelta) - InterpolateAt(x - xDelta))/(xDelta*2);
        }

        public float[] InterpolateAt(float[] xs)
        {
            var ys = new float[xs.Length];
            for (var i = 0; i < ys.Length; i++) ys[i] = InterpolateAt(xs[i]);
            return ys;
        }


        public void Print(float[] xs, string name)
        {
            Console.Write(name + @"=[");
            foreach (var x in xs)
                Console.Write(InterpolateAt(x) + @",");
            Console.WriteLine(@"];");
        }
    }
}