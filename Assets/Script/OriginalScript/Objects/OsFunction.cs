using System.Collections.Generic;
using System.Text;

// 関数型の評価結果を格納するクラス
public class OsFunction : OsObject
{
    // 関数名
    public AstIdentifier name;
    // 関数のパラメータ
    public List<AstIdentifier> parameters = new List<AstIdentifier>();
    // 関数の実行内容
    public AstBlock body;
    // 変数格納用
    public OsEnviroment env;

    override public string ToString()
    {
        List<string> param = new List<string>();
        foreach (AstExpression ele in parameters)
        {
            param.Add(ele.toCode());
        }
        StringBuilder builder = new StringBuilder();
        builder.Append("function " + name.toCode());
        builder.Append("(");
        builder.Append(string.Join(", ", param));
        builder.Append(")");
        builder.Append(body.toCode());
        return builder.ToString();
    }

    public OsObjectType type() => OsObjectType.FUNCTION;
}