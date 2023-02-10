using Penguin.Cms.Abstractions;
using Penguin.Cms.Abstractions.Interfaces;
using Penguin.Reflection;
using System;
using System.Collections.Generic;

namespace Penguin.Cms.Core.Services
{
    public class MacroService
    {
        protected IServiceProvider ServiceProvider { get; set; }

        public MacroService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public List<Macro> GetMacros(object requester)
        {
            List<IMacroProvider> MacroHandlers = new();

            IEnumerable<Type> MacroHandlerTypes = TypeFactory.GetAllImplementations(typeof(IMacroProvider));

            foreach (Type thisHandlerType in MacroHandlerTypes)
            {
                if (ServiceProvider.GetService(thisHandlerType) is IMacroProvider m)
                {
                    MacroHandlers.Add(m);
                }
            }

            List<Macro> toReturn = new();

            foreach (IMacroProvider thisHandler in MacroHandlers)
            {
                toReturn.AddRange(thisHandler.GetMacros(requester));
            }

            return toReturn;
        }
    }
}