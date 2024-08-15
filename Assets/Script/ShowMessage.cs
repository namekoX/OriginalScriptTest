using System.Collections.Generic;
using UnityEngine;

// 引数で渡したメッセージを表示する組み込み関数
public class ShowMessage : OstBuiltinFunctions
{
    // メッセージを表示する
    public OsObject eval(List<OsObject> args)
    {
        Debug.Log("メッセージ: " + args[0]);

        return new OsNull();
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
    override public string ToString() => "showMessage()";
}

