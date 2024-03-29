﻿using Loxifi;
using Penguin.Persistence.Abstractions;
using Penguin.Persistence.Abstractions.Interfaces;
using Penguin.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Penguin.Cms.Core.Extensions
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    public static class IServiceProviderExtensions
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    {
        private static readonly ConcurrentDictionary<Type, Type> RepositoryTypes = new();

        /// <summary>
        /// Returns the most derived repository for the provided type
        /// </summary>
        /// <param name="serviceProvider">The service provider source</param>
        /// <param name="t">The type to get the repository for</param>
        /// <returns>An instance of that repository from the service provider</returns>
        public static IRepository GetRepositoryForType(this IServiceProvider serviceProvider, Type t)
        {
            return serviceProvider is null
                ? throw new ArgumentNullException(nameof(serviceProvider))
                : serviceProvider.GetRepositoryForType<IRepository>(t);
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
                List<Type> Implementations = t.IsGenericType
                    ? TypeFactory.Default.GetAllImplementations(typeof(T).MakeGenericType(t)).ToList()
                    : TypeFactory.Default.GetAllImplementations(typeof(IRepository<>).MakeGenericType(t)).Where(rt => typeof(T).IsAssignableFrom(rt)).ToList();
                if (Implementations.Count == 1)
                {
                    _ = RepositoryTypes.TryAdd(t, Implementations.Single());
                    TypedRepository = (T)serviceProvider.GetService(Implementations.Single());
                }
                else if (Implementations.Count > 1)
                {
                    throw new Exception($"Multiple implementations found for repository type {typeof(T).MakeGenericType(t)}");
                }
                else
                {
                    Type searchType = t;

                    List<Type> GenericRepositories = TypeFactory.Default
                                                    .GetAllImplementations(typeof(T))
                                                    .Where(rt => rt.ContainsGenericParameters)
                                                    .Where(rt => rt.GetGenericArguments()[0].GetGenericParameterConstraints()[0].IsAssignableFrom(searchType))
                                                    .ToList();

                    List<Type> Constraints = GenericRepositories.Select(rt => rt.GetGenericArguments()[0].GetGenericParameterConstraints()[0]).Distinct().ToList();

                    Type MostDerivedConstraint = TypeFactory.Default.GetMostDerivedType(Constraints, typeof(KeyedObject));

                    GenericRepositories = GenericRepositories
                                          .Where(rt => MostDerivedConstraint.IsAssignableFrom(rt.GetGenericArguments()[0].GetGenericParameterConstraints()[0]))
                                          .Select(rt => rt.MakeGenericType(searchType))
                                          .ToList();

                    Type DerivedRepositoryGeneric = TypeFactory.Default.GetMostDerivedType(GenericRepositories, typeof(T));
                    do
                    {
                        if (searchType is null)
                        {
                            throw new NullReferenceException();
                        }

                        TypedRepository = (T)serviceProvider.GetService(DerivedRepositoryGeneric);

                        searchType = searchType.BaseType;
                    } while (searchType != typeof(object) && TypedRepository == null);

                    _ = RepositoryTypes.TryAdd(t, TypedRepository.GetType());
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