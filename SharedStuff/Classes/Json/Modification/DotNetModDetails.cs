namespace GTAIVDowngrader.Classes.Json.Modification
{
    public class DotNetModDetails
    {

        #region Variables
        public bool ForScriptHookDotNet;
        public bool ForIVSDKDotNet;
        #endregion

        #region Constructor
        public DotNetModDetails(DotNetModDetails instance)
        {
            ForScriptHookDotNet = instance.ForIVSDKDotNet;
            ForIVSDKDotNet = instance.ForIVSDKDotNet;
        }
        public DotNetModDetails()
        {
            
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format("ForScriptHookDotNet: {0}, ForIVSDKDotNet: {1}", ForScriptHookDotNet, ForIVSDKDotNet);
        }
        #endregion

    }
}
