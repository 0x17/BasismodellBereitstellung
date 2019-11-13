namespace BasismodellBereitstellung.DataModel
{
    public class Asset
    {
        /// <summary>
        /// Contractual due (delivery) date of the asset.
        /// </summary>
        public readonly int DueDate;
        
        /// <summary>
        /// Contractual penalty cost per time period delay when missing the due date. 
        /// </summary>
        public readonly double DelayCosts;

        public Asset(int dueDate, double delayCosts)
        {
            DueDate = dueDate;
            DelayCosts = delayCosts;
        }
    }
}