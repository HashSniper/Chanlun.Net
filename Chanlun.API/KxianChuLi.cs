namespace Chanlun.API;

public class KxianChuLi
{
    public List<KxianRaw> KxianRawList = new();
    public List<Kxian> KxianList = new();

    public void Add(float gao, float di)
    {
        KxianRaw raw;
        raw.Gao = gao;
        raw.Di = di;
        KxianRawList.Add(raw);

        if (KxianList.Count == 0)
        {
            Kxian kxian;
            kxian.Gao = gao;
            kxian.Di = di;
            kxian.FangXiang = 1;
            kxian.KaiShi = 0;
            kxian.JieShu = 0;
            kxian.ZhongJian = 0;
            KxianList.Add(kxian);
        }
        else
        {
            var last = KxianList[KxianList.Count - 1];
            if (gao > last.Gao && di > last.Di)
            {
                // 向上
                Kxian kxian;
                kxian.Gao = gao;
                kxian.Di = di;
                kxian.FangXiang = 1;
                kxian.KaiShi = last.JieShu + 1;
                kxian.JieShu = kxian.KaiShi;
                kxian.ZhongJian = kxian.KaiShi;
                KxianList.Add(kxian);
            }
            else if (gao < last.Gao && di < last.Di)
            {
                // 向下
                Kxian kxian;
                kxian.Gao = gao;
                kxian.Di = di;
                kxian.FangXiang = -1;
                kxian.KaiShi = last.JieShu + 1;
                kxian.JieShu = kxian.KaiShi;
                kxian.ZhongJian = kxian.KaiShi;
                KxianList.Add(kxian);
            }
            else if (gao <= last.Gao && di >= last.Di)
            {
                // 前包含
                var current = KxianList[KxianList.Count - 1];
                if (current.FangXiang == 1)
                    current.Di = di;
                else
                    current.Gao = gao;
                current.JieShu = current.JieShu + 1;
                KxianList[KxianList.Count - 1] = current;
            }
            else
            {
                // 后包含
                var current = KxianList[KxianList.Count - 1];
                if (current.FangXiang == 1)
                    current.Gao = gao;
                else
                    current.Di = di;
                current.JieShu = current.JieShu + 1;
                current.ZhongJian = current.JieShu;
                KxianList[KxianList.Count - 1] = current;
            }
        }
    }
}
