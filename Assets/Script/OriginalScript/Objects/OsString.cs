
// 文字列型の評価結果を格納するクラス
public class OsString : OsObject
{
    // 設定値
    public string value;

    public OsString(string value) => this.value = value;
    override public string ToString() => value;
    public OsObjectType type() => OsObjectType.STRING;
}