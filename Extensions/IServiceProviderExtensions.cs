using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Persistence.Abstractions.Models.Base;
using Penguin.Persistence.Repositories.Interfaces;
using Penguin.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Penguin.Cms.Core.Extensions
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public static class IServiceProviderExtensions
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        private static readonly ConcurrentDictionary<Type, Type> RepositoryTypes = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Returns the most derived repository for the provided type
        /// </summary>
        /// <typeparam name="T">The repository implementation to provide</typeparam>
        /// <param name="serviceProvider">The service provider source</param>
        /// <param name="t">The type to get the repository for</param>
        /// <returns>An instance of that repository from the service provider</returns>
        public static IRepository GetRepositoryForType(this IServiceProvider serviceProvider, Type t)
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            return serviceProvider.GetRepositoryForType<IRepository>(t);
        }

        /// <summary>
        /// Returns the most derived repository for the provided type
        /// </summary>
        /// <typeparam name="T">The repository implementation to provide</typeparam>
        /// <param name="serviceProvider">The service provider source</param>
        /// <param name="t">The type to get the repository for</param>
        /// <returns>An instance of that repository from the service provider</returns>
        public static T GetRepositoryForType<T>(this IServiceProvider serviceProvider, Type t) where T : IRepository
        {
            if (serviceProvider is null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (t is null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            T TypedRepository;

            if (!RepositoryTypes.TryGetValue(t, out Type RepositoryType))
            {

                List<Type> Implementations;

                if (t.IsGenericType)
                {
                    Implementations = TypeFactory.GetAllImplementations(typeof(T).MakeGenericType(t)).ToList();
                }
                else
                {
                    Implementations = TypeFactory.GetAllImplementations(typeof(IRepository<>).MakeGenericType(t)).Where(rt => typeof(T).IsAssignableFrom(rt)).ToList();
                }

                if (Implementations.Count == 1)
                {
                    RepositoryTypes.TryAdd(t, Implementations.Single());
                    TypedRepository = (T)serviceProvider.GetService(Implementations.Single());
                }
                else if (Implementations.Count > 1)
                {
                    throw new Exception($"Multiple implementations found for repository type {typeof(T).MakeGenericType(t)}");
                }
                else
                {

                    Type searchType = t;

                    List<Type> GenericRepositories = TypeFactory
                                                    .GetAllImplementations(typeof(T))
                                                    .Where(rt => rt.ContainsGenericParameters)
                                                    .Where(rt => rt.GetGenericArguments()[0].GetGenericParameterConstraints()[0].IsAssignableFrom(searchType))
                                                    .ToList();

                    List<Type> Constraints = GenericRepositories.Select(rt => rt.GetGenericArguments()[0].GetGenericParameterConstraints()[0]).Distinct().ToList();

                    Type MostDerivedConstraint = TypeFactory.GetMostDerivedType(Constraints, typeof(KeyedObject));

                    GenericRepositories = GenericRepositories
                                          .Where(rt => MostDerivedConstraint.IsAssignableFrom(rt.GetGenericArguments()[0].GetGenericParameterConstraints()[0]))
                                          .Select(rt => rt.MakeGenericType(searchType))
                                          .ToList();

                    Type DerivedRepositoryGeneric = TypeFactory.GetMostDerivedType(GenericRepositories, typeof(T));
                    do
                    {
                        if (searchType is null)
                        {
                            throw new NullReferenceException();
                        }

                        TypedRepository = (T)serviceProvider.GetService(DerivedRepositoryGeneric);

                        searchType = searchType.BaseType;
                    } while (searchType != typeof(object) && TypedRepository == null);

                    RepositoryTypes.TryAdd(t, TypedRepository.GetType());
                }
            }
            else
            {
                TypedRepository = (T)serviceProvider.GetService(RepositoryType);
            }

            return TypedRepository;
        }
    }
}