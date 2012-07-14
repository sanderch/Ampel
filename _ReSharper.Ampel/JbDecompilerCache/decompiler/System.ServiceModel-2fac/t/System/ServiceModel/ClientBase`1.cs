// Type: System.ServiceModel.ClientBase`1
// Assembly: System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.ServiceModel.dll

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Diagnostics;
using System.ServiceModel.Dispatcher;
using System.Threading;

namespace System.ServiceModel
{
  public abstract class ClientBase<TChannel> : ICommunicationObject, IDisposable where TChannel : class
  {
    private static ChannelFactoryRefCache<TChannel> factoryRefCache = new ChannelFactoryRefCache<TChannel>(32);
    private static object staticLock = new object();
    private static AsyncCallback onAsyncCallCompleted = Fx.ThunkCallback(new AsyncCallback(ClientBase<TChannel>.OnAsyncCallCompleted));
    private bool canShareFactory = true;
    private object syncRoot = new object();
    private object finalizeLock = new object();
    private TChannel channel;
    private ChannelFactoryRef<TChannel> channelFactoryRef;
    private EndpointTrait<TChannel> endpointTrait;
    private bool useCachedFactory;
    private bool sharingFinalized;
    private bool channelFactoryRefReleased;
    private bool releasedLastRef;

    object ThisLock
    {
      private get
      {
        return this.syncRoot;
      }
    }

    protected TChannel Channel
    {
      get
      {
        if ((object) this.channel == null)
        {
          lock (this.ThisLock)
          {
            if ((object) this.channel == null)
            {
              using (ServiceModelActivity resource_0 = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : (ServiceModelActivity) null)
              {
                if (DiagnosticUtility.ShouldUseActivity)
                  ServiceModelActivity.Start(resource_0, System.ServiceModel.SR.GetString("ActivityOpenClientBase", new object[1]
                  {
                    (object) typeof (TChannel).FullName
                  }), ActivityType.OpenClient);
                if (this.useCachedFactory)
                {
                  try
                  {
                    this.CreateChannelInternal();
                  }
                  catch (Exception exception_0)
                  {
                    if (this.useCachedFactory && (exception_0 is CommunicationException || exception_0 is ObjectDisposedException || exception_0 is TimeoutException))
                    {
                      DiagnosticUtility.ExceptionUtility.TraceHandledException(exception_0, TraceEventType.Warning);
                      this.InvalidateCacheAndCreateChannel();
                    }
                    else
                      throw;
                  }
                }
                else
                  this.CreateChannelInternal();
              }
            }
          }
        }
        return this.channel;
      }
    }

    public ChannelFactory<TChannel> ChannelFactory
    {
      get
      {
        this.TryDisableSharing();
        return this.GetChannelFactory();
      }
    }

    public ClientCredentials ClientCredentials
    {
      get
      {
        this.TryDisableSharing();
        return this.ChannelFactory.Credentials;
      }
    }

    public CommunicationState State
    {
      get
      {
        IChannel channel = (IChannel) (object) this.channel;
        if (channel != null)
          return channel.State;
        if (!this.useCachedFactory)
          return this.GetChannelFactory().State;
        else
          return CommunicationState.Created;
      }
    }

    public IClientChannel InnerChannel
    {
      get
      {
        return (IClientChannel) (object) this.Channel;
      }
    }

    public ServiceEndpoint Endpoint
    {
      get
      {
        this.TryDisableSharing();
        return this.GetChannelFactory().Endpoint;
      }
    }

    event EventHandler ICommunicationObject.Closed
    {
      add
      {
        this.InnerChannel.Closed += value;
      }
      remove
      {
        this.InnerChannel.Closed -= value;
      }
    }

    event EventHandler ICommunicationObject.Closing
    {
      add
      {
        this.InnerChannel.Closing += value;
      }
      remove
      {
        this.InnerChannel.Closing -= value;
      }
    }

    event EventHandler ICommunicationObject.Faulted
    {
      add
      {
        this.InnerChannel.Faulted += value;
      }
      remove
      {
        this.InnerChannel.Faulted -= value;
      }
    }

    event EventHandler ICommunicationObject.Opened
    {
      add
      {
        this.InnerChannel.Opened += value;
      }
      remove
      {
        this.InnerChannel.Opened -= value;
      }
    }

    event EventHandler ICommunicationObject.Opening
    {
      add
      {
        this.InnerChannel.Opening += value;
      }
      remove
      {
        this.InnerChannel.Opening -= value;
      }
    }

    static ClientBase()
    {
    }

    protected ClientBase()
    {
      this.endpointTrait = new EndpointTrait<TChannel>("*", (EndpointAddress) null, (InstanceContext) null);
      this.InitializeChannelFactoryRef();
    }

    protected ClientBase(string endpointConfigurationName)
    {
      if (endpointConfigurationName == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
      this.endpointTrait = new EndpointTrait<TChannel>(endpointConfigurationName, (EndpointAddress) null, (InstanceContext) null);
      this.InitializeChannelFactoryRef();
    }

    protected ClientBase(string endpointConfigurationName, string remoteAddress)
    {
      if (endpointConfigurationName == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
      if (remoteAddress == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");
      this.endpointTrait = new EndpointTrait<TChannel>(endpointConfigurationName, new EndpointAddress(remoteAddress), (InstanceContext) null);
      this.InitializeChannelFactoryRef();
    }

    protected ClientBase(string endpointConfigurationName, EndpointAddress remoteAddress)
    {
      if (endpointConfigurationName == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
      if (remoteAddress == (EndpointAddress) null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");
      this.endpointTrait = new EndpointTrait<TChannel>(endpointConfigurationName, remoteAddress, (InstanceContext) null);
      this.InitializeChannelFactoryRef();
    }

    protected ClientBase(Binding binding, EndpointAddress remoteAddress)
    {
      if (binding == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
      if (remoteAddress == (EndpointAddress) null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");
      this.channelFactoryRef = new ChannelFactoryRef<TChannel>(new ChannelFactory<TChannel>(binding, remoteAddress));
      this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
      this.TryDisableSharing();
    }

    protected ClientBase(ServiceEndpoint endpoint)
    {
      if (endpoint == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
      this.channelFactoryRef = new ChannelFactoryRef<TChannel>(new ChannelFactory<TChannel>(endpoint));
      this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
      this.TryDisableSharing();
    }

    protected ClientBase(InstanceContext callbackInstance)
    {
      if (callbackInstance == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
      this.endpointTrait = new EndpointTrait<TChannel>("*", (EndpointAddress) null, callbackInstance);
      this.InitializeChannelFactoryRef();
    }

    protected ClientBase(InstanceContext callbackInstance, string endpointConfigurationName)
    {
      if (callbackInstance == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
      if (endpointConfigurationName == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
      this.endpointTrait = new EndpointTrait<TChannel>(endpointConfigurationName, (EndpointAddress) null, callbackInstance);
      this.InitializeChannelFactoryRef();
    }

    protected ClientBase(InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress)
    {
      if (callbackInstance == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
      if (endpointConfigurationName == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
      if (remoteAddress == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");
      this.endpointTrait = new EndpointTrait<TChannel>(endpointConfigurationName, new EndpointAddress(remoteAddress), callbackInstance);
      this.InitializeChannelFactoryRef();
    }

    protected ClientBase(InstanceContext callbackInstance, string endpointConfigurationName, EndpointAddress remoteAddress)
    {
      if (callbackInstance == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
      if (endpointConfigurationName == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
      if (remoteAddress == (EndpointAddress) null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");
      this.endpointTrait = new EndpointTrait<TChannel>(endpointConfigurationName, remoteAddress, callbackInstance);
      this.InitializeChannelFactoryRef();
    }

    protected ClientBase(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
    {
      if (callbackInstance == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
      if (binding == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
      if (remoteAddress == (EndpointAddress) null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");
      this.channelFactoryRef = new ChannelFactoryRef<TChannel>((ChannelFactory<TChannel>) new DuplexChannelFactory<TChannel>(callbackInstance, binding, remoteAddress));
      this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
      this.TryDisableSharing();
    }

    protected ClientBase(InstanceContext callbackInstance, ServiceEndpoint endpoint)
    {
      if (callbackInstance == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
      if (endpoint == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
      this.channelFactoryRef = new ChannelFactoryRef<TChannel>((ChannelFactory<TChannel>) new DuplexChannelFactory<TChannel>(callbackInstance, endpoint));
      this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
      this.TryDisableSharing();
    }

    protected T GetDefaultValueForInitialization<T>()
    {
      return default (T);
    }

    public void Open()
    {
      this.Open(this.GetChannelFactory().InternalOpenTimeout);
    }

    public void Abort()
    {
      IChannel channel = (IChannel) (object) this.channel;
      if (channel != null)
        channel.Abort();
      if (!this.channelFactoryRefReleased)
      {
        lock (ClientBase<TChannel>.staticLock)
        {
          if (!this.channelFactoryRefReleased)
          {
            if (this.channelFactoryRef.Release())
              this.releasedLastRef = true;
            this.channelFactoryRefReleased = true;
          }
        }
      }
      if (!this.releasedLastRef)
        return;
      this.channelFactoryRef.Abort();
    }

    public void Close()
    {
      this.Close(this.GetChannelFactory().InternalCloseTimeout);
    }

    public void DisplayInitializationUI()
    {
      this.InnerChannel.DisplayInitializationUI();
    }

    private void CreateChannelInternal()
    {
      try
      {
        this.channel = this.CreateChannel();
        if (!this.sharingFinalized || !this.canShareFactory || this.useCachedFactory)
          return;
        this.TryAddChannelFactoryToCache();
      }
      finally
      {
        if (!this.sharingFinalized)
          this.TryDisableSharing();
      }
    }

    protected virtual TChannel CreateChannel()
    {
      if (this.sharingFinalized)
        return this.GetChannelFactory().CreateChannel();
      lock (this.finalizeLock)
      {
        this.sharingFinalized = true;
        return this.GetChannelFactory().CreateChannel();
      }
    }

    void IDisposable.Dispose()
    {
      this.Close();
    }

    void ICommunicationObject.Open(TimeSpan timeout)
    {
      TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
      if (!this.useCachedFactory)
        this.GetChannelFactory().Open(timeoutHelper.RemainingTime());
      this.InnerChannel.Open(timeoutHelper.RemainingTime());
    }

    void ICommunicationObject.Close(TimeSpan timeout)
    {
      using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : (ServiceModelActivity) null)
      {
        if (DiagnosticUtility.ShouldUseActivity)
          ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityCloseClientBase", new object[1]
          {
            (object) typeof (TChannel).FullName
          }), ActivityType.Close);
        TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
        if ((object) this.channel != null)
          this.InnerChannel.Close(timeoutHelper.RemainingTime());
        if (this.channelFactoryRefReleased)
          return;
        lock (ClientBase<TChannel>.staticLock)
        {
          if (!this.channelFactoryRefReleased)
          {
            if (this.channelFactoryRef.Release())
              this.releasedLastRef = true;
            this.channelFactoryRefReleased = true;
          }
        }
        if (!this.releasedLastRef)
          return;
        if (this.useCachedFactory)
          this.channelFactoryRef.Abort();
        else
          this.channelFactoryRef.Close(timeoutHelper.RemainingTime());
      }
    }

    IAsyncResult ICommunicationObject.BeginClose(AsyncCallback callback, object state)
    {
      return this.BeginClose(this.GetChannelFactory().InternalCloseTimeout, callback, state);
    }

    IAsyncResult ICommunicationObject.BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
    {
      return (IAsyncResult) new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.BeginChannelClose), new ChainedEndHandler(this.EndChannelClose), new ChainedBeginHandler(this.BeginFactoryClose), new ChainedEndHandler(this.EndFactoryClose));
    }

    void ICommunicationObject.EndClose(IAsyncResult result)
    {
      ChainedAsyncResult.End(result);
    }

    IAsyncResult ICommunicationObject.BeginOpen(AsyncCallback callback, object state)
    {
      return this.BeginOpen(this.GetChannelFactory().InternalOpenTimeout, callback, state);
    }

    IAsyncResult ICommunicationObject.BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
    {
      return (IAsyncResult) new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.BeginFactoryOpen), new ChainedEndHandler(this.EndFactoryOpen), new ChainedBeginHandler(this.BeginChannelOpen), new ChainedEndHandler(this.EndChannelOpen));
    }

    void ICommunicationObject.EndOpen(IAsyncResult result)
    {
      ChainedAsyncResult.End(result);
    }

    internal IAsyncResult BeginFactoryOpen(TimeSpan timeout, AsyncCallback callback, object state)
    {
      if (this.useCachedFactory)
        return (IAsyncResult) new CompletedAsyncResult(callback, state);
      else
        return this.GetChannelFactory().BeginOpen(timeout, callback, state);
    }

    internal void EndFactoryOpen(IAsyncResult result)
    {
      if (this.useCachedFactory)
        CompletedAsyncResult.End(result);
      else
        this.GetChannelFactory().EndOpen(result);
    }

    internal IAsyncResult BeginChannelOpen(TimeSpan timeout, AsyncCallback callback, object state)
    {
      return this.InnerChannel.BeginOpen(timeout, callback, state);
    }

    internal void EndChannelOpen(IAsyncResult result)
    {
      this.InnerChannel.EndOpen(result);
    }

    internal IAsyncResult BeginFactoryClose(TimeSpan timeout, AsyncCallback callback, object state)
    {
      if (this.useCachedFactory)
        return (IAsyncResult) new CompletedAsyncResult(callback, state);
      else
        return this.GetChannelFactory().BeginClose(timeout, callback, state);
    }

    internal void EndFactoryClose(IAsyncResult result)
    {
      if (typeof (CompletedAsyncResult).IsAssignableFrom(result.GetType()))
        CompletedAsyncResult.End(result);
      else
        this.GetChannelFactory().EndClose(result);
    }

    internal IAsyncResult BeginChannelClose(TimeSpan timeout, AsyncCallback callback, object state)
    {
      if ((object) this.channel != null)
        return this.InnerChannel.BeginClose(timeout, callback, state);
      else
        return (IAsyncResult) new CompletedAsyncResult(callback, state);
    }

    internal void EndChannelClose(IAsyncResult result)
    {
      if (typeof (CompletedAsyncResult).IsAssignableFrom(result.GetType()))
        CompletedAsyncResult.End(result);
      else
        this.InnerChannel.EndClose(result);
    }

    private ChannelFactory<TChannel> GetChannelFactory()
    {
      return this.channelFactoryRef.ChannelFactory;
    }

    private void InitializeChannelFactoryRef()
    {
      lock (ClientBase<TChannel>.staticLock)
      {
        ChannelFactoryRef<TChannel> local_0;
        if (ClientBase<TChannel>.factoryRefCache.TryGetValue(this.endpointTrait, out local_0))
        {
          if (local_0.ChannelFactory.State != CommunicationState.Opened)
          {
            ClientBase<TChannel>.factoryRefCache.Remove(this.endpointTrait);
          }
          else
          {
            this.channelFactoryRef = local_0;
            this.channelFactoryRef.AddRef();
            this.useCachedFactory = true;
            return;
          }
        }
      }
      if (this.channelFactoryRef != null)
        return;
      this.channelFactoryRef = ClientBase<TChannel>.CreateChannelFactoryRef(this.endpointTrait);
    }

    private static ChannelFactoryRef<TChannel> CreateChannelFactoryRef(EndpointTrait<TChannel> endpointTrait)
    {
      ChannelFactory<TChannel> channelFactory = endpointTrait.CreateChannelFactory();
      channelFactory.TraceOpenAndClose = false;
      return new ChannelFactoryRef<TChannel>(channelFactory);
    }

    private void TryDisableSharing()
    {
      if (this.sharingFinalized)
        return;
      lock (this.finalizeLock)
      {
        if (this.sharingFinalized)
          return;
        this.canShareFactory = false;
        this.sharingFinalized = true;
        if (!this.useCachedFactory)
          return;
        ChannelFactoryRef<TChannel> local_0 = this.channelFactoryRef;
        this.channelFactoryRef = ClientBase<TChannel>.CreateChannelFactoryRef(this.endpointTrait);
        this.useCachedFactory = false;
        lock (ClientBase<TChannel>.staticLock)
        {
          if (!local_0.Release())
            local_0 = (ChannelFactoryRef<TChannel>) null;
        }
        if (local_0 == null)
          return;
        local_0.Abort();
      }
    }

    private void TryAddChannelFactoryToCache()
    {
      lock (ClientBase<TChannel>.staticLock)
      {
        ChannelFactoryRef<TChannel> local_0;
        if (ClientBase<TChannel>.factoryRefCache.TryGetValue(this.endpointTrait, out local_0))
          return;
        this.channelFactoryRef.AddRef();
        ClientBase<TChannel>.factoryRefCache.Add(this.endpointTrait, this.channelFactoryRef);
        this.useCachedFactory = true;
      }
    }

    private void InvalidateCacheAndCreateChannel()
    {
      this.RemoveFactoryFromCache();
      this.TryDisableSharing();
      this.CreateChannelInternal();
    }

    private void RemoveFactoryFromCache()
    {
      lock (ClientBase<TChannel>.staticLock)
      {
        ChannelFactoryRef<TChannel> local_0;
        if (!ClientBase<TChannel>.factoryRefCache.TryGetValue(this.endpointTrait, out local_0) || !object.ReferenceEquals((object) this.channelFactoryRef, (object) local_0))
          return;
        ClientBase<TChannel>.factoryRefCache.Remove(this.endpointTrait);
      }
    }

    protected void InvokeAsync(ClientBase<TChannel>.BeginOperationDelegate beginOperationDelegate, object[] inValues, ClientBase<TChannel>.EndOperationDelegate endOperationDelegate, SendOrPostCallback operationCompletedCallback, object userState)
    {
      if (beginOperationDelegate == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("beginOperationDelegate");
      if (endOperationDelegate == null)
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endOperationDelegate");
      ClientBase<TChannel>.AsyncOperationContext context = new ClientBase<TChannel>.AsyncOperationContext(AsyncOperationManager.CreateOperation(userState), endOperationDelegate, operationCompletedCallback);
      Exception error = (Exception) null;
      object[] results = (object[]) null;
      IAsyncResult result = (IAsyncResult) null;
      try
      {
        result = beginOperationDelegate(inValues, ClientBase<TChannel>.onAsyncCallCompleted, (object) context);
        if (result.CompletedSynchronously)
          results = endOperationDelegate(result);
      }
      catch (Exception ex)
      {
        if (Fx.IsFatal(ex))
          throw;
        else
          error = ex;
      }
      if (error == null && !result.CompletedSynchronously)
        return;
      ClientBase<TChannel>.CompleteAsyncCall(context, results, error);
    }

    private static void OnAsyncCallCompleted(IAsyncResult result)
    {
      if (result.CompletedSynchronously)
        return;
      ClientBase<TChannel>.AsyncOperationContext context = (ClientBase<TChannel>.AsyncOperationContext) result.AsyncState;
      Exception error = (Exception) null;
      object[] results = (object[]) null;
      try
      {
        results = context.EndDelegate(result);
      }
      catch (Exception ex)
      {
        if (Fx.IsFatal(ex))
          throw;
        else
          error = ex;
      }
      ClientBase<TChannel>.CompleteAsyncCall(context, results, error);
    }

    private static void CompleteAsyncCall(ClientBase<TChannel>.AsyncOperationContext context, object[] results, Exception error)
    {
      if (context.CompletionCallback != null)
      {
        ClientBase<TChannel>.InvokeAsyncCompletedEventArgs completedEventArgs = new ClientBase<TChannel>.InvokeAsyncCompletedEventArgs(results, error, false, context.AsyncOperation.UserSuppliedState);
        context.AsyncOperation.PostOperationCompleted(context.CompletionCallback, (object) completedEventArgs);
      }
      else
        context.AsyncOperation.OperationCompleted();
    }

    protected delegate IAsyncResult BeginOperationDelegate<TChannel>(object[] inValues, AsyncCallback asyncCallback, object state) where TChannel : class;

    protected delegate object[] EndOperationDelegate<TChannel>(IAsyncResult result) where TChannel : class;

    protected class InvokeAsyncCompletedEventArgs : AsyncCompletedEventArgs
    {
      private object[] results;

      public object[] Results
      {
        get
        {
          return this.results;
        }
      }

      internal InvokeAsyncCompletedEventArgs(object[] results, Exception error, bool cancelled, object userState)
        : base(error, cancelled, userState)
      {
        this.results = results;
      }
    }

    private class AsyncOperationContext
    {
      private AsyncOperation asyncOperation;
      private ClientBase<TChannel>.EndOperationDelegate endDelegate;
      private SendOrPostCallback completionCallback;

      internal AsyncOperation AsyncOperation
      {
        get
        {
          return this.asyncOperation;
        }
      }

      internal ClientBase<TChannel>.EndOperationDelegate EndDelegate
      {
        get
        {
          return this.endDelegate;
        }
      }

      internal SendOrPostCallback CompletionCallback
      {
        get
        {
          return this.completionCallback;
        }
      }

      internal AsyncOperationContext(AsyncOperation asyncOperation, ClientBase<TChannel>.EndOperationDelegate endDelegate, SendOrPostCallback completionCallback)
      {
        this.asyncOperation = asyncOperation;
        this.endDelegate = endDelegate;
        this.completionCallback = completionCallback;
      }
    }

    protected class ChannelBase<T> : IClientChannel, IContextChannel, IExtensibleObject<IContextChannel>, IDisposable, IOutputChannel, IRequestChannel, IChannel, ICommunicationObject, IChannelBaseProxy where T : class
    {
      private ServiceChannel channel;
      private ImmutableClientRuntime runtime;

      bool IClientChannel.AllowInitializationUI
      {
        get
        {
          return this.channel.AllowInitializationUI;
        }
        set
        {
          this.channel.AllowInitializationUI = value;
        }
      }

      bool IClientChannel.DidInteractiveInitialization
      {
        get
        {
          return this.channel.DidInteractiveInitialization;
        }
      }

      Uri IClientChannel.Via
      {
        get
        {
          return this.channel.Via;
        }
      }

      bool IContextChannel.AllowOutputBatching
      {
        get
        {
          return this.channel.AllowOutputBatching;
        }
        set
        {
          this.channel.AllowOutputBatching = value;
        }
      }

      IInputSession IContextChannel.InputSession
      {
        get
        {
          return this.channel.InputSession;
        }
      }

      EndpointAddress IContextChannel.LocalAddress
      {
        get
        {
          return this.channel.LocalAddress;
        }
      }

      TimeSpan IContextChannel.OperationTimeout
      {
        get
        {
          return this.channel.OperationTimeout;
        }
        set
        {
          this.channel.OperationTimeout = value;
        }
      }

      IOutputSession IContextChannel.OutputSession
      {
        get
        {
          return this.channel.OutputSession;
        }
      }

      EndpointAddress IContextChannel.RemoteAddress
      {
        get
        {
          return this.channel.RemoteAddress;
        }
      }

      string IContextChannel.SessionId
      {
        get
        {
          return this.channel.SessionId;
        }
      }

      CommunicationState ICommunicationObject.State
      {
        get
        {
          return this.channel.State;
        }
      }

      IExtensionCollection<IContextChannel> IExtensibleObject<IContextChannel>.Extensions
      {
        get
        {
          return this.channel.Extensions;
        }
      }

      Uri IOutputChannel.Via
      {
        get
        {
          return this.channel.Via;
        }
      }

      EndpointAddress IOutputChannel.RemoteAddress
      {
        get
        {
          return this.channel.RemoteAddress;
        }
      }

      Uri IRequestChannel.Via
      {
        get
        {
          return this.channel.Via;
        }
      }

      EndpointAddress IRequestChannel.RemoteAddress
      {
        get
        {
          return this.channel.RemoteAddress;
        }
      }

      event EventHandler<UnknownMessageReceivedEventArgs> IClientChannel.UnknownMessageReceived
      {
        add
        {
          this.channel.UnknownMessageReceived += value;
        }
        remove
        {
          this.channel.UnknownMessageReceived -= value;
        }
      }

      event EventHandler ICommunicationObject.Closed
      {
        add
        {
          this.channel.Closed += value;
        }
        remove
        {
          this.channel.Closed -= value;
        }
      }

      event EventHandler ICommunicationObject.Closing
      {
        add
        {
          this.channel.Closing += value;
        }
        remove
        {
          this.channel.Closing -= value;
        }
      }

      event EventHandler ICommunicationObject.Faulted
      {
        add
        {
          this.channel.Faulted += value;
        }
        remove
        {
          this.channel.Faulted -= value;
        }
      }

      event EventHandler ICommunicationObject.Opened
      {
        add
        {
          this.channel.Opened += value;
        }
        remove
        {
          this.channel.Opened -= value;
        }
      }

      event EventHandler ICommunicationObject.Opening
      {
        add
        {
          this.channel.Opening += value;
        }
        remove
        {
          this.channel.Opening -= value;
        }
      }

      protected ChannelBase(ClientBase<T> client)
      {
        if (client.Endpoint.Address == (EndpointAddress) null)
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelFactoryEndpointAddressUri")));
        ChannelFactory<T> channelFactory = client.ChannelFactory;
        channelFactory.EnsureOpened();
        this.channel = channelFactory.ServiceChannelFactory.CreateServiceChannel(client.Endpoint.Address, client.Endpoint.Address.Uri);
        this.channel.InstanceContext = channelFactory.CallbackInstance;
        this.runtime = this.channel.ClientRuntime.GetRuntime();
      }

      [SecuritySafeCritical]
      protected IAsyncResult BeginInvoke(string methodName, object[] args, AsyncCallback callback, object state)
      {
        object[] args1 = new object[args.Length + 2];
        Array.Copy((Array) args, (Array) args1, args.Length);
        args1[args1.Length - 2] = (object) callback;
        args1[args1.Length - 1] = state;
        IMethodCallMessage methodCall = (IMethodCallMessage) new ClientBase<TChannel>.ChannelBase<T>.MethodCallMessage(args1);
        ProxyOperationRuntime operationByName = this.GetOperationByName(methodName);
        object[] ins = operationByName.MapAsyncBeginInputs(methodCall, out callback, out state);
        return this.channel.BeginCall(operationByName.Action, operationByName.IsOneWay, operationByName, ins, callback, state);
      }

      [SecuritySafeCritical]
      protected object EndInvoke(string methodName, object[] args, IAsyncResult result)
      {
        object[] args1 = new object[args.Length + 1];
        Array.Copy((Array) args, (Array) args1, args.Length);
        args1[args1.Length - 1] = (object) result;
        IMethodCallMessage methodCall = (IMethodCallMessage) new ClientBase<TChannel>.ChannelBase<T>.MethodCallMessage(args1);
        ProxyOperationRuntime operationByName = this.GetOperationByName(methodName);
        object[] outs;
        operationByName.MapAsyncEndInputs(methodCall, out result, out outs);
        object ret = this.channel.EndCall(operationByName.Action, outs, result);
        object[] objArray = operationByName.MapAsyncOutputs(methodCall, outs, ref ret);
        if (objArray != null)
          Array.Copy((Array) objArray, (Array) args, args.Length);
        return ret;
      }

      private ProxyOperationRuntime GetOperationByName(string methodName)
      {
        ProxyOperationRuntime operationByName = this.runtime.GetOperationByName(methodName);
        if (operationByName != null)
          return operationByName;
        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotSupportedException(System.ServiceModel.SR.GetString("SFxMethodNotSupported1", new object[1]
        {
          (object) methodName
        })));
      }

      void IClientChannel.DisplayInitializationUI()
      {
        this.channel.DisplayInitializationUI();
      }

      IAsyncResult IClientChannel.BeginDisplayInitializationUI(AsyncCallback callback, object state)
      {
        return this.channel.BeginDisplayInitializationUI(callback, state);
      }

      void IClientChannel.EndDisplayInitializationUI(IAsyncResult result)
      {
        this.channel.EndDisplayInitializationUI(result);
      }

      TProperty IChannel.GetProperty<TProperty>()
      {
        return this.channel.GetProperty<TProperty>();
      }

      void ICommunicationObject.Abort()
      {
        this.channel.Abort();
      }

      void ICommunicationObject.Close()
      {
        this.channel.Close();
      }

      void ICommunicationObject.Close(TimeSpan timeout)
      {
        this.channel.Close(timeout);
      }

      IAsyncResult ICommunicationObject.BeginClose(AsyncCallback callback, object state)
      {
        return this.channel.BeginClose(callback, state);
      }

      IAsyncResult ICommunicationObject.BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
      {
        return this.channel.BeginClose(timeout, callback, state);
      }

      void ICommunicationObject.EndClose(IAsyncResult result)
      {
        this.channel.EndClose(result);
      }

      void ICommunicationObject.Open()
      {
        this.channel.Open();
      }

      void ICommunicationObject.Open(TimeSpan timeout)
      {
        this.channel.Open(timeout);
      }

      IAsyncResult ICommunicationObject.BeginOpen(AsyncCallback callback, object state)
      {
        return this.channel.BeginOpen(callback, state);
      }

      IAsyncResult ICommunicationObject.BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
      {
        return this.channel.BeginOpen(timeout, callback, state);
      }

      void ICommunicationObject.EndOpen(IAsyncResult result)
      {
        this.channel.EndOpen(result);
      }

      void IDisposable.Dispose()
      {
        this.channel.Dispose();
      }

      void IOutputChannel.Send(System.ServiceModel.Channels.Message message)
      {
        this.channel.Send(message);
      }

      void IOutputChannel.Send(System.ServiceModel.Channels.Message message, TimeSpan timeout)
      {
        this.channel.Send(message, timeout);
      }

      IAsyncResult IOutputChannel.BeginSend(System.ServiceModel.Channels.Message message, AsyncCallback callback, object state)
      {
        return this.channel.BeginSend(message, callback, state);
      }

      IAsyncResult IOutputChannel.BeginSend(System.ServiceModel.Channels.Message message, TimeSpan timeout, AsyncCallback callback, object state)
      {
        return this.channel.BeginSend(message, timeout, callback, state);
      }

      void IOutputChannel.EndSend(IAsyncResult result)
      {
        this.channel.EndSend(result);
      }

      System.ServiceModel.Channels.Message IRequestChannel.Request(System.ServiceModel.Channels.Message message)
      {
        return this.channel.Request(message);
      }

      System.ServiceModel.Channels.Message IRequestChannel.Request(System.ServiceModel.Channels.Message message, TimeSpan timeout)
      {
        return this.channel.Request(message, timeout);
      }

      IAsyncResult IRequestChannel.BeginRequest(System.ServiceModel.Channels.Message message, AsyncCallback callback, object state)
      {
        return this.channel.BeginRequest(message, callback, state);
      }

      IAsyncResult IRequestChannel.BeginRequest(System.ServiceModel.Channels.Message message, TimeSpan timeout, AsyncCallback callback, object state)
      {
        return this.channel.BeginRequest(message, timeout, callback, state);
      }

      System.ServiceModel.Channels.Message IRequestChannel.EndRequest(IAsyncResult result)
      {
        return this.channel.EndRequest(result);
      }

      ServiceChannel IChannelBaseProxy.GetServiceChannel()
      {
        return this.channel;
      }

      private class MethodCallMessage : IMethodCallMessage, IMethodMessage, IMessage
      {
        private readonly object[] args;

        public object[] Args
        {
          get
          {
            return this.args;
          }
        }

        public int ArgCount
        {
          get
          {
            return this.args.Length;
          }
        }

        public LogicalCallContext LogicalCallContext
        {
          get
          {
            return (LogicalCallContext) null;
          }
        }

        public int InArgCount
        {
          get
          {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotImplementedException());
          }
        }

        public object[] InArgs
        {
          get
          {
            return this.args;
          }
        }

        public bool HasVarArgs
        {
          get
          {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotImplementedException());
          }
        }

        public MethodBase MethodBase
        {
          get
          {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotImplementedException());
          }
        }

        public string MethodName
        {
          get
          {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotImplementedException());
          }
        }

        public object MethodSignature
        {
          get
          {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotImplementedException());
          }
        }

        public string TypeName
        {
          get
          {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotImplementedException());
          }
        }

        public string Uri
        {
          get
          {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotImplementedException());
          }
        }

        public IDictionary Properties
        {
          get
          {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotImplementedException());
          }
        }

        public MethodCallMessage(object[] args)
        {
          this.args = args;
        }

        public object GetInArg(int argNum)
        {
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotImplementedException());
        }

        public string GetInArgName(int index)
        {
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotImplementedException());
        }

        public object GetArg(int argNum)
        {
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotImplementedException());
        }

        public string GetArgName(int index)
        {
          throw DiagnosticUtility.ExceptionUtility.ThrowHelperError((Exception) new NotImplementedException());
        }
      }
    }
  }
}
