namespace GTAIVDowngrader.Classes
{
    internal class CommandLineArgument
    {
        #region Properties
        public int Category { get; private set; }
        public string ArgumentName { get; private set; }
        public string ArgumentDescription { get; private set; }
        #endregion

        #region Constructor
        public CommandLineArgument(int category, string aName, string aDesc)
        {
            Category = category;
            ArgumentName = aName;
            ArgumentDescription = aDesc;
        }
        #endregion
    }
}
