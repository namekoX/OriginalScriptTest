
// 抽象構文木でメソッド形式(文字列.文字列)を扱うためのクラスです
using System.Collections.Generic;
using System.Text;

public class AstMethod : AstExpression
{
    // 変数の種類
    public OsToken token;
    // 左値(変数名)
    public AstIdentifier left;
    // 右値(メソッド名)
    public AstIdentifier right;
    // 引数
    public List<AstExpression> arguments;

    public string tokenLiteral() => token.literal;
    public string toCode()
    {
        List<string> args = new List<string>();
        foreach (AstExpression ele in arguments)
        {
            args.Add(ele.toCode());
        }

        StringBuilder builder = new StringBuilder();
        builder.Append(left == null ? "" : left.toCode());
        builder.Append(".");
        builder.Append(right == null ? "" : right.toCode());
        builder.Append("(");
        builder.Append(string.Join(", ", args));
        builder.Append(")");
        return builder.ToString();
    }
}