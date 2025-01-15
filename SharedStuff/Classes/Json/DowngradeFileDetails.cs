namespace GTAIVDowngrader.Classes.Json
{
    public class DowngradeFileDetails
    {

        #region Variables

        // Details
        public FileDetails FileDetails;

        #endregion

        #region Constructor
#if FILE_EDITOR_PROJ
        // copy constructor
        public DowngradeFileDetails(DowngradeFileDetails instance)
        {
            FileDetails = new FileDetails(instance.FileDetails);
        }
#endif
        // default constructor
        public DowngradeFileDetails()
        {
            FileDetails = new FileDetails();
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format(
                "FileDetails: {0}",
                FileDetails.ToString());
        }
        #endregion

    }
}
