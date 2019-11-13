using System;
using System.Linq;
using BasismodellBereitstellung.BaseModel;
using Model = BasismodellBereitstellung.BaseModel.Model;

namespace BasismodellBereitstellung
{
    class Program
    {
        static void Main(string[] args)
        {
            Instance CreateInstance()
            {
                Asset[] assets =
                {
                    new Asset(10, 0.5),
                    new Asset(5, 1.0)
                };

                Component[] components =
                {
                    new Component(assets, new[] {1, 2}, new[] {3, 4}, 5, 0.5),
                    new Component(assets, new[] {3, 4}, new[] {2, 2}, 3, 0.5),
                    new Component(assets, new[] {3, 4}, new[] {2, 2}, 3, 0.5)
                };

                const int numPeriods = 100;
                var periods = Enumerable.Range(0, numPeriods).ToArray();
                return new Instance(assets, components, periods);
            }
            
            void PrintSolution(Instance instance, Solution sol)
            {
                for (int i = 0; i < instance.Assets.Length; i++)
                    for (int k = 0; k < instance.Components.Length; k++)
                        Console.WriteLine($"Asset {i}, Component {k}, Choice = {sol.ComponentChoice[i, k]}, Start = {sol.ComponentActionStart[i, k]}");
                Console.WriteLine($"Solution status = {sol.SolutionObj.Status} and model status = {sol.SolutionObj.ModelStatus}");
            }
            
            var instance = CreateInstance();
            var model = new Model(instance);
            var sol = model.Solve();
            PrintSolution(instance, sol);
        }
    }
}