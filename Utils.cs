using System;
using System.Collections.Generic;
using System.Linq;
using OPTANO.Modeling.Optimization;

namespace BasismodellBereitstellung
{
    public static class Utils
    {
        public static Expression Sum3D<T1, T2, T3>(IEnumerable<T1> t1, IEnumerable<T2> t2, IEnumerable<T3> t3,
            Func<T1, T2, T3, Expression> f)
        {
            return Expression.Sum(t1.Select(t1Elem =>
                Expression.Sum(t2.Select(t2Elem =>
                    Expression.Sum(t3.Select(t3Elem =>
                        f(t1Elem, t2Elem, t3Elem)))))));
        }
    }
}