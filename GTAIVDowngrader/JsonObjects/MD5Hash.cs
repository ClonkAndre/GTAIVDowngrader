namespace GTAIVDowngrader.JsonObjects
{
    public class MD5Hash
    {
        #region Variables
        public string Version;
        public string Hash;
        #endregion

        #region Constructor
        public MD5Hash()
        {

        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format("Version: {0}, Hash: {1}", Version, Hash);
        }
        #endregion
    }
}
