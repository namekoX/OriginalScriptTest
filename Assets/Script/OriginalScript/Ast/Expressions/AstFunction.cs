using System.Collections;
using System.Collections.Generic;
using System.Text;

// 抽象構文木で関数を扱うためのクラスです
public class AstFunction : AstExpression
{
    // 種類(関数)
    public OsToken token;
    // 関数名
    public AstIdentifier name;
    // 関数のパラメータ
    public List<AstIdentifier> parameters;
    // 関数の実行内容
    public AstBlock body;

    public string toCode()
    {
        List<string> param = new List<string>();
        foreach (AstExpression ele in parameters)
        {
            param.Add(ele.toCode());
        }
        StringBuilder builder = new StringBuilder();
        builder.Append("function " + name.toCode());
        builder.Append("(");
        builder.Append(string.Join(", ", param));
        builder.Append(")");
        builder.Append(body.toCode());
        return builder.ToString();
    }

    public string tokenLiteral() => token.literal;
}