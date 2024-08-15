using System.Collections;
using System.Collections.Generic;
using System.Text;

// Dictionaryを使用する組み込み関数
public class OsHash : OstBuiltinFunctions
{
    // 変数を格納しておくためのDictionary
    public Dictionary<string, OsObject> store = new Dictionary<string, OsObject>();

    // Dictionaryを作成する 自分自身を返却
    public OsObject eval(List<OsObject> args)
    {
        store = new Dictionary<string, OsObject>();
        return this;
    }

    // 関数の引数が正常であることをチェックする 正常時tureを返却
    public bool checkArgs(List<OsObject> args)
    {
        if (args != null && args.Count != 0)
        {
            // 引数は使用できない
            return false;
        }

        return true;
    }

    // Dictionaryに引数を追加する
    public OsObject add(List<OsObject> args)
    {
        if (args == null || args.Count != 2)
        {
            // 引数がキーと値のペアじゃなければNG
            return new OsError("addメソッドの引数が不正です。");
        }

        store[args[0].ToString()] = args[1];
        return this;
    }

    // Dictionaryに値があるか確認する
    public OsObject contains(List<OsObject> args)
    {
        if (args == null || args.Count != 1)
        {
            // 引数がキーじゃなければNG
            return new OsError("containsメソッドの引数が不正です。");
        }

        return new OsBoolean(store.ContainsKey(args[0].ToString()));
    }

    // Dictionaryから値を取り出す
    public OsObject get(List<OsObject> args)
    {
        if (args == null || args.Count != 1)
        {
            // 引数がキーじゃなければNG
            return new OsError("getメソッドの引数が不正です。");
        }

        bool ok = store.TryGetValue(args[0].ToString(), out OsObject value);
        if (!ok)
        {
            return new OsError($"{args[0].ToString()}が見つかりません。");
        }
        return value;
    }

    public OsObjectType type() => OsObjectType.BUILTINFUNCTIONS;
    override public string ToString() => "hash()";
}

