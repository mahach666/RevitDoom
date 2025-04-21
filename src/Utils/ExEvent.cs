using Autodesk.Revit.UI;
using System;
using System.Threading.Tasks;

namespace RevitDoom.Utils
{

    public class ExEvent
    {
        private readonly EventHandler _handler;
        private readonly ExternalEvent _externalEvent;
        private TaskCompletionSource<object> _tcs;

        public ExEvent()
        {
            _handler = new EventHandler();
            _handler.EventCompleted += OnEventCompleted;
            _externalEvent = ExternalEvent.Create(_handler);
        }

        public Task Run(Action<UIApplication> action)
        {
            _tcs = new TaskCompletionSource<object>();
            _handler.Action = action ?? throw new ArgumentNullException(nameof(action));
            _externalEvent.Raise();
            return _tcs.Task;
        }

        private void OnEventCompleted(object sender, object result)
        {
            if (_handler.Exception == null)
                _tcs.TrySetResult(null);
            else
                _tcs.TrySetException(_handler.Exception);
        }

        private class EventHandler : IExternalEventHandler
        {
            public event EventHandler<object> EventCompleted;

            public Exception Exception { get; private set; }
            public Action<UIApplication> Action { get; set; }

            public void Execute(UIApplication app)
            {
                Exception = null;

                try
                {
                    Action(app);
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }

                EventCompleted?.Invoke(this, null);
            }

            public string GetName() => "ExEvent";
        }
    }
}