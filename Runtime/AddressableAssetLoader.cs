using System;
using System.Collections.Generic;
using com.ktgame.assets.loader.core;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace com.ktgame.assets.loader.addressables
{
	public sealed class AddressableAssetLoader : IAssetLoader
	{
		private int _nextRequestId;
		private readonly Dictionary<int, AsyncOperationHandle> _requestHandles = new Dictionary<int, AsyncOperationHandle>();

		public AssetRequest<TAsset> Load<TAsset>(string address) where TAsset : Object
		{
			var requestId = _nextRequestId++;
			var addressableHandle = Addressables.LoadAssetAsync<TAsset>(address);
			addressableHandle.WaitForCompletion();
			_requestHandles.Add(requestId, addressableHandle);
			var request = new AssetRequest<TAsset>(requestId);
			var setter = (IAssetRequest<TAsset>)request;
			setter.SetProgressFunc(() => addressableHandle.PercentComplete);
			
			if (addressableHandle.Status == AsyncOperationStatus.Succeeded)
			{
				setter.SetTask(UniTask.FromResult(addressableHandle.Result));
				setter.SetResult(addressableHandle.Result);
				setter.SetStatus(AssetRequestStatus.Succeeded);
			}
			else
			{
				setter.SetTask(UniTask.FromException<TAsset>(addressableHandle.OperationException ?? new Exception("Addressable load failed.")));
				setter.SetResult(null);
				setter.SetStatus(AssetRequestStatus.Failed);
				setter.SetOperationException(addressableHandle.OperationException);
			}
			
			return request;
		}

		public AssetRequest<Object> Load(string address)
		{
			return Load<Object>(address);
		}

		public AssetRequest<TAsset> LoadAsync<TAsset>(string address) where TAsset : Object
		{
			var requestId = _nextRequestId++;
			var addressableHandle = Addressables.LoadAssetAsync<TAsset>(address);
			_requestHandles.Add(requestId, addressableHandle);
			var handle = new AssetRequest<TAsset>(requestId);
			var setter = (IAssetRequest<TAsset>)handle;
			var utcs = new UniTaskCompletionSource<TAsset>();
			
			addressableHandle.Completed += x =>
			{
				if (!_requestHandles.ContainsKey(requestId))
				{
					utcs.TrySetCanceled();
					return;
				}

				if (x.Status == AsyncOperationStatus.Failed)
				{
					setter.SetStatus(AssetRequestStatus.Failed);
					setter.SetOperationException(x.OperationException);
					setter.SetResult(null);
					utcs.TrySetException(x.OperationException ?? new Exception("Addressable load failed."));
					return;
				}

				setter.SetResult(x.Result);
				setter.SetStatus(AssetRequestStatus.Succeeded);
				utcs.TrySetResult(x.Result);
			};

			setter.SetProgressFunc(() => addressableHandle.PercentComplete);
			setter.SetTask(utcs.Task);
			return handle;
		}

		public AssetRequest<Object> LoadAsync(string address)
		{
			return LoadAsync<Object>(address);
		}

		public void Release(AssetRequest request)
		{
			if (!_requestHandles.TryGetValue(request.RequestId, out var addressableHandle))
			{
				throw new InvalidOperationException($"There is no asset that has been requested for release (RequestId: {request.RequestId}).");
			}

			_requestHandles.Remove(request.RequestId);
			Addressables.Release(addressableHandle);
		}
	}
}
