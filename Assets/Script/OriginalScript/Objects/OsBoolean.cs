
// 真偽値型の評価結果を格納するクラス
public class OsBoolean : OsObject
{
    // 設定値
    public bool value;

    public OsBoolean(bool value) => this.value = value;
    override public string ToString() => value ? "true" : "false";
    public OsObjectType type() => OsObjectType.BOOLEAN;
}