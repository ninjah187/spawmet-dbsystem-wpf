using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF
{
    public class ModelElementComparer : IEqualityComparer<IModelElement>
    {
        public bool Equals(IModelElement x, IModelElement y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(IModelElement x)
        {
            return x.GetHashCode();
        }

        //public int Compare(IModelElement element1, IModelElement element2)
        //{
        //    if (element1.Id < element2.Id)
        //    {
        //        return -1;
        //    }
        //    if (element1.Id == element2.Id)
        //    {
        //        return 0;
        //    }
        //    if (element1.Id > element2.Id)
        //    {
        //        return 1;
        //    }

        //    throw new InvalidOperationException("Comparing failed.");
        //}
    }
}
