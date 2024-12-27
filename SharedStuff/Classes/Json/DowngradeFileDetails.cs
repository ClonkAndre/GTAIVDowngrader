namespace GTAIVDowngrader.Classes.Json
{
    public class DowngradeFileDetails
    {

        #region Variables

        // Details
        public FileDetails FileDetails;
        public string Type;

        #endregion

        #region Constructor
        public DowngradeFileDetails(DowngradeFileDetails instance)
        {
            FileDetails = instance.FileDetails;
            Type = instance.Type;
        }
        public DowngradeFileDetails()
        {
            FileDetails = new FileDetails();
            Type = "";
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return string.Format(
                "FileDetails: {0}, " +
                "Type: {1}",
                FileDetails.ToString(),
                Type);
        }
        #endregion

    }
}
