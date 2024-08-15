
// 抽象構文木で文字列リテラルを扱うためのクラスです
public class AstString : AstExpression
{
    // 変数の種類
    public OsToken token;
    // 値
    public string value;

    public AstString(OsToken token, string value)
    {
        this.token = token;
        this.value = value;
    }

    public string tokenLiteral() => token.literal;
    public string toCode() => value;
}