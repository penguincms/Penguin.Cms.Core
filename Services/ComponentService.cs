using Penguin.Cms.Abstractions.Interfaces;
using Penguin.DependencyInjection.Abstractions.Interfaces;
using Penguin.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Cms.Core.Services
{
    public class ComponentService : ISelfRegistering
    {
        private IServiceProvider ServiceProvider { get; set; }

        public ComponentService(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public IEnumerable<TReturn> GetComponents<TReturn, TParameter>(TParameter Id)
        {
            foreach (Type t in TypeFactory.GetAllImplementations<IProvideComponents<TReturn, TParameter>>())
            {
                if ((this.ServiceProvider.GetService(t) ?? Activator.CreateInstance(t)) is IProvideComponents<TReturn, TParameter> provider)
                {
                    foreach (TReturn o in provider.GetComponents(Id).OfType<TReturn>())
                    {
                        yield return o;
                    }
                }
            }
        }

        public IEnumerable<TReturn> GetComponents<TReturn>()
        {
            foreach (Type t in TypeFactory.GetAllImplementations<IProvideComponents<TReturn>>())
            {
                if ((this.ServiceProvider.GetService(t) ?? Activator.CreateInstance(t)) is IProvideComponents<TReturn> provider)
                {
                    foreach (TReturn o in provider.GetComponents().OfType<TReturn>())
                    {
                        yield return o;
                    }
                }
            }
        }
    }
}