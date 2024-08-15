

using System.Collections.Generic;
using System.Linq;
using System.Text;

// 抽象構文木のルートノードです
public class AstRoot : AstNode
{
    // パースシテトークンの一覧を格納する
    public List<AstStatement> statements = new List<AstStatement>();

    // 便宜上実装する 最初のトークンのリテラルを返却
    public string tokenLiteral()
    {
        return statements.FirstOrDefault()?.tokenLiteral() ?? "";
    }

    // コードを復元する
    public string toCode()
    {
        StringBuilder builder = new StringBuilder();
        foreach (AstStatement ast in statements)
        {
            builder.AppendLine(ast.toCode());
        }
        return builder.ToString().TrimEnd();
    }
}