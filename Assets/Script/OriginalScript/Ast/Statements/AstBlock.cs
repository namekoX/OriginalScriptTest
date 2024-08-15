using System.Collections.Generic;
using System.Text;


// ブロック文({ })を扱うためのクラスです
public class AstBlock : AstStatement
{
    // 中括弧トークン
    public OsToken token;
    // ブロック文の中にあるコード
    public List<AstStatement> statements;

    public string toCode()
    {
        StringBuilder builder = new StringBuilder();
        foreach (AstStatement statement in statements)
        {
            builder.Append(statement.toCode());
        }
        return builder.ToString();
    }

    public string tokenLiteral() => token.literal;
}
