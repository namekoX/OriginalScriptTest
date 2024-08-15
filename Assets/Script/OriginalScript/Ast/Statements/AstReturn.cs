

using System.Collections.Generic;
using System.Linq;
using System.Text;

// Return文を扱うためのクラスです
public class AstReturn : AstStatement
{
    // Returnトークン
    public OsToken token;
    // 返却値
    public AstExpression returnValue;

    // トークンのリテラル(return)
    public string tokenLiteral() => token.literal;

    // コードを復元する
    public string toCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(token?.literal ?? "");
        builder.Append(" ");
        builder.Append(returnValue?.toCode() ?? "");
        builder.Append(";");
        return builder.ToString();
    }
}