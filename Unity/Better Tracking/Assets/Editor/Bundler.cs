using UnityEditor;

public class Bundler
{
	const string dir = "AssetBundles";
	const string extension = ".btk";

    [MenuItem("BetterTracking/Build Bundles")]
    static void BuildAllAssetBundles()
    {
		BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.ForceRebuildAssetBundle, BuildTarget.StandaloneWindows);

		FileUtil.ReplaceFile(dir + "/better_tracking_prefabs", dir + "/better_tracking_prefabs" + extension);

		FileUtil.DeleteFileOrDirectory(dir + "/better_tracking_prefabs");
	}


}
