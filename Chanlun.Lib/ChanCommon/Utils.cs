namespace Chanlun.Lib.ChanCommon;

public class Utils
{
    public static DateTime ConvertToDateTime(int d, int t)
    {

        if (d.ToString().Length == 8)
        {
            return DateTime.ParseExact(
                d.ToString(), 
                "yyyyMMdd", 
                System.Globalization.CultureInfo.InvariantCulture
            );
        }

        // 通达信 DATE 格式为 (年份-1900)*10000 + 月*100 + 日，如 1260115 表示 2026-01-15
        int year = (d / 10000) + 1900;
        int month = (d % 10000) / 100;
        int day = d % 100;

        int hour, minute, second;
        if (t >= 10000)
        {
            // HHMMSS 格式
            hour = t / 10000;
            minute = (t % 10000) / 100;
            second = t % 100;
        }
        else
        {
            // HHMM 格式
            hour = t / 100;
            minute = t % 100;
            second = 0;
        }

        // 处理通达信中可能的无效时间值（如日线时间为 0）
        if (hour == 0 && minute == 0 && second == 0)
        {
            hour = 0;
            minute = 0;
            second = 0;
        }

        return new DateTime(year, month, day, hour, minute, second);
    }
}