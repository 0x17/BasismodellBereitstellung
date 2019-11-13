using System.Collections.Generic;
using System.Linq;

namespace BasismodellBereitstellung.BaseModel
{
    /// <summary>
    /// A component (module, stage) of a complex good (asset). Some attributes are specific to the parent asset, like
    /// the release date (arrival after disassembling) and repair duration in time units (periods).
    /// Components typically arrive from the outmost to the most central component, meaning that for example in an
    /// aircraft turbine the fan arrives first.
    /// </summary>
    public class Component
    {
        /// <summary>
        /// First period when the component from a specific asset is available. 
        /// </summary>
        public Dictionary<Asset, int> ReleaseDate;
        
        /// <summary>
        /// Duration of the repair of the component from a specific asset in number of periods.  
        /// </summary>
        public readonly Dictionary<Asset, int> RepairDuration;
        
        /// <summary>
        /// Order duration of the component in "good as new"-state in number of periods.
        /// </summary>
        public readonly int OrderDuration;
        
        /// <summary>
        /// Cost of one unit of this component in "good as new"-state in monetary units. 
        /// </summary>
        public readonly double OrderCosts;

        /// <summary>
        /// Constructor with asset specific values in arrays.
        /// </summary>
        /// <param name="assets">Complex goods.</param>
        /// <param name="releaseDates">Release dates of this component for each good (asset).</param>
        /// <param name="repairDurations">Repair durations of this component for each good (asset).</param>
        /// <param name="orderDuration">Asset independent duration of one unit order of this component.</param>
        /// <param name="orderCosts">Cost of one unit order of this component in monetary units.</param>
        public Component(Asset[] assets, int[] releaseDates, int[] repairDurations, int orderDuration, double orderCosts)
        {
            ReleaseDate = new Dictionary<Asset, int>(releaseDates.Select((rd, i) => new KeyValuePair<Asset,int>(assets[i], rd))); 
            RepairDuration = new Dictionary<Asset, int>(repairDurations.Select((rd, i) => new KeyValuePair<Asset,int>(assets[i], rd))); 
            OrderDuration = orderDuration;
            OrderCosts = orderCosts;
        }
        
        public Component(Dictionary<Asset, int> releaseDate, Dictionary<Asset, int> repairDuration, int orderDuration, double orderCosts)
        {
            ReleaseDate = releaseDate;
            RepairDuration = repairDuration;
            OrderDuration = orderDuration;
            OrderCosts = orderCosts;
        }
    }
}