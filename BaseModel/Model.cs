using System;
using System.Collections.Generic;
using System.Linq;
using OPTANO.Modeling.Optimization;
using OPTANO.Modeling.Optimization.Enums;
using OPTANO.Modeling.Optimization.Solver.Gurobi810;

namespace BasismodellBereitstellung.BaseModel
{
    public class Model : IDisposable
    {
        private readonly Asset[] assets;
        private readonly Component[] components;
        private readonly int[] periods;
        private readonly ModelScope scope;
        private readonly OPTANO.Modeling.Optimization.Model model;
        
        private VariableCollection<Asset, Component, int> z;
        private VariableCollection<Asset, Component, int> y;
        private VariableCollection<Asset, Component> x;
        private VariableCollection<Asset> v;

        public Model(Asset[] assets, Component[] components, int[] periods)
        {
            this.assets = assets;
            this.components = components;
            this.periods = periods;
            scope = new ModelScope();
            model = new OPTANO.Modeling.Optimization.Model();
            SetupModel();
        }

        public Model(Instance instance) : this(instance.Assets, instance.Components, instance.Periods)
        {}

        public void Dispose()
        {
            scope.Dispose();
        }

        private void SetupModel()
        {
            // Primary decision variables
            VariableType BinaryGenerator3D(Asset asset, Component component, int period) => VariableType.Binary;
            Func<Asset, Component, int, string> DebugNameGenerator3D(string varName) => (asset, component, period) => $"{varName}_{asset}_{component}_{period}";

            z = new VariableCollection<Asset, Component, int>(model, assets, components, periods,"z", variableTypeGenerator:BinaryGenerator3D, debugNameGenerator:DebugNameGenerator3D("z"));
            y = new VariableCollection<Asset,Component,int>(model, assets, components, periods,"y", variableTypeGenerator:BinaryGenerator3D, debugNameGenerator:DebugNameGenerator3D("y"));
            
            // Derived auxiliary variables
            x = new VariableCollection<Asset, Component>(model, assets, components, "X");
            v = new VariableCollection<Asset>(model, assets, "v");
            
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
                    
                    // Repair only after arrival
                    model.AddConstraint(Expression.Sum(NotAvailablePeriods(asset, component).Select(t => z[asset, component, t])) == 0);
                }
            }

            Expression RepairActive(Asset asset, Component component, int period)
            {
                return Expression.Sum(periods
                    .Where(tau => tau >= period && tau <= period + component.RepairDuration[asset] - 1)
                    .Select(tau => z[asset, component, tau]));
            }

            foreach (var component in components)
            {
                foreach (var period in periods)
                {
                    // No parallel repairs (capacity restriction, one machine per component)
                    model.AddConstraint(Expression.Sum(assets.Select(asset => RepairActive(asset, component, period))) <= 1);
                }
            }
        }

        private IEnumerable<int> NotAvailablePeriods(Asset asset, Component component)
        {
            return periods.Where(t => t < component.ReleaseDate[asset]);
        }

        private Solution ExtractSolution(OPTANO.Modeling.Optimization.Solution sol)
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
        
        public Solution Solve()
        {
            using var solver = new GurobiSolver();
            var solution = solver.Solve(model);
            return ExtractSolution(solution);
        }
    }
}