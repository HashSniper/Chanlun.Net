namespace Chan.Lib.ChanModel;

public class Features
{
    private readonly Dictionary<string, double?> _features = new();

    public Features(Dictionary<string, double>? featureDict = null)
    {
        if (featureDict != null)
        {
            foreach (var kv in featureDict)
                _features[kv.Key] = kv.Value;
        }
    }

    public void AddFeat(object inp1, double? inp2 = null)
    {
        if (inp1 is string s && inp2.HasValue)
        {
            _features[s] = inp2.Value;
        }
        else if (inp1 is Dictionary<string, double> dict)
        {
            foreach (var kv in dict)
                _features[kv.Key] = kv.Value;
        }
        else if (inp1 is Dictionary<string, double?> dict2)
        {
            foreach (var kv in dict2)
                _features[kv.Key] = kv.Value;
        }
        else if (inp1 is Features cf)
        {
            foreach (var kv in cf._features)
                _features[kv.Key] = kv.Value;
        }
    }
}
