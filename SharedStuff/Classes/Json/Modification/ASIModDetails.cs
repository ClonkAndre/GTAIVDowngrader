namespace GTAIVDowngrader.Classes.Json.Modification
{
    public class ASIModDetails
    {

        #region Variables
        public bool ForScriptHook;
        #endregion

        #region Constructor
        public ASIModDetails(ASIModDetails instance)
        {
            ForScriptHook = instance.ForScriptHook;
        }
        public ASIModDetails()
        {

        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format("ForScriptHook: {0}", ForScriptHook);
        }
        #endregion

    }
}
