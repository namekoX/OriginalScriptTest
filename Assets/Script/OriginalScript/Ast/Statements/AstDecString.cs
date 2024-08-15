
// string型を宣言する文を扱うためのクラスです
public class AstDecString : AstDecVar
{
    // この型クラスに格納されている値が適切か判断する
    public new bool check()
    {
        // nullかstringの場合はOK
        if (value == null || value is AstString)
        {
            return true;
        }

        return false;

    }
}