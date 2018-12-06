﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalEngine
{
    /// <summary>
    /// a package 
    /// we can load or unload resource
    /// </summary>
    public class PackageProvider : GameObject
    {
        private PackageComponent mPackageComponent;
        private LogComponent mLogComponent;

        public string Path { get; }
        public string FullPath => GetFullPath();

        internal Asset LoadAsset(string name, List<Asset> dependentAssets)
        {
            mLogComponent.Log(StringGroup.Log + "[Load Asset] [Name = {0}].", name);

            return mPackageComponent.LoadAsset(FullPath, name, dependentAssets);
        }

        internal Asset LoadAssetRange(string name, int start, int size, List<Asset> dependentAssets)
        {
            mLogComponent.Log(StringGroup.Log + "[Load Asset Range] [Name = {0}].", name);

            return mPackageComponent.LoadAssetRange(FullPath, name, start, size, dependentAssets);
        }

        internal void UnLoadAsset(string name)
        {
            mLogComponent.Log(StringGroup.Log + "[UnLoad Asset] [Name = {0}].", name);

            mPackageComponent.UnLoadAsset(name);
        }

        internal void AddAsset(Asset asset)
        {
            mLogComponent.Log(StringGroup.Log + "[Add Asset] [Type = {0}] [Name = {1}].", asset.GetType(), asset.Name);

            if (asset.Package != null)
            {
                mLogComponent.Log(StringGroup.Warning + "[Add Asset and Change the Asset's Package] [Type = {0}] [Name = {1}].",
                    asset.GetType(), asset.Name);

                asset.Package.RemoveAsset(asset);
            }

            asset.Package = this;

            mPackageComponent.AddAsset(asset);
        }

        internal void RemoveAsset(Asset asset)
        {
            mLogComponent.Log(StringGroup.Log + "[Remove Asset] [Type = {0}] [Name = {1}].", asset.GetType(), asset.Name);

            asset.Package = null;

            mPackageComponent.RemoveAsset(asset);
        }

        public PackageProvider(string name, string path) : base(name)
        {
            AddComponent(mPackageComponent = new PackageComponent());
            AddComponent(mLogComponent = new LogComponent(name));

            Path = path;

            mLogComponent.Log(StringGroup.Log + "[Initialize PackageProvider Finish].");
        }

        public Asset GetAsset(string name)
        {
            return mPackageComponent.GetAsset(name);
        }

        public string GetFullPath()
        {
            if (GetType() != typeof(PackageProvider)) return "";

            return (Parent as PackageProvider)?.GetFullPath() + "/" + Path;
        }
    }
}