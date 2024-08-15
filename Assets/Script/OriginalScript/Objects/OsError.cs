
// Return文の結果を格納するクラス
public class OsReturnValue : OsObject
{
    public OsObject value;
    public OsReturnValue(OsObject value) => this.value = value;
    override public string ToString() => value.ToString();
    public OsObjectType type() => OsObjectType.RETURN_VALUE;
}
