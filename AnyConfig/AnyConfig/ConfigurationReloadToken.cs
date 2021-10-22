using Microsoft.Extensions.Primitives;
using System;
using System.Threading;

namespace AnyConfig
{
    /// <summary>
    /// AnyConfig reload token
    /// </summary>
    public class ConfigurationReloadToken : IChangeToken
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        /// <summary>
        /// Indicates if this token will proactively raise callbacks. Callbacks are still guaranteed to be invoked, eventually.
        /// </summary>
        public bool ActiveChangeCallbacks => true;

        bool IChangeToken.ActiveChangeCallbacks => ActiveChangeCallbacks;

        /// <summary>
        /// Gets a value that indicates if a change has occurred.
        /// </summary>
        public bool HasChanged => _cts.IsCancellationRequested;
        bool IChangeToken.HasChanged => HasChanged;

        /// <summary>
        /// Registers for a callback that will be invoked when the entry has changed. <see cref="IChangeToken.HasChanged"/>
        /// MUST be set before the callback is invoked.
        /// </summary>
        /// <param name="callback">The callback to invoke.</param>
        /// <param name="state">State to be passed into the callback.</param>
        /// <returns></returns>
        public IDisposable RegisterChangeCallback(Action<object> callback, object state) => _cts.Token.Register(callback, state);

        IDisposable IChangeToken.RegisterChangeCallback(Action<object> callback, object state) => RegisterChangeCallback(callback, state);

        /// <summary>
        /// Used to trigger the change token when a reload occurs.
        /// </summary>
        public void OnReload() => _cts.Cancel();
    }
}
