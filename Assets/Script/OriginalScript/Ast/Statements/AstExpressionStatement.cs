
using System.Linq;
using System.Text;

// 式文を扱うためのクラスです
public class AstExpressionStatement : AstStatement
{
    // IDENTトークン
    public OsToken token;
    // 式
    public AstExpression expression;

    // トークンのリテラル(IDENT)
    public string tokenLiteral() => token.literal;

    // コードを復元する
    public string toCode() => expression?.toCode() ?? "";
}