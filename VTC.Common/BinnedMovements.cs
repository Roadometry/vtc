using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VTC.Common
{
    public class BinnedMovements : List<Movement>
    {
        public TimeInterval Interval;

        static BinnedMovements CreateBin(List<Movement> movements, DateTime start, DateTime end)
        {
            BinnedMovements bm = new BinnedMovements();
            bm.AddRange(movements);
            bm.Interval.StartTime = start;
            bm.Interval.EndTime = end;
            return bm;
        }

        public static List<BinnedMovements> BinMovementsByTime(List<Movement> movements, int binSizeMinutes)
        {
            List<BinnedMovements> binList = new List<BinnedMovements>(movements.Count);

            if (movements.Count == 0)
            {
                return binList;
            }

            DateTime startTime = movements.Select(m => m.Timestamp).OrderByDescending(t => t).Last();
            DateTime endTime = movements.Select(m => m.Timestamp).OrderByDescending(t => t).First();
            DateTime startTimeRounded = startTime.Floor(TimeSpan.FromMinutes(binSizeMinutes));
            DateTime endTimeRounded = endTime.Ceiling(TimeSpan.FromMinutes(binSizeMinutes));

            for (DateTime binStart = startTimeRounded;
                binStart < endTimeRounded;
                binStart += TimeSpan.FromMinutes(binSizeMinutes))
            {
                var binEnd = binStart + TimeSpan.FromMinutes(binSizeMinutes);
                BinnedMovements bm = new BinnedMovements();
                bm.Interval.StartTime = binStart;
                bm.Interval.EndTime = binEnd;
                bm.AddRange(movements.Where(m => m.Timestamp >= binStart && m.Timestamp < binEnd));
                binList.Add(bm);
            }

            return binList;
        }
    }

    public struct TimeInterval
    {
        public DateTime StartTime;
        public DateTime EndTime;
    }

    public static class DateTimeExtensions
    {
        public static DateTime Floor(this DateTime dateTime, TimeSpan interval)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % interval.Ticks));
        }

        public static DateTime Ceiling(this DateTime dateTime, TimeSpan interval)
        {
            var overflow = dateTime.Ticks % interval.Ticks;

            return overflow == 0 ? dateTime : dateTime.AddTicks(interval.Ticks - overflow);
        }

        public static DateTime Round(this DateTime dateTime, TimeSpan interval)
        {
            var halfIntervalTicks = (interval.Ticks + 1) >> 1;

            return dateTime.AddTicks(halfIntervalTicks - ((dateTime.Ticks + halfIntervalTicks) % interval.Ticks));
        }
    }
}
