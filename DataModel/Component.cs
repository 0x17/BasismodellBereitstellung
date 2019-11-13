using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;

namespace BasismodellBereitstellung.DataModel
{
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