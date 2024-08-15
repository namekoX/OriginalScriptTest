using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

// 抽象構文木で関数呼び出し式を扱うためのクラスです
// myFunction(x);
public class AstCall : AstExpression
{
    public OsToken token;
    public AstExpression functionName;
    public List<AstExpression> arguments;

    public string toCode()
    {
        List<string> args = new List<string>();
        foreach (AstExpression ele in arguments)
        {
            args.Add(ele.toCode());
        }

        StringBuilder builder = new StringBuilder();
        builder.Append(functionName.toCode());
        builder.Append("(");
        builder.Append(string.Join(", ", args));
        builder.Append(")");
        return builder.ToString();
    }

    public string tokenLiteral() => token.literal;
}
