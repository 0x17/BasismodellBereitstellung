namespace BasismodellBereitstellung.DataModel
{
    public enum ActionChoice
    {
        Repair,
        Order
    }
    
    public class Solution
    {
        public readonly int[,] ComponentActionStart;
        public readonly ActionChoice[,] ComponentChoice;
        public readonly OPTANO.Modeling.Optimization.Solution SolutionObj;

        public Solution(int[,] componentActionStart, ActionChoice[,] componentChoice, OPTANO.Modeling.Optimization.Solution solutionObj)
        {
            ComponentActionStart = componentActionStart;
            ComponentChoice = componentChoice;
            SolutionObj = solutionObj;
        }
    }
}