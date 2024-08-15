// 評価結果を格納するオブジェクトのインターフェース
public interface OsObject
{
    // 結果の型
    public OsObjectType type();

    // 値の文字列表現を返す
    public string ToString();
}

public enum OsObjectType
{
    INTEGER, // 整数
    FLOAT, // 実数
    STRING, // 文字列
    BOOLEAN, // 真偽値
    NULL, // 未割当
    RETURN_VALUE, // 返却値
    ERROR, // エラー
    FUNCTION, // 関数
    BUILTINFUNCTIONS, // 組込関数
    ARRAY, // 配列
}