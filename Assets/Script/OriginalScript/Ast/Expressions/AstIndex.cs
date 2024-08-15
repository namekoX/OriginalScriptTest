
// 配列の添え字部を扱うためのクラス
using System.Text;

public class AstIndex : AstExpression
{
    // 変数の種類
    public OsToken token;
    // 変数名
    public AstExpression left;
    // 添え字
    public AstExpression index;

    public string tokenLiteral() => token.literal;
    public string toCode()
    {
        StringBuilder buider = new StringBuilder();

        buider.Append(left.toCode());
        buider.Append("[");
        buider.Append(index.toCode());
        buider.Append("]");
        return buider.ToString();
    }
}
