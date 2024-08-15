using System.Collections;
using System.Collections.Generic;
using System.Text;

// 引数で渡した値の長さを返却する組み込み関数
public class OsLen : OstBuiltinFunctions
{
    // 引数で渡した値の長さを返却する
    public OsObject eval(List<OsObject> args)
    {
        OsObject arg = args[0];
        if (arg == null || arg is OsNull)
        {
            // 引数がNULLの場合は0
            return new OsInteger(0);
        }
        if (arg is OsArray arr)
        {
            // 引数が配列の場合は要素数を返却
            return new OsInteger(arr.values.Count);
        }

        return new OsInteger(arg.ToString().Length);
    }

    // 関数の引数が正常であることをチェックする 正常時tureを返却
    public bool checkArgs(List<OsObject> args)
    {
        if (args == null || args.Count != 1)
        {
            // 引数1つ以外はエラー
            return false;
        }

        return true;
    }

    public OsObjectType type() => OsObjectType.BUILTINFUNCTIONS;
    override public string ToString() => "len()";
}

