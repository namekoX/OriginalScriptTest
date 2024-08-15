using System.Text;

// variant型を宣言する文を扱うためのクラスです
public class AstDecVar : AstStatement
{
    // Varトークン
    public OsToken token;
    // 変数名
    public AstIdentifier name;
    // 変数の設定値
    public AstExpression value;

    // トークンのリテラル(ver)
    public string tokenLiteral() => token.literal;

    // コードを復元する
    public string toCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(token?.literal ?? "");
        builder.Append(" ");
        builder.Append(name.toCode() ?? "");
        builder.Append(" = ");
        builder.Append(value?.toCode() ?? "");
        builder.Append(";");
        return builder.ToString();
    }

    // この型クラスに格納されている値が適切か判断する
    public bool check()
    {
        // varの場合はすべてOK
        return true;
    }
}