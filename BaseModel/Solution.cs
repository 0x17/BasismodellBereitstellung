namespace BasismodellBereitstellung.BaseModel
{
    /// <summary>
    /// Different types of actions in the base model.
    /// </summary>
    public enum ActionChoice
    {
        Repair,
        Order
    }
    
    public class Solution
    {
        /// <summary>
        /// Starting period of the action for the (assetIndex,componentIndex) pair.
        /// </summary>
        public readonly int[,] ComponentActionStart;
        /// <summary>
        /// Action type for the (assetIndex,componentIndex) pair.
        /// </summary>
        public readonly ActionChoice[,] ComponentChoice;
        
        /// <summary>
        /// Solution object from Optano with infos from the solver.
        /// </summary>
        public readonly OPTANO.Modeling.Optimization.Solution SolutionObj;

        /// <summary>
        /// Constructor for a solution object.
        /// </summary>
        /// <param name="componentActionStart">Starting period of (assetIndex,componentIndex) action for individual component.</param>
        /// <param name="componentChoice">Chosen action type for (assetIndex,componentIndex) individual component.</param>
        /// <param name="solutionObj">Optano solution object.</param>
        public Solution(int[,] componentActionStart, ActionChoice[,] componentChoice, OPTANO.Modeling.Optimization.Solution solutionObj)
        {
            ComponentActionStart = componentActionStart;
            ComponentChoice = componentChoice;
            SolutionObj = solutionObj;
        }
    }
}