using System.Collections.Generic;

// 宣言された変数値や関数などを格納しておくためのクラス
public class OsEnviroment
{
    // 変数を格納しておくためのDictionary
    public Dictionary<string, OsObject> store = new Dictionary<string, OsObject>();

    // 外部環境、自分の上のブロックで宣言された環境を引き継ぐために使う
    public OsEnviroment outer { get; set; }



    // 新規作成時、組込関数を設定する(引数falseで設定しない)
    public OsEnviroment()
    {
        setBuiltinFunc();
    }
    public OsEnviroment(bool isSetBuiltinFunc)
    {
        if (!isSetBuiltinFunc)
        {
            setBuiltinFunc();
        }
    }

    // 組み込み関数の設定
    protected void setBuiltinFunc()
    {
        store.Add("len", new OsLen());
        store.Add("createHashMap", new OsHash());
    }

    // 自分自身を引き継いで新しい環境を作成する
    public static OsEnviroment createNewEnclosedEnviroment(OsEnviroment outer)
    {
        OsEnviroment env = new OsEnviroment(false);
        env.outer = outer;
        return env;
    }

    public (OsObject, bool) get(string name)
    {
        bool ok = store.TryGetValue(name, out OsObject value);
        if (!ok && outer != null)
        {
            // 自環境に存在しない場合、外側の環境を確認
            (value, ok) = outer.get(name);
        }
        return (value, ok);
    }

    public OsObject set(string name, OsObject value)
    {
        store[name] = value;
        if (outer != null && outer.contains(name))
        {
            // 外部環境にある変数ならその変数も書き換え
            outer.set(name, value);
        }
        return value;
    }

    public bool contains(string name)
    {
        return store.ContainsKey(name);
    }
}