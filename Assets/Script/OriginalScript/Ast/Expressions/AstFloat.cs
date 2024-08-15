
// 抽象構文木で小数点含む数値を扱うためのクラスです
public class AstFloat : AstExpression
{
    // 変数の種類
    public OsToken token;
    // 値
    public float value;

    public AstFloat(OsToken token, float value)
    {
        this.token = token;
        this.value = value;
    }

    public string tokenLiteral() => token.literal;
    public string toCode() => value.ToString();

    // integer型に変換
    public AstInteger toInteger()
    {
        return new AstInteger(new OsToken(OsTokenType.INT, ((int)value).ToString()), ((int)value));
    }
}