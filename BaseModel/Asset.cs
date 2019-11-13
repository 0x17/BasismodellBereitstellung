namespace BasismodellBereitstellung.BaseModel
{
    /// <summary>
    /// Complex good built out of multiple components. The good must be repaired before the due date. Is the delivery
    /// date after the due date, then contractual penalty costs must be paid for each delayed period.
    /// </summary>
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