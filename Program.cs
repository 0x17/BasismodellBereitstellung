using System;
using System.Linq;
using BasismodellBereitstellung.DataModel;
using OPTANO.Modeling.Optimization;
using OPTANO.Modeling.Optimization.Enums;
using OPTANO.Modeling.Optimization.Solver.Gurobi810;
using Solution = BasismodellBereitstellung.DataModel.Solution;

namespace BasismodellBereitstellung
{
    class Program
    {
        public static Solution SolveBaseModel(Asset[] assets, Component[] components, int[] periods)
        {
            using var scope = new ModelScope();
            var model = new Model();
            
            // Primary decision variables
            VariableType BinaryGenerator3D(Asset asset, Component component, int period) => VariableType.Binary;
            Func<Asset, Component, int, string> DebugNameGenerator3D(string varName) => (asset, component, period) => $"{varName}_{asset}_{component}_{period}";

            var z = new VariableCollection<Asset, Component, int>(model, assets, components, periods,"z", variableTypeGenerator:BinaryGenerator3D, debugNameGenerator:DebugNameGenerator3D("z"));
            var y = new VariableCollection<Asset,Component,int>(model, assets, components, periods,"y", variableTypeGenerator:BinaryGenerator3D, debugNameGenerator:DebugNameGenerator3D("y"));
            
            // Derived auxiliary variables
            var x = new VariableCollection<Asset, Component>(model, assets, components, "X");
            var v = new VariableCollection<Asset>(model, assets, "v");

            void SetupModel()
            {
                // Objective function: Minimize the total costs from delays and orders
                var delayCosts = Expression.Sum(assets.Select(asset => asset.DelayCosts * v[asset]));
                var orderCosts = Utils.Sum3D(assets, components, periods,
                    (asset, component, period) => y[asset, component, period] * component.OrderCosts);
                model.AddObjective(new Objective(delayCosts + orderCosts, "totalCosts"));

                foreach (var asset in assets)
                {
                    foreach (var component in components)
                    {
                        // Compute delay from finish and due date
                        model.AddConstraint(v[asset] >= x[asset, component] - asset.DueDate, $"link_v{asset}_x{asset},{component}");

                        // Finish must be after repair end (if repaired)
                        var repairFinishPeriod = Expression.Sum(periods.Select(t => z[asset, component, t] * (t + component.RepairDuration[asset])));
                        model.AddConstraint(x[asset, component] >= repairFinishPeriod, $"finish_after_repair_{asset}_{component}");

                        // Finish must be after order arrival (if ordered)
                        var orderArrivedPeriod = Expression.Sum(periods.Select(t => y[asset, component, t] * (t + component.OrderDuration)));
                        model.AddConstraint(x[asset, component] >= orderArrivedPeriod, $"finish_after_order_{asset}_{component}");

                        // Repair xor order
                        model.AddConstraint(Expression.Sum(periods.Select(t => z[asset, component, t] + y[asset, component, t])) == 1, "repair_xor_order_{asset}_{component}");
                    }
                }
            }

            Solution ExtractSolution(OPTANO.Modeling.Optimization.Solution sol)
            {
                int StartPeriodFromVar(VariableCollection<Asset, Component, int> vname, int i, int k)
                {
                    return periods.Sum(period => (int)vname[assets[i], components[k], period].Value * period);
                }

                ActionChoice[,] componentChoice = new ActionChoice[assets.Length,components.Length];
                int[,] componentActionStart = new int[assets.Length,components.Length];
                for(int i=0; i<assets.Length; i++)
                {
                    for (int k=0; k < components.Length; k++)
                    {
                        ActionChoice choice = periods.Sum(period => z[assets[i], components[k], period].Value) > 0
                            ? ActionChoice.Repair
                            : ActionChoice.Order;
                        componentChoice[i,k] = choice;
                        switch (choice)
                        {
                            case ActionChoice.Repair:
                                componentActionStart[i,k] = StartPeriodFromVar(z, i, k);
                                break;
                            case ActionChoice.Order:
                                componentActionStart[i,k] = StartPeriodFromVar(y, i, k);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    
                    }
                }

                return new Solution(componentActionStart, componentChoice, sol);
            }
            
            SetupModel();
            using var solver = new GurobiSolver();
            var solution = solver.Solve(model);
            return ExtractSolution(solution);
        }

        static void Main(string[] args)
        {
            Asset[] assets = {
                new Asset(10, 0.5),
                new Asset(5, 1.0)
            };
            
            Component[] components =
            {
                new Component(assets, new[] {1,2}, new[] {3,4}, 5, 0.5), 
                new Component(assets, new[] {3,4}, new[] {2,2}, 3, 0.5)
            };
            
            const int numPeriods = 100;
            var periods = Enumerable.Range(0, numPeriods).ToArray();

            var sol = SolveBaseModel(assets, components, periods);
            
            for(int i=0; i<assets.Length; i++)
                for (int k = 0; k < components.Length; k++)
                    Console.WriteLine($"Asset {i}, Component {k}, Choice = {sol.ComponentChoice[i, k]}, Start = {sol.ComponentActionStart[i, k]}");
            Console.WriteLine($"Solution status = {sol.SolutionObj.Status} and model status = {sol.SolutionObj.ModelStatus}");
        }
    }
}