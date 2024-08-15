
// エラーを格納するクラス
public class OsError : OsObject
{
    public string message;
    public OsError(string message) => this.message = message;
    override public string ToString() => message;
    public OsObjectType type() => OsObjectType.ERROR;
}
