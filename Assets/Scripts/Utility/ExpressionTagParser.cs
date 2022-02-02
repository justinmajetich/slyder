using System;
using System.Collections.Generic;

public static class ExpressionTagParser
{
    public static event Action<int> OnSpeedExpressed;
    public static event Action<int> OnPauseExpressed;
    public static event Action<int> OnEmotionExpressed;
    public static event Action<int> OnActionExpressed;

    /// <summary>
    /// Parses expression tag embedded in subtitle text, firing events accordinly.
    /// </summary>
    /// <param name="statement">The statement to parse.</param>
    /// <param name="offset">The offset at which to begin parsing statement string.</param>
    /// <returns>Returns the index of the character following tag close.</returns>
    public static string Parse(string statement, int tagStartPosition)
    {
        Dictionary<string, int> tag = new Dictionary<string, int>();
        string key = "";
        string valueString = "";
        int i = tagStartPosition + 1;

        // Copies key from tag.
        while (statement[i] != '=')
        {
            key += statement[i++];
        }
        i++;

        // Copies value from tag.
        while (statement[i] != Constants.ExpressionTagClose)
        {
            valueString += statement[i++];
        }
        i++;

        int value = int.Parse(valueString);

        switch (key)
        {
            case Constants.ExpressionTagSpeed:
                OnSpeedExpressed?.Invoke(value);
                break;
            case Constants.ExpressionTagPause:
                OnPauseExpressed?.Invoke(value);
                break;
            case Constants.ExpressionTagEmotion:
                OnEmotionExpressed?.Invoke(value);
                break;
            case Constants.ExpressionTagAction:
                OnActionExpressed?.Invoke(value);
                break;
        }

        // Return subtitle without tag.
        return statement.Remove(tagStartPosition, i - tagStartPosition);
    }
}
