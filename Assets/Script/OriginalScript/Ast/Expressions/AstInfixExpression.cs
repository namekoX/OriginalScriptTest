using System.Text;

// 抽象構文木で中間演算子を扱うためのクラスです
// 演算子が中間に来るもの　例えば 1 + 1、1 != 2など
public class AstInfixExpression : AstExpression
{

    // 変数の種類
    public OsToken token;
    // 演算子
    public string ope;
    // 右値
    public AstExpression right;
    // 左値
    public AstExpression left;

    public string tokenLiteral() => token.literal;
    public string toCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("(");
        builder.Append(left.toCode());
        builder.Append(" ");
        builder.Append(ope);
        builder.Append(" ");
        builder.Append(right.toCode());
        builder.Append(")");
        return builder.ToString();
    }
}