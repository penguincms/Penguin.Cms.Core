using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Penguin.Cms.Core.Interfaces
{
    /// <summary>
    /// Generates default entities of a specifit type
    /// </summary>
    /// <typeparam name="T">The type this generator creates</typeparam>
    public interface IGenerateDefaultEntities<T> : IGenerateDefaultEntities
    {
        /// <summary>
        /// Returns an IEnumerable of generated entities
        /// </summary>
        /// <returns>An IEnumerable of generated entities</returns>
        new IEnumerable<T> Generate();
    }
}