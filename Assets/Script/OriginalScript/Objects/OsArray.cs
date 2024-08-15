
// 配列型の評価結果を格納するクラス
using System.Collections.Generic;
using System.Text;

public class OsArray : OsObject
{
    // 設定値
    public List<OsObject> values = new List<OsObject>();
    override public string ToString()
    {
        StringBuilder buider = new StringBuilder();

        if (values == null)
        {
            return null;
        }

        buider.Append("[");

        bool isFirst = true;
        foreach (OsObject val in values)
        {
            if (!isFirst)
            {
                buider.Append(",");
            }
            buider.Append(val == null ? "" : val.ToString());
            isFirst = false;
        }

        buider.Append("]");
        return buider.ToString();

    }
    public OsObjectType type() => OsObjectType.ARRAY;
}