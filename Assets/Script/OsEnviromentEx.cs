using System.Collections.Generic;

// 宣言された変数値や関数などを格納しておくためのクラス
public class OsEnviromentEx : OsEnviroment
{
    // 新規作成時、組込関数を設定する
    public OsEnviromentEx() : base()
    {
        setBuiltinFuncEx();
    }

    // 組み込み関数の追加設定
    private void setBuiltinFuncEx()
    {
        store.Add("showMessage", new ShowMessage());
    }
}