using System.Collections;
using System.Collections.Generic;
using System.Text;

// 抽象構文木でIf文を扱うためのクラスです
public class AstIf : AstExpression
{
    // 種類(IF)
    public OsToken token;
    // 条件
    public List<AstExpression> conditions = new List<AstExpression>();
    // 条件がtrueで実行するコード
    public List<AstBlock> consequences = new List<AstBlock>();
    // 条件がfalseで実行するコード(else文)
    public AstBlock alternative;

    public string toCode()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("if");
        builder.Append(conditions[0].toCode());
        builder.Append(" ");
        builder.Append(consequences[0].toCode());

        // if else句があるとき
        for (int i = 1; i < conditions.Count; i++) {
            builder.Append("else if");
            builder.Append(conditions[i].toCode());
            builder.Append(" ");
            builder.Append(consequences[i].toCode());
        }

        // else句があるとき
        if (alternative != null)
        {
            builder.Append("else ");
            builder.Append(alternative.toCode());
        }

        return builder.ToString();
    }

    public string tokenLiteral() => token.literal;

}