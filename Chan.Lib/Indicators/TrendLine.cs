using Chan.Lib.Common;
using Chan.Lib.Bis;

namespace Chan.Lib.Indicators;

public class Point
{
    public int X { get; }
    public double Y { get; }

    public Point(int x, double y)
    {
        X = x;
        Y = y;
    }

    public double CalSlope(Point p)
    {
        return X != p.X ? (Y - p.Y) / (X - p.X) : double.PositiveInfinity;
    }
}

public class Line
{
    public Point P { get; }
    public double Slope { get; }

    public Line(Point p, double slope)
    {
        P = p;
        Slope = slope;
    }

    public double CalDis(Point p)
    {
        return Math.Abs(Slope * p.X - p.Y + P.Y - Slope * P.X) / Math.Sqrt(Slope * Slope + 1);
    }
}

public class TrendLine
{
    public Line? Line { get; private set; }
    public TREND_LINE_SIDE Side { get; }

    public TrendLine(List<Bi> lst, TREND_LINE_SIDE side = TREND_LINE_SIDE.OUTSIDE)
    {
        Side = side;
        Cal(lst);
    }

    private void Cal(List<Bi> lst)
    {
        double bench = double.PositiveInfinity;
        List<Point> allP;
        if (Side == TREND_LINE_SIDE.INSIDE)
            allP = lst.Select((bi, i) => new Point(bi.GetBeginKlu().Idx, bi.GetBeginVal())).Reverse().ToList();
        else
            allP = lst.Select((bi, i) => new Point(bi.GetEndKlu().Idx, bi.GetEndVal())).Reverse().ToList();

        var cP = new List<Point>(allP);
        while (true)
        {
            var (line, idx) = CalTl(cP, lst[^1].Dir, Side);
            double dis = allP.Sum(p => line.CalDis(p));
            if (dis < bench)
            {
                bench = dis;
                Line = line;
            }
            cP = cP.Skip(idx).ToList();
            if (cP.Count == 1)
                break;
        }
    }

    private static double InitPeakSlope(BI_DIR dir, TREND_LINE_SIDE side)
    {
        if (side == TREND_LINE_SIDE.INSIDE)
            return 0;
        return dir == BI_DIR.UP ? double.PositiveInfinity : double.NegativeInfinity;
    }

    private static (Line line, int idx) CalTl(List<Point> cP, BI_DIR dir, TREND_LINE_SIDE side)
    {
        var p = cP[0];
        double peakSlope = InitPeakSlope(dir, side);
        int idx = 1;
        for (int pointIdx = 0; pointIdx < cP.Count - 1; pointIdx++)
        {
            var p2 = cP[pointIdx + 1];
            double slope = p.CalSlope(p2);
            if ((dir == BI_DIR.UP && slope < 0) || (dir == BI_DIR.DOWN && slope > 0))
                continue;
            if (side == TREND_LINE_SIDE.INSIDE)
            {
                if ((dir == BI_DIR.UP && slope > peakSlope) || (dir == BI_DIR.DOWN && slope < peakSlope))
                {
                    peakSlope = slope;
                    idx = pointIdx + 1;
                }
            }
            else
            {
                if ((dir == BI_DIR.UP && slope < peakSlope) || (dir == BI_DIR.DOWN && slope > peakSlope))
                {
                    peakSlope = slope;
                    idx = pointIdx + 1;
                }
            }
        }
        return (new Line(p, peakSlope), idx);
    }
}
