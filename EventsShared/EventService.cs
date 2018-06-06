using niwrA.CommandManager.Helpers;
using niwrA.EventManager.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace niwrA.EventManager
{
    public class EventService : IEventService
    {
        private readonly CommandManager.IDateTimeProvider _dateTimeProvider;
        public EventService()
        {
            _dateTimeProvider = new DefaultDateTimeProvider();
        }
        public EventService(CommandManager.IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }
        public void ProcessEvents(IEnumerable<IEvent> events)
        {
            var processed = new List<IEvent>();
            foreach (var e in events)
            {
                ProcessEvents(e);
            }
        }
        /// <summary>
        /// Process a single query (execute it)
        /// </summary>
        /// <param name="e"></param>
        public void ProcessEvents(IEvent e)
        {
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            e.Execute();
            e.ExecutedOn = _dateTimeProvider.GetServerDateTime();
        }
        /// <summary>
        /// Create a command of the specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T CreateEvent<T>() where T : IEvent, new()
        {
            var query = new T()
            {
            };
            query.Guid = Guid.NewGuid();
            query.CreatedOn = _dateTimeProvider.GetServerDateTime();
            return query;
        }
    }
}
