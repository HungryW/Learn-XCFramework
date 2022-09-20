using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static XCFrameworkBase.SVersionList;

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
            private const string DefaultExtension = "dat";
            private const int ms_nCachedHashBytesLength = 4;
            private static readonly byte[] ms_arrCachedHashBytes = new byte[ms_nCachedHashBytesLength];

            private static int _AssetNameToDependencyAssetNamesComparer(KeyValuePair<string, string[]> a, KeyValuePair<string, string[]> b)
            {
                return a.Key.CompareTo(b.Key);
            }

            private static int _GetAssetNameIndex(List<KeyValuePair<string, string[]>> assetNameToDependencyAssetNames, string assetName)
            {
                return GetAssetNameIndexWithBinarySearch(assetNameToDependencyAssetNames, assetName, 0, assetNameToDependencyAssetNames.Count - 1);
            }

            private static int GetAssetNameIndexWithBinarySearch(List<KeyValuePair<string, string[]>> assetNameToDependencyAssetNames, string assetName, int leftIndex, int rightIndex)
            {
                if (leftIndex > rightIndex)
                {
                    return -1;
                }

                int middleIndex = (leftIndex + rightIndex) / 2;
                if (assetNameToDependencyAssetNames[middleIndex].Key == assetName)
                {
                    return middleIndex;
                }

                if (assetNameToDependencyAssetNames[middleIndex].Key.CompareTo(assetName) > 0)
                {
                    return GetAssetNameIndexWithBinarySearch(assetNameToDependencyAssetNames, assetName, leftIndex, middleIndex - 1);
                }
                else
                {
                    return GetAssetNameIndexWithBinarySearch(assetNameToDependencyAssetNames, assetName, middleIndex + 1, rightIndex);
                }
            }

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
                    SVersionList.SAsset[] arrAsset = versionList.GetAssets();
                    SVersionList.SResource[] arrResource = versionList.GetResources();
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
                        byte[] hashBytes = new byte[ms_nCachedHashBytesLength];
                        CUtility.Converter.GetBytes(res.HashCode, hashBytes);
                        foreach (int nAssetIdx in arrAssetIdxs)
                        {
                            SVersionList.SAsset assetInfo = arrAsset[nAssetIdx];
                            binaryWriter.WriteEncryptedString(assetInfo.Name, hashBytes);
                            int[] arrDependAssetIdxs = assetInfo.GetDependAssetIdxes();
                            binaryWriter.Write(arrDependAssetIdxs.Length);
                            foreach (int nDependAssetIdx in arrDependAssetIdxs)
                            {
                                binaryWriter.WriteEncryptedString(arrAsset[nDependAssetIdx].Name, hashBytes);
                            }
                        }
                    }

                    SVersionList.SResourceGroup[] arrResGroup = versionList.GetResourceGroups();
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
                Array.Clear(ms_arrCachedHashBytes, 0, ms_nCachedHashBytesLength);
                return true;
            }

            public static SPackageVersionList PackageVersionListDeserializeCallback_V0(Stream stream)
            {
                using (BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8))
                {
                    byte[] arrEncryBytes = binaryReader.ReadBytes(ms_nCachedHashBytesLength);
                    string szAppGameVersion = binaryReader.ReadEncryptedString(arrEncryBytes);
                    int nInternalResVersion = binaryReader.ReadInt32();
                    int nAssetCount = binaryReader.ReadInt32();
                    SVersionList.SAsset[] arrAsset = nAssetCount > 0 ? new SVersionList.SAsset[nAssetCount] : null;
                    int nResCount = binaryReader.ReadInt32();
                    SVersionList.SResource[] arrRes = nResCount > 0 ? new SVersionList.SResource[nResCount] : null;
                    string[][] arrResToAssetNames = new string[nResCount][];
                    List<KeyValuePair<string, string[]>> listAssetNameToDependNames = new List<KeyValuePair<string, string[]>>(nAssetCount);
                    for (int i = 0; i < nResCount; i++)
                    {
                        string szName = binaryReader.ReadEncryptedString(arrEncryBytes);
                        string szVariant = binaryReader.ReadEncryptedString(arrEncryBytes);
                        byte eLoadType = binaryReader.ReadByte();
                        int nLen = binaryReader.ReadInt32();
                        int nHashCode = binaryReader.ReadInt32();
                        CUtility.Converter.GetBytes(nHashCode, ms_arrCachedHashBytes);

                        int nResContainAssetCount = binaryReader.ReadInt32();
                        string[] arrAssetNames = new string[nResContainAssetCount];
                        for (int j = 0; j < nResContainAssetCount; j++)
                        {
                            arrAssetNames[j] = binaryReader.ReadEncryptedString(ms_arrCachedHashBytes);
                            int nDependAssetNameCount = binaryReader.ReadInt32();
                            string[] arrDependAssetName = nDependAssetNameCount > 0 ? new string[nDependAssetNameCount] : null;
                            for (int k = 0; k < nDependAssetNameCount; k++)
                            {
                                arrDependAssetName[k] = binaryReader.ReadEncryptedString(ms_arrCachedHashBytes);
                            }
                            listAssetNameToDependNames.Add(new KeyValuePair<string, string[]>(arrAssetNames[j], arrDependAssetName));
                        }
                        arrResToAssetNames[i] = arrAssetNames;
                        arrRes[i] = new SVersionList.SResource(szName, szVariant, null, eLoadType, nLen, nHashCode, nLen, nHashCode, nResContainAssetCount > 0 ? new int[nResContainAssetCount] : null);
                    }
                    listAssetNameToDependNames.Sort((a, b) => { return a.Key.CompareTo(b.Key); });
                    Array.Clear(ms_arrCachedHashBytes, 0, ms_nCachedHashBytesLength);
                    int nIdx = 0;
                    foreach (var val in listAssetNameToDependNames)
                    {
                        if (val.Value != null)
                        {
                            int[] arrDependAssetIdx = new int[val.Value.Length];
                            for (int i = 0; i < val.Value.Length; i++)
                            {
                                arrDependAssetIdx[i] = _GetAssetNameIndex(listAssetNameToDependNames, val.Value[i]);
                            }
                            arrAsset[nIdx++] = new SVersionList.SAsset(val.Key, arrDependAssetIdx);
                        }
                        else
                        {
                            arrAsset[nIdx++] = new SVersionList.SAsset(val.Key, null);
                        }
                    }

                    for (int i = 0; i < arrRes.Length; i++)
                    {
                        int[] arrAssetIdxes = arrRes[i].GetAssetIdxes();
                        for (int j = 0; j < arrAssetIdxes.Length; j++)
                        {
                            arrAssetIdxes[j] = _GetAssetNameIndex(listAssetNameToDependNames, arrResToAssetNames[i][j]);
                        }
                    }
                    int nResourceGroupCount = binaryReader.ReadInt32();
                    SVersionList.SResourceGroup[] arrResourceGroups = nResourceGroupCount > 0 ? new SVersionList.SResourceGroup[nResourceGroupCount] : null;
                    for (int i = 0; i < nResourceGroupCount; i++)
                    {
                        string szName = binaryReader.ReadEncryptedString(arrEncryBytes);
                        int nGroupContainResCount = binaryReader.ReadInt32();
                        int[] arrResIndx = nGroupContainResCount > 0 ? new int[nGroupContainResCount] : null;
                        for (int j = 0; j < nGroupContainResCount; j++)
                        {
                            arrResIndx[j] = binaryReader.ReadUInt16();
                        }

                        arrResourceGroups[i] = new SVersionList.SResourceGroup(szName, arrResIndx);
                    }
                    return new SPackageVersionList(szAppGameVersion, nInternalResVersion, arrAsset, arrRes, null, arrResourceGroups);
                }
            }

        }
    }
}
