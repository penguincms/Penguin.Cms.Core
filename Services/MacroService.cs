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
            this.ServiceProvider = serviceProvider;
        }

        public List<Macro> GetMacros(object requester)
        {
            List<IMacroProvider> MacroHandlers = new List<IMacroProvider>();

            IEnumerable<Type> MacroHandlerTypes = TypeFactory.GetAllImplementations(typeof(IMacroProvider));

            foreach (Type thisHandlerType in MacroHandlerTypes)
            {
                if (this.ServiceProvider.GetService(thisHandlerType) is IMacroProvider m)
                {
                    MacroHandlers.Add(m);
                }
            }

            List<Macro> toReturn = new List<Macro>();

            foreach (IMacroProvider thisHandler in MacroHandlers)
            {
                toReturn.AddRange(thisHandler.GetMacros(requester));
            }

            return toReturn;
        }
    }
}