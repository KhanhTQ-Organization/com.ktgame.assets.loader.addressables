using com.ktgame.assets.loader.core;
using UnityEngine;

namespace com.ktgame.assets.loader.addressables
{
	[CreateAssetMenu(fileName = "AddressableAssetLoader", menuName = "Ktgame/Asset Loader/Addressable Asset Loader")]
	public class AddressableAssetLoaderObject : AssetLoaderObject, IAssetLoader
	{
		private readonly AddressableAssetLoader _loader = new AddressableAssetLoader();

		public override AssetRequest<TAsset> Load<TAsset>(string address)
		{
			return _loader.Load<TAsset>(address);
		}

		public override AssetRequest<Object> Load(string address)
		{
			return _loader.Load(address);
		}

		public override AssetRequest<TAsset> LoadAsync<TAsset>(string address)
		{
			return _loader.LoadAsync<TAsset>(address);
		}

		public override AssetRequest<Object> LoadAsync(string address)
		{
			return _loader.LoadAsync(address);
		}

		public override void Release(AssetRequest request)
		{
			_loader.Release(request);
		}
	}
}
