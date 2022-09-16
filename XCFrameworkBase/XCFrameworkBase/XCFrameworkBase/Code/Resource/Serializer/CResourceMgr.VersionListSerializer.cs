using System;
using System.IO;
using System.Text;

namespace XCFrameworkBase
{
    public static class XX
    {
        private static readonly byte[] ms_CachedBytes = new byte[byte.MaxValue + 1];
        public static void WriteEncryptedString(this BinaryWriter binaryWriter, string value, byte[] encryptBytes)
        {
            if (string.IsNullOrEmpty(value))
            {
                binaryWriter.Write((byte)0);
                return;
            }

            int length = CUtility.Converter.GetBytes(value, ms_CachedBytes);
            if (length > byte.MaxValue)
            {
                throw new GameFrameworkException(CUtility.Text.Format("String '{0}' is too long.", value));
            }

            CUtility.Encryption.GetSelfXorBytes(ms_CachedBytes, encryptBytes);
            binaryWriter.Write((byte)length);
            binaryWriter.Write(ms_CachedBytes, 0, length);
        }

        public static string ReadEncryptedString(this BinaryReader binaryReader, byte[] encryptBytes)
        {
            byte length = binaryReader.ReadByte();
            if (length <= 0)
            {
                return null;
            }

            if (length > byte.MaxValue)
            {
                throw new GameFrameworkException("String is too long.");
            }

            for (byte i = 0; i < length; i++)
            {
                ms_CachedBytes[i] = binaryReader.ReadByte();
            }

            CUtility.Encryption.GetSelfXorBytes(ms_CachedBytes, 0, length, encryptBytes);
            string value = CUtility.Converter.GetString(ms_CachedBytes, 0, length);
            Array.Clear(ms_CachedBytes, 0, length);
            return value;
        }
    }

    public partial class CResourceMgr
    {
        public static class CVersionListSerializer
        {
            private const int CachedHashBytesLength = 4;
            private static readonly byte[] ms_arrCachedHashBytes = new byte[CachedHashBytesLength];



            public static bool PackageVersionListSerializeCallback_V0(Stream stream, SPackageVersionList versionList)
            {
                if (!versionList.IsValid)
                {
                    return false;
                }

                CUtility.Random.GetRandomBytes(ms_arrCachedHashBytes);
                using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8))
                {
                    binaryWriter.Write(ms_arrCachedHashBytes);
                    binaryWriter.WriteEncryptedString(versionList.ApplicableGameVersion, ms_arrCachedHashBytes);

                    binaryWriter.Write(versionList.InternalResourceVersion);
                    SPackageVersionList.SAsset[] arrAsset = versionList.GetAssets();
                    SPackageVersionList.SResource[] arrResource = versionList.GetResources();
                    binaryWriter.Write(arrAsset.Length);
                    binaryWriter.Write(arrResource.Length);

                    foreach (var res in arrResource)
                    {
                        binaryWriter.WriteEncryptedString(res.Name, ms_arrCachedHashBytes);
                        binaryWriter.WriteEncryptedString(res.Variant, ms_arrCachedHashBytes);
                        binaryWriter.Write(res.LoadType);
                        binaryWriter.Write(res.Len);
                        binaryWriter.Write(res.HashCode);
                        int[] arrAssetIdxs = res.GetAssetIdxes();
                        binaryWriter.Write(arrAssetIdxs.Length);
                        byte[] hashBytes = new byte[CachedHashBytesLength];
                        CUtility.Converter.GetBytes(res.HashCode, hashBytes);
                        foreach (int nAssetIdx in arrAssetIdxs)
                        {
                            SPackageVersionList.SAsset assetInfo = arrAsset[nAssetIdx];
                            binaryWriter.WriteEncryptedString(assetInfo.Name, hashBytes);
                            int[] arrDependAssetIdxs = assetInfo.GetDependAssetIdxes();
                            binaryWriter.Write(arrDependAssetIdxs.Length);
                            foreach (int nDependAssetIdx in arrDependAssetIdxs)
                            {
                                binaryWriter.WriteEncryptedString(arrAsset[nDependAssetIdx].Name, hashBytes);
                            }
                        }
                    }

                    SPackageVersionList.SResourceGroup[] arrResGroup = versionList.GetResourceGroups();
                    binaryWriter.Write(arrResGroup.Length);
                    foreach (var resGroup in arrResGroup)
                    {
                        binaryWriter.WriteEncryptedString(resGroup.Name, ms_arrCachedHashBytes);
                        int[] arrResIdx = resGroup.GetResourceIdxes();
                        binaryWriter.Write(arrResIdx.Length);
                        foreach (var nResIdx in arrResIdx)
                        {
                            binaryWriter.Write(nResIdx);
                        }
                    }
                }
                Array.Clear(ms_arrCachedHashBytes, 0, CachedHashBytesLength);
                return true;
            }


            public static SPackageVersionList PackageVersionListDeserializeCallback_V0(Stream stream)
            {
                using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
                {
                    byte[] arrEncryBytes = binaryReader.ReadBytes(CachedHashBytesLength);
                    s
                }
                using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
                {
                    byte[] encryptBytes = binaryReader.ReadBytes(CachedHashBytesLength);
                    string applicableGameVersion = binaryReader.ReadEncryptedString(encryptBytes);
                    int internalResourceVersion = binaryReader.ReadInt32();
                    int assetCount = binaryReader.ReadInt32();
                    PackageVersionList.Asset[] assets = assetCount > 0 ? new PackageVersionList.Asset[assetCount] : null;
                    int resourceCount = binaryReader.ReadInt32();
                    PackageVersionList.Resource[] resources = resourceCount > 0 ? new PackageVersionList.Resource[resourceCount] : null;
                    string[][] resourceToAssetNames = new string[resourceCount][];
                    List<KeyValuePair<string, string[]>> assetNameToDependencyAssetNames = new List<KeyValuePair<string, string[]>>(assetCount);
                    for (int i = 0; i < resourceCount; i++)
                    {
                        string name = binaryReader.ReadEncryptedString(encryptBytes);
                        string variant = binaryReader.ReadEncryptedString(encryptBytes);
                        byte loadType = binaryReader.ReadByte();
                        int length = binaryReader.ReadInt32();
                        int hashCode = binaryReader.ReadInt32();
                        Utility.Converter.GetBytes(hashCode, s_CachedHashBytes);

                        int assetNameCount = binaryReader.ReadInt32();
                        string[] assetNames = new string[assetNameCount];
                        for (int j = 0; j < assetNameCount; j++)
                        {
                            assetNames[j] = binaryReader.ReadEncryptedString(s_CachedHashBytes);
                            int dependencyAssetNameCount = binaryReader.ReadInt32();
                            string[] dependencyAssetNames = dependencyAssetNameCount > 0 ? new string[dependencyAssetNameCount] : null;
                            for (int k = 0; k < dependencyAssetNameCount; k++)
                            {
                                dependencyAssetNames[k] = binaryReader.ReadEncryptedString(s_CachedHashBytes);
                            }

                            assetNameToDependencyAssetNames.Add(new KeyValuePair<string, string[]>(assetNames[j], dependencyAssetNames));
                        }

                        resourceToAssetNames[i] = assetNames;
                        resources[i] = new PackageVersionList.Resource(name, variant, null, loadType, length, hashCode, assetNameCount > 0 ? new int[assetNameCount] : null);
                    }

                    assetNameToDependencyAssetNames.Sort(AssetNameToDependencyAssetNamesComparer);
                    Array.Clear(s_CachedHashBytes, 0, CachedHashBytesLength);
                    int index = 0;
                    foreach (KeyValuePair<string, string[]> i in assetNameToDependencyAssetNames)
                    {
                        if (i.Value != null)
                        {
                            int[] dependencyAssetIndexes = new int[i.Value.Length];
                            for (int j = 0; j < i.Value.Length; j++)
                            {
                                dependencyAssetIndexes[j] = GetAssetNameIndex(assetNameToDependencyAssetNames, i.Value[j]);
                            }

                            assets[index++] = new PackageVersionList.Asset(i.Key, dependencyAssetIndexes);
                        }
                        else
                        {
                            assets[index++] = new PackageVersionList.Asset(i.Key, null);
                        }
                    }

                    for (int i = 0; i < resources.Length; i++)
                    {
                        int[] assetIndexes = resources[i].GetAssetIndexes();
                        for (int j = 0; j < assetIndexes.Length; j++)
                        {
                            assetIndexes[j] = GetAssetNameIndex(assetNameToDependencyAssetNames, resourceToAssetNames[i][j]);
                        }
                    }

                    int resourceGroupCount = binaryReader.ReadInt32();
                    PackageVersionList.ResourceGroup[] resourceGroups = resourceGroupCount > 0 ? new PackageVersionList.ResourceGroup[resourceGroupCount] : null;
                    for (int i = 0; i < resourceGroupCount; i++)
                    {
                        string name = binaryReader.ReadEncryptedString(encryptBytes);
                        int resourceIndexCount = binaryReader.ReadInt32();
                        int[] resourceIndexes = resourceIndexCount > 0 ? new int[resourceIndexCount] : null;
                        for (int j = 0; j < resourceIndexCount; j++)
                        {
                            resourceIndexes[j] = binaryReader.ReadUInt16();
                        }

                        resourceGroups[i] = new PackageVersionList.ResourceGroup(name, resourceIndexes);
                    }

                    return new PackageVersionList(applicableGameVersion, internalResourceVersion, assets, resources, null, resourceGroups);
                }
            }

        }
    }
}
