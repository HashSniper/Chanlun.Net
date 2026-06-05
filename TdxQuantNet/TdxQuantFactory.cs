namespace TdxQuantNet;

public class TdxQuantFactory
{
    public static TdxQuant Create(string tqName, string tdxPath)
    {
        var result = CreateInstance(tqName, tdxPath);
        if (result == null)
        {
            result = CreateInstance(tqName, tdxPath);
        }

        return result;
    }

    private static TdxQuant? CreateInstance(string tqName, string tdxPath)
    {
        var result = new TdxQuant(tqName, tdxPath);
        if (result.Init())
        {
            return result;
        }

        result.Dispose();

        return null;
    }
}