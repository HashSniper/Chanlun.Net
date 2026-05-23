namespace Chan.Lib.Common;

public class ChanException : Exception
{
    public ErrCode ErrCode { get; }
    public string Msg { get; }

    public ChanException(string message, ErrCode code = ErrCode.COMMON_ERROR)
        : base(message)
    {
        ErrCode = code;
        Msg = message;
    }

    public bool IsKldataErr()
    {
        return ErrCode >= (ErrCode)200 && ErrCode < (ErrCode)300;
    }

    public bool IsChanErr()
    {
        return ErrCode >= (ErrCode)0 && ErrCode < (ErrCode)100;
    }
}
