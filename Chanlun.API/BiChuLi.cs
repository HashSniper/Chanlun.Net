namespace Chanlun.API;

public class BiChuLi
{
    public List<Bi> BiList = new();

    private static bool IfChengbi(List<Kxian> tempKxianList, int direction)
    {
        if (tempKxianList.Count < 4)
            return false;

        if (direction == -1)
        {
            int i = 2;
            while (true)
            {
                for (; i < tempKxianList.Count; i++)
                {
                    if (tempKxianList[i].Di < tempKxianList[i - 1].Di && tempKxianList[i - 1].Di < tempKxianList[i - 2].Di)
                        break;
                }
                if (i >= tempKxianList.Count)
                    return false;

                float zuiDiJia = tempKxianList[i].Di;
                for (int j = i + 1; j < tempKxianList.Count; j++)
                {
                    if (tempKxianList[j].Di < zuiDiJia)
                        return true;
                }
                i = i + 1;
            }
        }
        else if (direction == 1)
        {
            int i = 2;
            while (true)
            {
                for (; i < tempKxianList.Count; i++)
                {
                    if (tempKxianList[i].Gao > tempKxianList[i - 1].Gao && tempKxianList[i - 1].Gao > tempKxianList[i - 2].Gao)
                        break;
                }
                if (i >= tempKxianList.Count)
                    return false;

                float zuiGaoJia = tempKxianList[i].Gao;
                for (int j = i + 1; j < tempKxianList.Count; j++)
                {
                    if (tempKxianList[j].Gao > zuiGaoJia)
                        return true;
                }
                i = i + 1;
            }
        }
        return false;
    }

    public void Handle(List<Kxian> kxianList)
    {
        List<Kxian> tempKxianList = new();

        foreach (var iter in kxianList)
        {
            if (BiList.Count == 0)
            {
                Bi bi = new();
                bi.FangXiang = 1;
                bi.KaiShi = iter.KaiShi;
                bi.JieShu = iter.JieShu;
                bi.Gao = iter.Gao;
                bi.Di = iter.Di;
                bi.KxianList = new List<Kxian> { iter };
                BiList.Add(bi);
            }
            else
            {
                if (BiList[BiList.Count - 1].FangXiang == 1)
                {
                    if (iter.Gao >= BiList[BiList.Count - 1].Gao)
                    {
                        var bi = BiList[BiList.Count - 1];
                        bi.JieShu = iter.JieShu;
                        bi.Gao = iter.Gao;
                        if (tempKxianList.Count > 0)
                        {
                            bi.KxianList.AddRange(tempKxianList);
                            tempKxianList.Clear();
                        }
                        bi.KxianList.Add(iter);
                        BiList[BiList.Count - 1] = bi;
                    }
                    else
                    {
                        tempKxianList.Add(iter);
                        if (IfChengbi(tempKxianList, -1))
                        {
                            Bi bi = new();
                            bi.FangXiang = -1;
                            bi.KaiShi = BiList[BiList.Count - 1].JieShu;
                            bi.JieShu = tempKxianList[tempKxianList.Count - 1].JieShu;
                            bi.Di = tempKxianList[tempKxianList.Count - 1].Di;
                            bi.Gao = BiList[BiList.Count - 1].Gao;
                            bi.KxianList = new List<Kxian>(tempKxianList);
                            tempKxianList.Clear();
                            BiList.Add(bi);
                        }
                    }
                }
                else if (BiList[BiList.Count - 1].FangXiang == -1)
                {
                    if (iter.Di <= BiList[BiList.Count - 1].Di)
                    {
                        var bi = BiList[BiList.Count - 1];
                        bi.JieShu = iter.JieShu;
                        bi.Di = iter.Di;
                        if (tempKxianList.Count > 0)
                        {
                            bi.KxianList.AddRange(tempKxianList);
                            tempKxianList.Clear();
                        }
                        bi.KxianList.Add(iter);
                        BiList[BiList.Count - 1] = bi;
                    }
                    else
                    {
                        tempKxianList.Add(iter);
                        if (IfChengbi(tempKxianList, 1))
                        {
                            Bi bi = new();
                            bi.FangXiang = 1;
                            bi.KaiShi = BiList[BiList.Count - 1].JieShu;
                            bi.JieShu = tempKxianList[tempKxianList.Count - 1].JieShu;
                            bi.Gao = tempKxianList[tempKxianList.Count - 1].Gao;
                            bi.Di = BiList[BiList.Count - 1].Di;
                            bi.KxianList = new List<Kxian>(tempKxianList);
                            tempKxianList.Clear();
                            BiList.Add(bi);
                        }
                    }
                }
            }
        }

        if (tempKxianList.Count >= 4)
        {
            if (BiList[BiList.Count - 1].FangXiang == 1)
            {
                if (IfChengbi(tempKxianList, -1))
                {
                    Bi bi = new();
                    bi.FangXiang = -1;
                    bi.KaiShi = BiList[BiList.Count - 1].JieShu;
                    bi.JieShu = tempKxianList[tempKxianList.Count - 1].JieShu;
                    bi.Di = tempKxianList[tempKxianList.Count - 1].Di;
                    bi.Gao = BiList[BiList.Count - 1].Gao;
                    bi.KxianList = new List<Kxian>(tempKxianList);
                    tempKxianList.Clear();
                    BiList.Add(bi);
                }
            }
            else if (BiList[BiList.Count - 1].FangXiang == -1)
            {
                if (IfChengbi(tempKxianList, 1))
                {
                    Bi bi = new();
                    bi.FangXiang = 1;
                    bi.KaiShi = BiList[BiList.Count - 1].JieShu;
                    bi.JieShu = tempKxianList[tempKxianList.Count - 1].JieShu;
                    bi.Gao = tempKxianList[tempKxianList.Count - 1].Gao;
                    bi.Di = BiList[BiList.Count - 1].Di;
                    bi.KxianList = new List<Kxian>(tempKxianList);
                    tempKxianList.Clear();
                    BiList.Add(bi);
                }
            }
        }
    }
}
