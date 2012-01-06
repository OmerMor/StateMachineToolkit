using System;

namespace Sanford.Threading
{
	public sealed partial class DelegateQueue
	{
		private enum NotificationType
		{
			None,
			BeginInvokeCompleted,
			PostCompleted
		}

		/// <summary>
		/// Implements the IAsyncResult interface for the DelegateQueue class.
		/// </summary>
		private sealed class DelegateQueueAsyncResult : AsyncResult
		{
			// The delegate to be invoked.
			private readonly Delegate method;

			// Args to be passed to the delegate.
			private readonly object[] args;

			// The object returned from the delegate.
			private object returnValue;

			// Represents a possible exception thrown by invoking the method.
			private Exception error;

			private readonly NotificationType notificationType;

			public DelegateQueueAsyncResult(
				object owner,
				Delegate method,
				object[] args,
				bool synchronously,
				NotificationType notificationType)
				: base(owner, null, null)
			{
				this.method = method;
				this.args = args;
				this.notificationType = notificationType;
			}

			public void Invoke()
			{
				try
				{
					returnValue = method.DynamicInvoke(args);
				}
				catch (Exception ex)
				{
					error = ex;
				}
				finally
				{
					Signal();
				}
			}

			public object[] GetArgs()
			{
				return args;
			}

			public object ReturnValue
			{
				get { return returnValue; }
			}

			public Exception Error
			{
				get { return error; }
				set { error = value; }
			}

			public Delegate Method
			{
				get { return method; }
			}

			public NotificationType NotificationType
			{
				get { return notificationType; }
			}
		}
	}
}