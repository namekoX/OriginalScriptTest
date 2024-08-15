
// 抽象構文木で整数リテラルを扱うためのクラスです
public class AstInteger : AstExpression
{
    // 変数の種類
    public OsToken token;
    // 値
    public int value;

    public AstInteger(OsToken token, int value)
    {
        this.token = token;
        this.value = value;
    }

    public string tokenLiteral() => token.literal;
    public string toCode() => value.ToString();

    // float型に変換
    public AstFloat toFloat()
    {
        return new AstFloat(new OsToken(OsTokenType.FLOAT, token.literal), value);
    }
}