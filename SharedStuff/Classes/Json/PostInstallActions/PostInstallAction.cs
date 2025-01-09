#if FILE_EDITOR_PROJ
using Newtonsoft.Json.Linq;
#endif

namespace GTAIVDowngrader.Classes.Json.PostInstallActions
{
    public class PostInstallAction
    {

        #region Variables
        public PostInstallActionType Type;
        public object Action;
        #endregion

        #region Constructor
        public PostInstallAction(PostInstallActionType type)
        {
            Type = type;
        }
        public PostInstallAction()
        {
            Type = PostInstallActionType.None;
        }
        #endregion

#if FILE_EDITOR_PROJ
        public void PrepareForEditor()
        {
            if (Action == null)
                return;

            switch (Type)
            {
                case PostInstallActionType.EditIniFile:
                    (Action as JObject).ToObject<EditIniFileAction>().PrepareForEditor();
                    break;
            }
        }
#endif

    }
}
