namespace Chanlun.Lib.Memory;

public static class StockTimeCache
{
    private static float BuildBaseKey(string code, float[] date, float[] time)
    {
        var keyString = $"{code}_{date[0]}:{time[0]}-{date[^1]}:{time[^1]}_{date.Length}";
        // FNV-1a 哈希，取低24位确保在 float 精确整数范围内（2^24 = 16,777,216）
        uint hash = 2166136261u;
        foreach (char c in keyString)
        {
            hash ^= c;
            hash *= 16777619u;
        }
        return hash & 0x00FFFFFF;
    }

    public static float[] Set(float[] code, float[] date, float[] time)
    {
        int codeValue = (int)code[0];
        string stockCode = codeValue.ToString("D6");
        
        var baseKey = BuildBaseKey(stockCode, date, time);
        var dateTimeList = new List<DateTime>(date.Length);

        for (int i = 0; i < date.Length; i++)
        {
            int d = (int)date[i];
            int t = (int)time[i];

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

            dateTimeList.Add(new DateTime(year, month, day, hour, minute, second));
        }

        ChanMemory.Add(baseKey,nameof(StockTimeCache), dateTimeList);

        // 返回与输入等长的数组，每个位置填充 key，便于通达信侧获取缓存标识
        var result = new float[date.Length];
        result[0] = baseKey;
        for (int i = 1; i < date.Length; ++i)
        {
            result[i] = i;
        }
        
        return result;
    }

    public static List<DateTime>? Get(float key)
    {
        return ChanMemory.Get(key,nameof(StockTimeCache)) as List<DateTime>;
    }
}
