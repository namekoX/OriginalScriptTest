using System.Collections.Generic;
using System.Text;


// 抽象構文木で配列を扱うためのクラスです
public class AstArray : AstExpression
{
    // 変数の種類
    public OsToken token;
    // 値
    public List<AstExpression> values;

    public string tokenLiteral() => token.literal;
    public string toCode()
    {
        StringBuilder buider = new StringBuilder();

        if (values == null)
        {
            return null;
        }

        buider.Append("[");

        bool isFirst = true;
        foreach (AstExpression val in values)
        {
            if (!isFirst)
            {
                buider.Append(",");
            }
            buider.Append(val== null ? "" : val.toCode());
            isFirst = false;
        }

        buider.Append("]");
        return buider.ToString();
    }
}