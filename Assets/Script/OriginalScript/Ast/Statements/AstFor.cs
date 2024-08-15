using System.Collections;
using System.Collections.Generic;
using System.Text;

// 抽象構文木でFor文を扱うためのクラスです
public class AstFor : AstStatement
{
    // 種類(For)
    public OsToken token;
    // 処理を継続する条件
    public AstExpression condition;
    // 繰り返し実行するコード
    public AstBlock statements;
    // 初期処理
    public AstStatement init;
    // ループ終了時に実行される処理
    public AstExpression next;

    public string toCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("for(");
        builder.Append(init.toCode());
        builder.Append(";");
        builder.Append(condition.toCode());
        builder.Append(";");
        builder.Append(next.toCode());
        builder.Append("){");
        builder.Append(statements.toCode());
        builder.Append("}");

        return builder.ToString();
    }

    public string tokenLiteral() => token.literal;

}