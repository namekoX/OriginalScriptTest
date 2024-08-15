
// プログラムソースの文字列を置換する　表記ゆれなどを全部網羅するのが面倒なので置き換えたりする
using System.Text.RegularExpressions;

public class OsReplacer
{
    public static string replace(string input)
    {
        if (input == null)
        {
            return "";
        }

        // インクリメントを+1に変換する
        {
            string pattern = @"(\w+)\+\+";
            string replacement = @"($1 = ($1+1))";
            input = Regex.Replace(input, pattern, replacement);
        }
        {
            // 厳密には++XとX++は区別したいが、、、一旦同じ操作にする
            string pattern = @"\+\+(\w+)";
            string replacement = @"($1 = ($1+1))";
            input = Regex.Replace(input, pattern, replacement);
        }

        // デクリメントを-1に変換する
        {
            string pattern = @"(\w+)\-\-";
            string replacement = @"($1 = ($1-1))";
            input = Regex.Replace(input, pattern, replacement);
        }
        {
            string pattern = @"\-\-(\w+)";
            string replacement = @"($1 = ($1-1))";
            input = Regex.Replace(input, pattern, replacement);
        }

        return input;

    }
}