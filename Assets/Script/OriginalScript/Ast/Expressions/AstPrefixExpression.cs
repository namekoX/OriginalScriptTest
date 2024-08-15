
// 抽象構文木で前置演算子を扱うためのクラスです
// 演算子が前に来るもの　例えば - 1、!trueなど
public class AstPrefixExpression : AstExpression
{

    // 変数の種類
    public OsToken token;
    // 演算子
    public string ope;
    // 値
    public AstExpression value;

    public string tokenLiteral() => token.literal;
    public string toCode() => "(" + ope + value.toCode() + ")";
}