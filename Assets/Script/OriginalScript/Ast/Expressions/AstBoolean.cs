
// 抽象構文木で真偽値リテラルを扱うためのクラスです
public class AstBoolean : AstExpression
{
    // 種類
    public OsToken token;
    // 真偽値
    public bool value;

    public AstBoolean(OsToken token, bool value)
    {
        this.token = token;
        this.value = value;
    }

    public string tokenLiteral() => token.literal;
    public string toCode() => token.literal;
}