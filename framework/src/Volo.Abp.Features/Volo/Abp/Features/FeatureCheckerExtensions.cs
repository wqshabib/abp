﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Authorization;
using Volo.Abp.Threading;

namespace Volo.Abp.Features
{
    public static class FeatureCheckerExtensions
    {
        public static async Task<T> GetAsync<T>(
            [NotNull] this IFeatureChecker featureChecker, 
            [NotNull] string name, 
            T defaultValue = default)
            where T : struct
        {
            Check.NotNull(featureChecker, nameof(featureChecker));
            Check.NotNull(name, nameof(name));

            var value = await featureChecker.GetOrNullAsync(name);
            return value?.To<T>() ?? defaultValue;
        }

        public static string GetOrNull(
            [NotNull] this IFeatureChecker featureChecker, 
            [NotNull] string name)
        {
            Check.NotNull(featureChecker, nameof(featureChecker));
            return AsyncHelper.RunSync(() => featureChecker.GetOrNullAsync(name));
        }

        public static T Get<T>(
            [NotNull] this IFeatureChecker featureChecker, 
            [NotNull] string name, 
            T defaultValue = default)
            where T : struct
        {
            return AsyncHelper.RunSync(() => featureChecker.GetAsync(name, defaultValue));
        }

        public static bool IsEnabled(
            [NotNull] this IFeatureChecker featureChecker, 
            [NotNull] string name)
        {
            return AsyncHelper.RunSync(() => featureChecker.IsEnabledAsync(name));
        }

        public static async Task<bool> IsEnabledAsync(this IFeatureChecker featureChecker, bool requiresAll, params string[] featureNames)
        {
            if (featureNames.IsNullOrEmpty())
            {
                return true;
            }

            if (requiresAll)
            {
                foreach (var featureName in featureNames)
                {
                    if (!(await featureChecker.IsEnabledAsync(featureName)))
                    {
                        return false;
                    }
                }

                return true;
            }

            foreach (var featureName in featureNames)
            {
                if (await featureChecker.IsEnabledAsync(featureName))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsEnabled(this IFeatureChecker featureChecker, bool requiresAll, params string[] featureNames)
        {
            return AsyncHelper.RunSync(() => featureChecker.IsEnabledAsync(requiresAll, featureNames));
        }

        public static async Task CheckEnabledAsync(this IFeatureChecker featureChecker, string featureName)
        {
            if (!(await featureChecker.IsEnabledAsync(featureName)))
            {
                throw new AbpAuthorizationException("Feature is not enabled: " + featureName);
            }
        }

        public static void CheckEnabled(this IFeatureChecker featureChecker, string featureName)
        {
            if (!featureChecker.IsEnabled(featureName))
            {
                throw new AbpAuthorizationException("Feature is not enabled: " + featureName);
            }
        }

        public static async Task CheckEnabledAsync(this IFeatureChecker featureChecker, bool requiresAll, params string[] featureNames)
        {
            if (featureNames.IsNullOrEmpty())
            {
                return;
            }

            if (requiresAll)
            {
                foreach (var featureName in featureNames)
                {
                    if (!(await featureChecker.IsEnabledAsync(featureName)))
                    {
                        throw new AbpAuthorizationException(
                            "Required features are not enabled. All of these features must be enabled: " +
                            string.Join(", ", featureNames)
                        );
                    }
                }
            }
            else
            {
                foreach (var featureName in featureNames)
                {
                    if (await featureChecker.IsEnabledAsync(featureName))
                    {
                        return;
                    }
                }

                throw new AbpAuthorizationException(
                    "Required features are not enabled. At least one of these features must be enabled: " +
                    string.Join(", ", featureNames)
                );
            }
        }

        public static void CheckEnabled(this IFeatureChecker featureChecker, bool requiresAll, params string[] featureNames)
        {
            AsyncHelper.RunSync(() => featureChecker.CheckEnabledAsync(requiresAll, featureNames));
        }
    }
}