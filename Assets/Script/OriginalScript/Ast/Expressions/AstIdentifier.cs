

using System.Collections.Generic;
using System.Linq;

// 抽象構文木で変数名や関数名などの識別子を扱うためのクラスです
public class AstIdentifier : AstExpression
{
    // 変数の種類
    public OsToken token;
    // 変数名
    public string value;

    public AstIdentifier(OsToken token, string value)
    {
        this.token = token;
        this.value = value;
    }

    public string tokenLiteral() => token.literal;
    public string toCode() => value;
}