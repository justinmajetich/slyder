public static class Constants
{
    public const char ExpressionTagOpen = '<';
    public const char ExpressionTagClose = '>';
    public const string ExpressionTagSpeed = "speed";
    public const string ExpressionTagPause = "pause";
    public const string ExpressionTagEmotion = "emotion";
    public const string ExpressionTagAction = "action";

    public const string LayerMaskWalkable = "Walkable";
    public const string LayerMaskInteractable = "Interactable";
    public const string LayerMaskSceneExit = "SceneExit";
    public const string TagWalkable = "Walkable";
    public const string TagInteractable = "Interactable";

    public class AudioSnapshots
    {
        public const string defaut = "default";
        public const string fadeOut = "fadeOut";
    }

    public enum Scene
    {
        MainMenu,
        Bedroom,
        Kitchen,
        Credits,
        DialogueUI,
        None
    }
}
