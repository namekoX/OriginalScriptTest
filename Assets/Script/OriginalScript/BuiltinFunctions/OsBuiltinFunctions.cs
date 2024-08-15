using System.Collections;
using System.Collections.Generic;
using System.Text;

// 抽象構文木から呼び出し可能な組込関数を定義するインターフェースです
public interface OstBuiltinFunctions : OsObject
{
    // 関数を実行する
    public OsObject eval(List<OsObject> args);

    // 関数の引数が正常であることをチェックする 正常時tureを返却
    public bool checkArgs(List<OsObject> args);
}

