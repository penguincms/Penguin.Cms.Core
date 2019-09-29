using System;
using System.Collections.Generic;
using System.Text;

namespace Penguin.Cms.Core.Interfaces
{
    public interface IGenerateDefaultEntities<T>
    {
        IEnumerable<T> Generate();
    }
}