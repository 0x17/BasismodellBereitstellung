using System;
using System.Linq;
using BasismodellBereitstellung.BaseModel;
using Model = BasismodellBereitstellung.BaseModel.Model;

namespace BasismodellBereitstellung
{
    class Program
    {
        static Instance CreateInstance()
        {
            Asset[] assets =
            {
                new Asset(10, 0.5),
                new Asset(5, 1.0)
            };

            Component[] components =
            {
                new Component(assets, new[] {1, 2}, new[] {3, 4}, 1, 3.0),
                new Component(assets, new[] {3, 4}, new[] {5, 2}, 1, 2.5),
                new Component(assets, new[] {5, 6}, new[] {2, 8}, 2, 2.0)
            };

            const int numPeriods = 100;
            var periods = Enumerable.Range(0, numPeriods).ToArray();
            return new Instance(assets, components, periods);
        }
        
        static void PrintSolution(Instance instance, Solution sol)
        {
            for (int k = 0; k < instance.Components.Length; k++)
            {
                Console.WriteLine();
                for (int i = 0; i < instance.Assets.Length; i++)
                {
                    Console.WriteLine($"Asset {i+1}, Component {k+1}, Choice = {sol.ComponentChoice[i, k]}, Start = {sol.ComponentActionStart[i, k]}");
                }
            }

            Console.WriteLine($"\nSolution status = {sol.SolutionObj.Status} and model status = {sol.SolutionObj.ModelStatus}");
        }
        
        static void Main(string[] args)
        {
            var instance = CreateInstance();
            var model = new Model(instance);
            var sol = model.Solve();
            PrintSolution(instance, sol);
        }
    }
}