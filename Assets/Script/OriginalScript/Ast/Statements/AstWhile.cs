using System.Collections;
using System.Collections.Generic;
using System.Text;

// 抽象構文木でWhile文を扱うためのクラスです
public class AstWhile : AstStatement
{
    // 種類(For)
    public OsToken token;
    // 処理を継続する条件
    public AstExpression condition;
    // 繰り返し実行するコード
    public AstBlock statements;

    public string toCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("while(");
        builder.Append(condition.toCode());
        builder.Append(")");
        builder.Append(statements.toCode());

        return builder.ToString();
    }

    public string tokenLiteral() => token.literal;

}