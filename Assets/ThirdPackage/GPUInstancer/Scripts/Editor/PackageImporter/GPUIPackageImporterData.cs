using System;
using System.Collections.Generic;
using UnityEngine;

namespace GPUInstancer
{
    public class GPUIPackageImporterData : ScriptableObject
    {
        public string domain = "com.mycompany.myasset";
        public PackageDefinition[] packageDefinitions;
        public List<ImportedPackageInfo> importedPackageInfos;
        [NonSerialized]
        public bool forceReimport;

        public bool Validate()
        {
            if (string.IsNullOrEmpty(domain))
            {
                Debug.LogError("Domain name is not provided!", this);
                return false;
            }
            for (int i = 0; i < packageDefinitions.Length; i++)
            {
                if (!packageDefinitions[i].Validate(this))
                    return false;
            }
            return true;
        }

        [Serializable]
        public struct PackageDefinition
        {
            public string packageName;
            public UnityEngine.Object packageToImport;
            [PIPackageToImportVersion]
            public string packageToImportVersion;
            public PackageCondition[] packageConditions;

            public bool Validate(GPUIPackageImporterData packageImporterData)
            {
                if (string.IsNullOrEmpty(packageName))
                {
                    Debug.LogError("Package name is not provided!", packageImporterData);
                    return false;
                }
                if (packageToImport == null)
                {
                    Debug.LogError("Package to import is not provided for " + packageName, packageImporterData);
                    return false;
                }
                if (string.IsNullOrEmpty(packageToImportVersion))
                {
                    Debug.LogError("Package version is not provided for " + packageName, packageImporterData);
                    return false;
                }
                if (!Version.TryParse(packageToImportVersion, out _))
                {
                    Debug.LogError("Package version is invalid for " + packageName, packageImporterData);
                    return false;
                }
                for (int i = 0; i < packageConditions.Length; i++)
                {
                    if (!packageConditions[i].Validate(packageImporterData))
                        return false;
                }
                return true;
            }
        }

        [Serializable]
        public struct PackageCondition
        {
            public PackageConditionType conditionType;
            [PIDependentPackageName]
            public string dependentPackageName;
            [PIDependentPackageExpression]
            public int dependentPackageExpression;
            [PIDependentPackageVersion]
            public string dependentPackageVersion;

            public bool Validate(GPUIPackageImporterData packageImporterData)
            {
                if (string.IsNullOrEmpty(dependentPackageName))
                {
                    Debug.LogError("Depended package is not provided for the condition.", packageImporterData);
                    return false;
                }
                if (conditionType == PackageConditionType.ScriptDefine)
                {
                    return true;
                }
                if (dependentPackageExpression < 3 && (string.IsNullOrEmpty(dependentPackageVersion) || !Version.TryParse(dependentPackageVersion, out _)))
                {
                    Debug.LogError("Depended package version is invalid for the condition.", packageImporterData);
                    return false;
                }
                return true;
            }
        }

        public enum PackageConditionType
        {
            UnityPackage = 0,
            ScriptDefine = 1
        }

        [Serializable]
        public struct ImportedPackageInfo
        {
            public string packageURL;
            public string importedVersion;
        }

        public class PIDependentPackageNameAttribute : PropertyAttribute { }
        public class PIDependentPackageExpressionAttribute : PropertyAttribute { }
        public class PIDependentPackageVersionAttribute : PropertyAttribute { }
        public class PIPackageToImportVersionAttribute : PropertyAttribute { }
    }
}